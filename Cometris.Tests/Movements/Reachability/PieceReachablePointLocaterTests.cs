using Cometris.Boards;
using Cometris.Movements;
using Cometris.Movements.Reachability;
using Cometris.Pieces.Mobility;
using Cometris.Tests.Pieces.Mobility;

namespace Cometris.Tests.Movements.Reachability
{
    [TestFixture(typeof(PartialBitBoard256X2))]
    [TestFixture(typeof(PartialBitBoard512))]
    public class PieceReachablePointLocaterTests<TBitBoard> where TBitBoard : unmanaged, IOperableBitBoard<TBitBoard, ushort>
    {
        private static IEnumerable<TestCaseData> PieceTMobilityTestCaseSource() => PieceMovablePointLocaterTests<TBitBoard>.PieceTMobilityTestCaseSource();

        private static IEnumerable<TestCaseData> PieceJMobilityTestCaseSource() => PieceMovablePointLocaterTests<TBitBoard>.PieceJMobilityTestCaseSource();

        private static IEnumerable<TestCaseData> PieceLMobilityTestCaseSource() => PieceMovablePointLocaterTests<TBitBoard>.PieceLMobilityTestCaseSource();

        private static IEnumerable<TestCaseData> PieceSMobilityTestCaseSource() => PieceMovablePointLocaterTests<TBitBoard>.PieceSMobilityTestCaseSource();

        private static IEnumerable<TestCaseData> PieceZMobilityTestCaseSource() => PieceMovablePointLocaterTests<TBitBoard>.PieceZMobilityTestCaseSource();

        private static IEnumerable<TestCaseData> PieceIMobilityTestCaseSource() => PieceMovablePointLocaterTests<TBitBoard>.PieceIMobilityTestCaseSource();

        private static IEnumerable<TestCaseData> PieceOMobilityTestCaseSource() => PieceMovablePointLocaterTests<TBitBoard>.PieceOMobilityTestCaseSource();

        [TestCaseSource(nameof(PieceTMobilityTestCaseSource))]
        public void PieceTAsymmetricPieceReachablePointLocaterLocatesCorrectly(TBitBoard board)
        {
            var mob = PieceTMovablePointLocater<TBitBoard>.LocateMovablePoints(board);
            var spawn = TBitBoard.CreateSingleLine(0x0100, 20);
            var (_, _, _, left) = MovementTestUtils.TraceAll<TBitBoard, PieceTRotatabilityLocator<TBitBoard>>(mob, spawn, board, true);
            Assert.That(TBitBoard.GetBlockAt(left[1], 2), Is.EqualTo(true));
        }

        [TestCaseSource(nameof(PieceJMobilityTestCaseSource))]
        public void PieceJAsymmetricPieceReachablePointLocaterLocatesCorrectly(TBitBoard board)
        {
            var mob = PieceJMovablePointLocater<TBitBoard>.LocateMovablePoints(board);
            var spawn = TBitBoard.Zero.WithLine(0x0100, 20);
            var (_, _, _, left) = MovementTestUtils.TraceAll<TBitBoard, PieceJLSZRotatabilityLocator<TBitBoard>>(mob, spawn, board, true);
            Assert.That(TBitBoard.GetBlockAt(left[1], 9), Is.EqualTo(true));
        }

        [TestCaseSource(nameof(PieceLMobilityTestCaseSource))]
        public void PieceLAsymmetricPieceReachablePointLocaterLocatesCorrectly(TBitBoard board)
        {
            var mob = PieceLMovablePointLocater<TBitBoard>.LocateMovablePoints(board);
            var spawn = TBitBoard.Zero.WithLine(0x0100, 20);
            var (_, right, _, _) = MovementTestUtils.TraceAll<TBitBoard, PieceJLSZRotatabilityLocator<TBitBoard>>(mob, spawn, board, true);
            Assert.That(TBitBoard.GetBlockAt(right[1], 0), Is.EqualTo(true));
        }

        [TestCaseSource(nameof(PieceSMobilityTestCaseSource))]
        public void PieceSAsymmetricPieceReachablePointLocaterLocatesCorrectly(TBitBoard board)
        {
            var mob = PieceSMovablePointLocater<TBitBoard>.LocateSymmetricMovablePoints(board);
            var spawn = TBitBoard.Zero.WithLine(0x0100, 20);
            var (_, _, lower, _) = MovementTestUtils.TraceAll<TBitBoard, PieceJLSZRotatabilityLocator<TBitBoard>>(TBitBoard.ConvertVerticalSymmetricToAsymmetricMobility(mob), spawn, board, true);
            Assert.That(TBitBoard.GetBlockAt(lower[1], 8), Is.EqualTo(true));
        }

        [TestCaseSource(nameof(PieceZMobilityTestCaseSource))]
        public void PieceZAsymmetricPieceReachablePointLocaterLocatesCorrectly(TBitBoard board)
        {
            var mob = PieceZMovablePointLocater<TBitBoard>.LocateSymmetricMovablePoints(board);
            var spawn = TBitBoard.CreateSingleLine(0x0100, 20);
            var (_, _, lower, _) = MovementTestUtils.TraceAll<TBitBoard, PieceJLSZRotatabilityLocator<TBitBoard>>(TBitBoard.ConvertVerticalSymmetricToAsymmetricMobility(mob), spawn, board, true);
            Assert.That(TBitBoard.GetBlockAt(lower[1], 1), Is.EqualTo(true));
        }

        [TestCaseSource(nameof(PieceIMobilityTestCaseSource))]
        public void PieceIAsymmetricPieceReachablePointLocaterLocatesCorrectly(TBitBoard board)
        {
            var mob = PieceIMovablePointLocater<TBitBoard>.LocateSymmetricMovablePoints(board);
            var spawn = TBitBoard.Zero.WithLine(0x0100, 20);
            var (upper, _, _, _) = MovementTestUtils.TraceAll<TBitBoard, PieceIRotatabilityLocator<TBitBoard>>(TBitBoard.ConvertHorizontalSymmetricToAsymmetricMobility(mob), spawn, board, true);
            Assert.That(TBitBoard.GetBlockAt(upper[17], 1), Is.EqualTo(true));
        }
    }
}
