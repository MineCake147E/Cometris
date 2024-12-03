using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Cometris.Collections;
using Cometris.Pieces;

namespace Cometris.Tests.Collections
{
    [TestFixture(typeof(ulong))]
    [TestFixture(typeof(uint))]
    public class CompressedValuePieceListTests<TStorage>
        where TStorage : IBinaryInteger<TStorage>, IUnsignedNumber<TStorage>
    {
        private static IEnumerable<int> CountValues => Enumerable.Range(0, CompressedValuePieceList<TStorage>.MaxCapacity + 1);

        private static IEnumerable<(int count, int sliceStart)> SliceByStartValues => CountValues.SelectMany(a => Enumerable.Range(0, a + 1).Select(b => (a, b)));

        private static IEnumerable<(int count, int sliceStart, int sliceLength)> SliceByStartLengthValues
            => SliceByStartValues.SelectMany(a => Enumerable.Range(0, a.count - a.sliceStart + 1).Select(b => (a.count, a.sliceStart, b)));

        private static IEnumerable<Piece> ValidPieces => [.. PiecesUtils.AllValidPieces];

        private static IEnumerable<Piece> GeneratePattern(int count, Piece patternOffset = Piece.Z) => Enumerable.Range(0, count).Select(a => (Piece)(((uint)a + (uint)patternOffset) % 7 + 1));

        [Test]
        public void ConstructorCreatesCorrectly([ValueSource(nameof(CountValues))] int count, [ValueSource(nameof(ValidPieces))] Piece patternOffset)
        {
            var pattern = GeneratePattern(count, patternOffset).ToArray();
            var created = new CompressedValuePieceList<TStorage>(pattern);
            Assert.That(created, Is.EqualTo(pattern));
        }

        [Test]
        public void CreateCreatesCorrectly([ValueSource(nameof(CountValues))] int count, [ValueSource(nameof(ValidPieces))] Piece patternOffset)
        {
            var pattern = GeneratePattern(count, patternOffset);
            var created = CompressedValuePieceList<TStorage>.Create(pattern);
            Assert.That(created, Is.EqualTo(pattern));
        }

        [Test]
        public void SliceByStartSlicesCorrectly([ValueSource(nameof(SliceByStartValues))] (int count, int sliceStart) args)
        {
            var pattern = GeneratePattern(args.count).ToArray();
            var created = new CompressedValuePieceList<TStorage>(pattern);
            var slicedExpected = new ArraySegment<Piece>(pattern).Slice(args.sliceStart);
            var slicedActual = created.Slice(args.sliceStart);
            Assert.That(slicedActual, Is.EqualTo(slicedExpected));
        }

        [Test]
        public void SliceByStartLengthSlicesCorrectly([ValueSource(nameof(SliceByStartLengthValues))] (int count, int sliceStart, int sliceLength) args)
        {
            var pattern = GeneratePattern(args.count).ToArray();
            var created = new CompressedValuePieceList<TStorage>(pattern);
            var slicedExpected = new ArraySegment<Piece>(pattern).Slice(args.sliceStart, args.sliceLength);
            var slicedActual = created.Slice(args.sliceStart, args.sliceLength);
            Assert.That(slicedActual, Is.EqualTo(slicedExpected));
        }
    }
}
