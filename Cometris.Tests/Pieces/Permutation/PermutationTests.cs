using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

using Cometris.Collections;
using Cometris.Pieces;
using Cometris.Pieces.Permutation;

namespace Cometris.Tests.Pieces.Permutation
{
    [TestFixture]
    public class PermutationTests
    {
        private static void PermuteBag<T>(Span<T> bag, ushort t)
        {
            uint k = t;
            if (k >= 5040 || bag.Length < 7) return;
            _ = bag[6];
            (k, var y0) = uint.DivRem(k, 7);
            (k, var y1) = uint.DivRem(k, 6);
            (k, var y2) = uint.DivRem(k, 5);
            (k, var y3) = uint.DivRem(k, 4);
            (var y5, var y4) = uint.DivRem(k, 3);
            PermuteStep(bag, (int)y0);
            PermuteStep(bag.Slice(1), (int)y1);
            PermuteStep(bag.Slice(2), (int)y2);
            PermuteStep(bag.Slice(3), (int)y3);
            PermuteStep(bag.Slice(4), (int)y4);
            PermuteStep(bag.Slice(5), (int)y5);
        }

        private static void PermuteStep<T>(Span<T> bag, int j)
        {
            var e = bag[j];
            bag.Slice(0, j).CopyTo(bag.Slice(1));
            bag[0] = e;
        }

        [Test]
        public void CreatePermutationCreatesCorrectly()
        {
            var pieces = PiecesUtils.AllValidPieces;
            Piece[] bag = [default, default, default, default, default, default, default];
            for (uint i = 0; i < 5040; i++)
            {
                var id = (ushort)i;
                pieces.CopyTo(bag);
                PermuteBag(bag, id);
                var k = PiecePermutationUtils.CreatePermutation<uint>(id);
                Assert.That(k, Is.EqualTo(bag), $"Testing {id}th permutation");
            }
        }

        [Test]
        public void CalculatePermutationCalculatesCorrectly()
        {
            var pieces = PiecesUtils.AllValidPieces;
            Piece[] bag = [default, default, default, default, default, default, default];
            var bagSpan = bag.AsSpan();
            for (uint i = 0; i < 5040; i++)
            {
                var id = (ushort)i;
                pieces.CopyTo(bagSpan);
                PermuteBag(bagSpan, id);
                var k = PieceListUtils.Create(PiecePermutationUtils.CalculatePermutation(id));
                Assert.That(k, Is.EqualTo(bag), $"Testing {id}th permutation");
            }
        }
    }
}
