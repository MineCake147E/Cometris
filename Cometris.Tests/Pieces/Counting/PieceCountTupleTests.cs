using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cometris.Collections;
using Cometris.Pieces;
using Cometris.Pieces.Counting;

namespace Cometris.Tests.Pieces.Counting
{
    [TestFixture]
    public class PieceCountTupleTests
    {
        [Test]
        public void ConstructorCreatesCorrectly([Values] Piece piece, [Values(1, 255)] byte count)
        {
            var c = new PieceCountTuple(piece, count);
            Assert.Multiple(() =>
            {
                Assert.That(c[piece], Is.EqualTo(count));
                var k = BagPieceSet.All.Remove(piece);
                Assert.That(k.Select(a => c[a]), Is.All.Zero);
            });
        }

        [Test]
        public void AddAddsCorrectly([Values] Piece piece, [Values(1, -1)] sbyte count, [Values(1, 255)] byte background)
        {
            var c = new PieceCountTuple(background);
            c = c.Add(piece, count);
            Assert.Multiple(() =>
            {
                Assert.That(c[piece], Is.EqualTo(unchecked((byte)((byte)count + background))));
                var k = BagPieceSet.All.Remove(piece);
                Assert.That(k.Select(a => c[a]), Is.All.EqualTo(background));
            });
        }
    }
}
