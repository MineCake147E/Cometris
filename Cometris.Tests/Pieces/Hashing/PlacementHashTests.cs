using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Cometris.Boards;
using Cometris.Pieces;
using Cometris.Pieces.Hashing;

using MikoMino;

namespace Cometris.Tests.Pieces.Hashing
{
    [TestFixture]
    public class PlacementHashTests
    {
        public static IEnumerable<TestCaseData> PieceSource => Enumerable.Range(0, 8).Select(a => new TestCaseData((Piece)a));
        public static IEnumerable<TestCaseData> BitsSource => Enumerable.Range(16, 64 - 16).Select(a => new TestCaseData(a));

        private static IEnumerable<CompressedPiecePlacement> GetWholePiecePlacementSet() => Enumerable.Range(0, 65536).Select(a => new CompressedPiecePlacement((ushort)a));

        private static IEnumerable<CompressedPiecePlacement> GetNormalPieceAngleCombinations()
        {
            List<Angle> angles = [Angle.Up, Angle.Right, Angle.Down, Angle.Left];
            List<Piece> pieces = [Piece.T, Piece.J, Piece.L];
            List<Angle> angles2 = [Angle.Up, Angle.Right];
            List<Piece> pieces2 = [Piece.I, Piece.S, Piece.Z];
            return pieces.SelectMany(a => angles.Select(b => new CompressedPiecePlacement(default, a, b)))
                .Concat(pieces2.SelectMany(a => angles2.Select(b => new CompressedPiecePlacement(default, a, b))))
                .Append(new CompressedPiecePlacement(default, Piece.O, Angle.Up));
        }

        private static IEnumerable<ushort> GetAllPositions() => Enumerable.Range(0, 1024).Select(a => (ushort)a);

        [Test]
        public void NoHashCollidesUInt64()
        {
            var originalSetEnumerable = GetWholePiecePlacementSet();
            AssertCollision(originalSetEnumerable, CompressedPiecePlacement.CalculateHashCode);
        }

        [Test]
        public void NoHashCollidesUInt64Fallback()
        {
            var originalSetEnumerable = GetWholePiecePlacementSet();
            AssertCollision(originalSetEnumerable, a => CompressedPiecePlacement.CalculateKeyedHashCodeFallback(a, default));
        }

        [TestCaseSource(nameof(BitsSource))]
        public void NoHashCollidesPowerOfTwoRange(int bits)
        {
            var originalSetEnumerable = GetWholePiecePlacementSet();
            AssertCollision(originalSetEnumerable, a => PlacementHash.CalculateRangeLimitedHash(a.CalculateHashCode(), 1ul << bits));
        }

        [TestCaseSource(nameof(BitsSource))]
        public void NoHashCollidesPowerOfTwoRangeFallback(int bits)
        {
            var originalSetEnumerable = GetWholePiecePlacementSet();
            AssertCollision(originalSetEnumerable, a => PlacementHash.CalculateRangeLimitedHash(CompressedPiecePlacement.CalculateKeyedHashCodeFallback(a, default), 1ul << bits));
        }

        [Test]
        public void NoDistinct2ChainedHashCollides()
        {
            var pac = GetNormalPieceAngleCombinations();
            var pos = GetAllPositions();
            var pap = pac.SelectMany(a => pos.Select(b => a.WithPosition(b).Value)).Order().ToArray();
            var comb = pap.SkipLast(1)
                .SelectMany((p0, i) => pap.Skip(i + 1).Select(p1 => (p0: new CompressedPiecePlacement(p0), p1: new CompressedPiecePlacement(p1))));
            AssertDistinct(comb, c => c.p0.CalculateHashCode() ^ c.p1.CalculateHashCode());
        }

        [Test]
        public void NoDistinctChainedHashCollides()
        {
            var pac = GetNormalPieceAngleCombinations();
            var pos = GetAllPositions();
            var pap = pac.SelectMany(a => pos.Select(b => a.WithPosition(b).Value)).Order().Select(a => new CompressedPiecePlacement(a)).ToArray();
            var pas = new ArraySegment<CompressedPiecePlacement>(pap);
            var comb = pas.Slice(0, pas.Count - 1)
                .SelectMany((p0, i) => pas.Slice(i + 1).Select(p1 => (p0, p1)));
            AssertDistinct2(comb, c => c.p0.CalculateHashCode() ^ c.p1.CalculateHashCode());
        }

        [TestCase(256)]
        [TestCase(1 << 26)]
        public void ZipAdjacentWorksCorrectly(int count)
        {
            var sw = new Stopwatch();
            var na = Enumerable.Range(0, count).ToArray();
            sw.Start();
            RandomNumberGenerator.Shuffle(na.AsSpan());
            sw.Stop();
            Console.WriteLine($"Shuffle finished in {sw.Elapsed}");
            var ns = na.AsParallel().AsUnordered().OrderBy(a => a);
            //var nl = ns.ToList();
            //var exp = nl.Zip(nl.Skip(1));
            var act = ns.ZipAdjacent();
            Assert.Multiple(() =>
            {
                Assert.That(act.Count(), Is.EqualTo(count - 1));
                var filtered = act.Where(a => a.Second - a.First != 1);
                Assert.That(filtered.Any(), Is.False);
                Console.WriteLine(string.Join(Environment.NewLine, filtered.Take(256)));
            });
        }

