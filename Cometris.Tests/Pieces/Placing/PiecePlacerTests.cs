using Cometris.Boards;
using Cometris.Pieces.Mobility;
using Cometris.Pieces.Placing;

namespace Cometris.Tests.Pieces.Placing
{
    [TestFixture(typeof(PartialBitBoard256X2))]
    [TestFixture(typeof(PartialBitBoard512))]
    public class PiecePlacerTests<TBitBoard>
        where TBitBoard : unmanaged, IOperableBitBoard<TBitBoard, ushort>
    {
        internal static IEnumerable<TestCaseData> BitBoardTestCaseSource()
        {
            yield return new TestCaseData(TBitBoard.Zero);
        }
        #region Pieces
        [Test]
        public void PieceTPlacerPlacesCorrectly()
            => AssertAsymmetricPiecePlacerPlacesCorrectly<PieceTPlacer<TBitBoard>, PieceTMovablePointLocater<TBitBoard>>(7, 20);

        [Test]
        public void PieceJPlacerPlacesCorrectly()
            => AssertAsymmetricPiecePlacerPlacesCorrectly<PieceJPlacer<TBitBoard>, PieceJMovablePointLocater<TBitBoard>>(7, 20);

        [Test]
        public void PieceLPlacerPlacesCorrectly()
            => AssertAsymmetricPiecePlacerPlacesCorrectly<PieceLPlacer<TBitBoard>, PieceLMovablePointLocater<TBitBoard>>(7, 20);

        [Test]
        public void PieceIPlacerPlacesCorrectly()
            => AssertTwoRotationSymmetricPiecePlacerPlacesCorrectly<PieceIPlacer<TBitBoard>, PieceIMovablePointLocater<TBitBoard>>(7, 20);

        [Test]
        public void PieceSPlacerPlacesCorrectly()
            => AssertTwoRotationSymmetricPiecePlacerPlacesCorrectly<PieceSPlacer<TBitBoard>, PieceSMovablePointLocater<TBitBoard>>(7, 20);

        [Test]
        public void PieceZPlacerPlacesCorrectly()
            => AssertTwoRotationSymmetricPiecePlacerPlacesCorrectly<PieceZPlacer<TBitBoard>, PieceZMovablePointLocater<TBitBoard>>(7, 20);

        [Test]
        public void PieceOPlacerPlacesCorrectly()
            => AssertSymmetricPiecePlacerPlacesCorrectly<PieceOPlacer<TBitBoard>, PieceOMovablePointLocater<TBitBoard>>(7, 20);

        #endregion

        #region Asymmetric

        private static void AssertAsymmetricPiecePlacerPlacesCorrectly<TPiecePlacer, TMovablePointLocater>(int x, int y)
            where TPiecePlacer : IPiecePlacer<TBitBoard>
            where TMovablePointLocater : IAsymmetricPieceMovablePointLocater<TBitBoard>
            => Assert.Multiple(() =>
            {
                var boardPoint = TBitBoard.CreateSingleBlock(x, y);
                AssertUpperAsymmetric<TPiecePlacer, TMovablePointLocater>(x, y, boardPoint);
                AssertRightAsymmetric<TPiecePlacer, TMovablePointLocater>(x, y, boardPoint);
                AssertLowerAsymmetric<TPiecePlacer, TMovablePointLocater>(x, y, boardPoint);
                AssertLeftAsymmetric<TPiecePlacer, TMovablePointLocater>(x, y, boardPoint);
            });

        private static void AssertUpperAsymmetric<TPiecePlacer, TMovablePointLocater>(int x, int y, TBitBoard boardPoint)
            where TPiecePlacer : IPiecePlacer<TBitBoard>
            where TMovablePointLocater : IAsymmetricPieceMovablePointLocater<TBitBoard>
        {
            var board = TPiecePlacer.PlaceUp(x, y);
            Console.WriteLine($"Up:\n{board}");
            var (upper, right, lower, left) = TMovablePointLocater.LocateMovablePoints(~board);
            Assert.Multiple(() =>
            {
                Assert.That(upper, Is.EqualTo(boardPoint));
                Assert.That(right, Is.EqualTo(TBitBoard.Zero));
                Assert.That(lower, Is.EqualTo(TBitBoard.Zero));
                Assert.That(left, Is.EqualTo(TBitBoard.Zero));
            });
        }

        private static void AssertRightAsymmetric<TPiecePlacer, TMovablePointLocater>(int x, int y, TBitBoard boardPoint)
            where TPiecePlacer : IPiecePlacer<TBitBoard>
            where TMovablePointLocater : IAsymmetricPieceMovablePointLocater<TBitBoard>
        {
            var board = TPiecePlacer.PlaceRight(x, y);
            Console.WriteLine($"Right:\n{board}");
            var (upper, right, lower, left) = TMovablePointLocater.LocateMovablePoints(~board);
            Assert.Multiple(() =>
            {
                Assert.That(upper, Is.EqualTo(TBitBoard.Zero));
                Assert.That(right, Is.EqualTo(boardPoint));
                Assert.That(lower, Is.EqualTo(TBitBoard.Zero));
                Assert.That(left, Is.EqualTo(TBitBoard.Zero));
            });
        }

        private static void AssertLowerAsymmetric<TPiecePlacer, TMovablePointLocater>(int x, int y, TBitBoard boardPoint)
            where TPiecePlacer : IPiecePlacer<TBitBoard>
            where TMovablePointLocater : IAsymmetricPieceMovablePointLocater<TBitBoard>
        {
            var board = TPiecePlacer.PlaceDown(x, y);
            Console.WriteLine($"Down:\n{board}");
            var (upper, right, lower, left) = TMovablePointLocater.LocateMovablePoints(~board);
            Assert.Multiple(() =>
            {
                Assert.That(upper, Is.EqualTo(TBitBoard.Zero));
                Assert.That(right, Is.EqualTo(TBitBoard.Zero));
                Assert.That(lower, Is.EqualTo(boardPoint));
                Assert.That(left, Is.EqualTo(TBitBoard.Zero));
            });
        }

        private static void AssertLeftAsymmetric<TPiecePlacer, TMovablePointLocater>(int x, int y, TBitBoard boardPoint)
            where TPiecePlacer : IPiecePlacer<TBitBoard>
            where TMovablePointLocater : IAsymmetricPieceMovablePointLocater<TBitBoard>
        {
            var board = TPiecePlacer.PlaceLeft(x, y);
            Console.WriteLine($"Left:\n{board}");
            var (upper, right, lower, left) = TMovablePointLocater.LocateMovablePoints(~board);
            Assert.Multiple(() =>
            {
                Assert.That(upper, Is.EqualTo(TBitBoard.Zero));
                Assert.That(right, Is.EqualTo(TBitBoard.Zero));
                Assert.That(lower, Is.EqualTo(TBitBoard.Zero));
                Assert.That(left, Is.EqualTo(boardPoint));
            });
        }

        #endregion

        #region TwoRotationSymmetric
        private static void AssertTwoRotationSymmetricPiecePlacerPlacesCorrectly<TPiecePlacer, TMovablePointLocater>(int x, int y)
            where TPiecePlacer : IPiecePlacer<TBitBoard>
            where TMovablePointLocater : ITwoRotationSymmetricPieceMovablePointLocater<TBitBoard>
            => Assert.Multiple(() =>
            {
                var boardPoint = TBitBoard.CreateSingleBlock(x, y);
                AssertUpperTwoRotationSymmetric<TPiecePlacer, TMovablePointLocater>(x, y, boardPoint);
                AssertRightTwoRotationSymmetric<TPiecePlacer, TMovablePointLocater>(x, y, boardPoint);
                AssertLowerTwoRotationSymmetric<TPiecePlacer, TMovablePointLocater>(x, y);
                AssertLeftTwoRotationSymmetric<TPiecePlacer, TMovablePointLocater>(x, y);
            });

        private static void AssertUpperTwoRotationSymmetric<TPiecePlacer, TMovablePointLocater>(int x, int y, TBitBoard boardPoint)
            where TPiecePlacer : IPiecePlacer<TBitBoard>
            where TMovablePointLocater : ITwoRotationSymmetricPieceMovablePointLocater<TBitBoard>
        {
            var board = TPiecePlacer.PlaceUp(x, y);
            Console.WriteLine($"Up:\n{board}");
            var mobility = TMovablePointLocater.ConvertToAsymmetricMobility(TMovablePointLocater.LocateSymmetricMovablePoints(~board));
            (var upper, var right, var lower, var left) = TMovablePointLocater.ConvertToAsymmetricMobility((boardPoint, TBitBoard.Zero));
            Assert.Multiple(() =>
            {
                Assert.That(mobility.upper, Is.EqualTo(upper));
                Assert.That(mobility.right, Is.EqualTo(right));
                Assert.That(mobility.lower, Is.EqualTo(lower));
                Assert.That(mobility.left, Is.EqualTo(left));
            });
        }

        private static void AssertRightTwoRotationSymmetric<TPiecePlacer, TMovablePointLocater>(int x, int y, TBitBoard boardPoint)
            where TPiecePlacer : IPiecePlacer<TBitBoard>
            where TMovablePointLocater : ITwoRotationSymmetricPieceMovablePointLocater<TBitBoard>
        {
            var board = TPiecePlacer.PlaceRight(x, y);
            Console.WriteLine($"Right:\n{board}");
            var mobility = TMovablePointLocater.ConvertToAsymmetricMobility(TMovablePointLocater.LocateSymmetricMovablePoints(~board));
            (var upper, var right, var lower, var left) = TMovablePointLocater.ConvertToAsymmetricMobility((TBitBoard.Zero, boardPoint));
            Assert.Multiple(() =>
            {
                Assert.That(mobility.upper, Is.EqualTo(upper));
                Assert.That(mobility.right, Is.EqualTo(right));
                Assert.That(mobility.lower, Is.EqualTo(lower));
                Assert.That(mobility.left, Is.EqualTo(left));
            });
        }

        private static void AssertLowerTwoRotationSymmetric<TPiecePlacer, TMovablePointLocater>(int x, int y)
            where TPiecePlacer : IPiecePlacer<TBitBoard>
            where TMovablePointLocater : ITwoRotationSymmetricPieceMovablePointLocater<TBitBoard>
        {
            var board = TPiecePlacer.PlaceDown(x, y);
            Console.WriteLine($"Down:\n{board}");
            var boardUp = TPiecePlacer.PlaceUp(x, y);
            (var _, var _, var mirrored, var _) = TMovablePointLocater.ConvertToAsymmetricMobility((board, TBitBoard.Zero));
            Assert.That(mirrored, Is.EqualTo(boardUp));
        }

        private static void AssertLeftTwoRotationSymmetric<TPiecePlacer, TMovablePointLocater>(int x, int y)
            where TPiecePlacer : IPiecePlacer<TBitBoard>
            where TMovablePointLocater : ITwoRotationSymmetricPieceMovablePointLocater<TBitBoard>
        {
            var board = TPiecePlacer.PlaceLeft(x, y);
            Console.WriteLine($"Left:\n{board}");
            var boardRight = TPiecePlacer.PlaceRight(x, y);
            (var _, var _, var _, var mirrored) = TMovablePointLocater.ConvertToAsymmetricMobility((TBitBoard.Zero, board));
            Assert.That(mirrored, Is.EqualTo(boardRight));
        }
        #endregion

        #region Symmetric
        private static void AssertSymmetricPiecePlacerPlacesCorrectly<TPiecePlacer, TMovablePointLocater>(int x, int y)
            where TPiecePlacer : IPiecePlacer<TBitBoard>
            where TMovablePointLocater : ISymmetricPieceMovablePointLocater<TBitBoard>
            => Assert.Multiple(() =>
            {
                var boardPoint = TBitBoard.CreateSingleBlock(x, y);
                AssertUpperSymmetric<TPiecePlacer, TMovablePointLocater>(x, y, boardPoint);
                AssertRightSymmetric<TPiecePlacer, TMovablePointLocater>(x, y);
                AssertLowerSymmetric<TPiecePlacer, TMovablePointLocater>(x, y);
                AssertLeftSymmetric<TPiecePlacer, TMovablePointLocater>(x, y);
            });

        private static void AssertUpperSymmetric<TPiecePlacer, TMovablePointLocater>(int x, int y, TBitBoard boardPoint)
            where TPiecePlacer : IPiecePlacer<TBitBoard>
            where TMovablePointLocater : ISymmetricPieceMovablePointLocater<TBitBoard>
        {
            var board = TPiecePlacer.PlaceUp(x, y);
            Console.WriteLine($"Up:\n{board}");
            var mobility = TMovablePointLocater.ConvertToAsymmetricMobility(TMovablePointLocater.LocateSymmetricMovablePoints(~board));
            (var upper, var right, var lower, var left) = TMovablePointLocater.ConvertToAsymmetricMobility(boardPoint);
            Assert.Multiple(() =>
            {
                Assert.That(mobility.upper, Is.EqualTo(upper));
                Assert.That(mobility.right, Is.EqualTo(right));
                Assert.That(mobility.lower, Is.EqualTo(lower));
                Assert.That(mobility.left, Is.EqualTo(left));
            });
        }

        private static void AssertRightSymmetric<TPiecePlacer, TMovablePointLocater>(int x, int y)
            where TPiecePlacer : IPiecePlacer<TBitBoard>
            where TMovablePointLocater : ISymmetricPieceMovablePointLocater<TBitBoard>
        {
            var board = TPiecePlacer.PlaceRight(x, y);
            Console.WriteLine($"Right:\n{board}");
            var boardUp = TPiecePlacer.PlaceUp(x, y);
            (var _, var mirrored, var _, var _) = TMovablePointLocater.ConvertToAsymmetricMobility(board);
            Assert.That(mirrored, Is.EqualTo(boardUp));
        }

        private static void AssertLowerSymmetric<TPiecePlacer, TMovablePointLocater>(int x, int y)
            where TPiecePlacer : IPiecePlacer<TBitBoard>
            where TMovablePointLocater : ISymmetricPieceMovablePointLocater<TBitBoard>
        {
            var board = TPiecePlacer.PlaceDown(x, y);
            Console.WriteLine($"Down:\n{board}");
            var boardUp = TPiecePlacer.PlaceUp(x, y);
            (var _, var _, var mirrored, var _) = TMovablePointLocater.ConvertToAsymmetricMobility(board);
            Assert.That(mirrored, Is.EqualTo(boardUp));
        }

        private static void AssertLeftSymmetric<TPiecePlacer, TMovablePointLocater>(int x, int y)
            where TPiecePlacer : IPiecePlacer<TBitBoard>
            where TMovablePointLocater : ISymmetricPieceMovablePointLocater<TBitBoard>
        {
            var board = TPiecePlacer.PlaceLeft(x, y);
            Console.WriteLine($"Left:\n{board}");
            var boardUp = TPiecePlacer.PlaceUp(x, y);
            (var _, var _, var _, var mirrored) = TMovablePointLocater.ConvertToAsymmetricMobility(board);
            Assert.That(mirrored, Is.EqualTo(boardUp));
        }
        #endregion
    }
}