        [TestCase(256)]
        [TestCase(1 << 27)]
        public void AssertDistinct2WorksCorrectly(int count)
        {
            var sw = new Stopwatch();
            var na = Enumerable.Range(0, count).Append(8).ToArray();
            sw.Start();
            RandomNumberGenerator.Shuffle(na.AsSpan());
            sw.Stop();
            Console.WriteLine($"Shuffle finished in {sw.Elapsed}");
            var ns = na.AsParallel().AsOrdered().OrderBy(HashCode.Combine);
            //var nl = ns.ToList();
            //var exp = nl.Zip(nl.Skip(1));
            var zipped = ns.ZipAdjacent().AsUnordered();
            var colliding = zipped.Any(b => HashCode.Combine(b.First) == HashCode.Combine(b.Second));
            Console.WriteLine(zipped.Count());
            if (colliding)
            {
                var collisions = zipped.Where(b => HashCode.Combine(b.First) == HashCode.Combine(b.Second)).Take(10).Select(a => $"{a.First} : {HashCode.Combine(a.First)}, {a.Second} : {HashCode.Combine(a.Second)}");
                Console.WriteLine($"Colliding Elements: {string.Join(Environment.NewLine, collisions)}");
            }
            else if (count < 513)
            {
                Console.WriteLine(string.Join(Environment.NewLine, ns.Select(a => $"{a} : {HashCode.Combine(a)}")));
                Console.WriteLine();
                Console.WriteLine(string.Join(Environment.NewLine, zipped.Select(a => $"{a.First} : {HashCode.Combine(a.First)}, {a.Second} : {HashCode.Combine(a.Second)}")));
            }
            Assert.That(colliding, Is.True, () => $"Number of Collision: {zipped.Count(b => HashCode.Combine(b.First) == HashCode.Combine(b.Second))}");
        }

        private sealed class DistinctHashEqualityComparer<T> : IEqualityComparer<(ulong hash, T item)>
        {
            public bool Equals((ulong hash, T item) x, (ulong hash, T item) y) => x.hash == y.hash;
            public int GetHashCode([DisallowNull] (ulong hash, T item) obj) => obj.hash.GetHashCode();
        }

        private static void AssertDistinct<T>(IEnumerable<T> hashedPair, Func<T, ulong> hash)
        {
            var ordered = hashedPair.AsParallel().OrderBy(hash).ToList();
            var zipped = ordered.Zip(ordered.Skip(1)).AsParallel().AsUnordered();
            var colliding = zipped.Any(b => hash(b.First) == hash(b.Second));
            Console.WriteLine(zipped.Count());
            if (colliding)
            {
                var collisions = zipped.Where(b => hash(b.First) == hash(b.Second)).Take(10).Select(a => $"{a.First} : {hash(a.First)}, {a.Second} : {hash(a.Second)}");
                Console.WriteLine($"Colliding Elements: {string.Join(Environment.NewLine, collisions)}");
            }
            Assert.That(colliding, Is.False, () => $"Number of Collision: {zipped.Count(b => hash(b.First) == hash(b.Second))}");
        }

        private static void AssertDistinct2<T>(IEnumerable<T> hashedPair, Func<T, ulong> hash)
        {
            var ordered = hashedPair.AsParallel().OrderBy(hash);
            var zipped = ordered.ZipAdjacent();
            var colliding = zipped.Any(b => hash(b.First) == hash(b.Second));
            Console.WriteLine(zipped.Count());
            if (colliding)
            {
                var collisions = zipped.Where(b => hash(b.First) == hash(b.Second)).Take(10).Select(a => $"{a.First} : {hash(a.First)}, {a.Second} : {hash(a.Second)}");
                Console.WriteLine($"Colliding Elements: {string.Join(Environment.NewLine, collisions)}");
            }
            Assert.That(colliding, Is.False, () => $"Number of Collision: {zipped.Count(b => hash(b.First) == hash(b.Second))}");
        }

        private static void AssertCollision<T>(IEnumerable<T> hashedPair, Func<T, ulong> hash)
        {
            var groups = hashedPair.GroupBy(hash);
            var colliding = groups.Any(a => a.Skip(1).Any());
            if (colliding)
            {
                var collisions = groups.Where(a => a.Skip(1).Any()).OrderByDescending(a => a.Count()).Take(64).Select(a => $"0x{ConvertToSeparatedHexadecimal(a.Key, 4)}: {string.Join(", ", a.Select(b => b?.ToString()).OfType<string>())}");
                Console.WriteLine($"Colliding Elements: {Environment.NewLine}{string.Join(Environment.NewLine, collisions)}");
            }
            Assert.That(colliding, Is.False, () => $"Number of Collision Groups: {groups.Count(a => a.Skip(1).Any())}");
        }

        private static StringBuilder ConvertToSeparatedHexadecimal<T>(T value, int words) where T : IBinaryInteger<T>
        {
            var sb = new StringBuilder(words * 4 + words - 1);
            for (int i = words - 1; i >= 1; i--)
            {
                var s = ushort.CreateTruncating(value >> (i * 16));
                _ = sb.Append($"{s:x4}_");
            }
            var sl = ushort.CreateTruncating(value);
            _ = sb.Append($"{sl:x4}");
            return sb;
        }

        private static void AssertDistinctCollision<T0, T1>(IEnumerable<T0> hashedPair, Func<T0, ulong> hash, Func<T0, T1> keySelector)
        {
            var groups = hashedPair.GroupBy(hash);
            var collision = groups.Where(a => a.DistinctBy(keySelector).Count() > 1);
            Assert.That(collision, Is.Empty, () => $"Number of Collision: {collision.Count()}");
        }
    }
}
