using Cometris.Boards;
using Cometris.Movements;
using Cometris.Movements.Reachability;
using Cometris.Pieces.Mobility;
using Cometris.Tests.Pieces.Mobility;

namespace Cometris.Tests.Movements.Reachability
{
    [TestFixture(typeof(PartialBitBoard256X2))]
    [TestFixture(typeof(PartialBitBoard512))]
    public class PieceReachablePointLocaterTests<TBitBoard> where TBitBoard : unmanaged, IBitBoard<TBitBoard, ushort>
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
            var (_, _, _, left) = TraceAll<PieceTRotatabilityLocator<TBitBoard>>(mob, spawn, board);
            Assert.That(TBitBoard.GetBlockAt(left[1], 2), Is.EqualTo(true));
        }

        [TestCaseSource(nameof(PieceJMobilityTestCaseSource))]
        public void PieceJAsymmetricPieceReachablePointLocaterLocatesCorrectly(TBitBoard board)
        {
            var mob = PieceJMovablePointLocater<TBitBoard>.LocateMovablePoints(board);
            var spawn = TBitBoard.Zero.WithLine(0x0100, 20);
            var (_, _, _, left) = TraceAll<PieceJLSZRotatabilityLocator<TBitBoard>>(mob, spawn, board);
            Assert.That(TBitBoard.GetBlockAt(left[1], 9), Is.EqualTo(true));
        }

        [TestCaseSource(nameof(PieceLMobilityTestCaseSource))]
        public void PieceLAsymmetricPieceReachablePointLocaterLocatesCorrectly(TBitBoard board)
        {
            var mob = PieceLMovablePointLocater<TBitBoard>.LocateMovablePoints(board);
            var spawn = TBitBoard.Zero.WithLine(0x0100, 20);
            var (_, right, _, _) = TraceAll<PieceJLSZRotatabilityLocator<TBitBoard>>(mob, spawn, board);
            Assert.That(TBitBoard.GetBlockAt(right[1], 0), Is.EqualTo(true));
        }

        [TestCaseSource(nameof(PieceSMobilityTestCaseSource))]
        public void PieceSAsymmetricPieceReachablePointLocaterLocatesCorrectly(TBitBoard board)
        {
            var mob = PieceSMovablePointLocater<TBitBoard>.LocateSymmetricMovablePoints(board);
            var spawn = TBitBoard.Zero.WithLine(0x0100, 20);
            var (_, _, lower, _) = TraceAll<PieceJLSZRotatabilityLocator<TBitBoard>>(TBitBoard.ConvertVerticalSymmetricToAsymmetricMobility(mob), spawn, board);
            Assert.That(TBitBoard.GetBlockAt(lower[1], 8), Is.EqualTo(true));
        }

        [TestCaseSource(nameof(PieceZMobilityTestCaseSource))]
        public void PieceZAsymmetricPieceReachablePointLocaterLocatesCorrectly(TBitBoard board)
        {
            var mob = PieceZMovablePointLocater<TBitBoard>.LocateSymmetricMovablePoints(board);
            var spawn = TBitBoard.CreateSingleLine(0x0100, 20);
            var (_, _, lower, _) = TraceAll<PieceJLSZRotatabilityLocator<TBitBoard>>(TBitBoard.ConvertVerticalSymmetricToAsymmetricMobility(mob), spawn, board);
            Assert.That(TBitBoard.GetBlockAt(lower[1], 1), Is.EqualTo(true));
        }

        [TestCaseSource(nameof(PieceIMobilityTestCaseSource))]
        public void PieceIAsymmetricPieceReachablePointLocaterLocatesCorrectly(TBitBoard board)
        {
            var mob = PieceIMovablePointLocater<TBitBoard>.LocateSymmetricMovablePoints(board);
            var spawn = TBitBoard.Zero.WithLine(0x0100, 20);
            var (upper, _, _, _) = TraceAll<PieceIRotatabilityLocator<TBitBoard>>(TBitBoard.ConvertHorizontalSymmetricToAsymmetricMobility(mob), spawn, board);
            Assert.That(TBitBoard.GetBlockAt(upper[17], 1), Is.EqualTo(true));
        }

        #region TraceAll
        private static (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) TraceAll<TRotatabilityLocator>((TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) mob, TBitBoard spawn, TBitBoard background)
            where TRotatabilityLocator : unmanaged, IRotatabilityLocator<TRotatabilityLocator, TBitBoard>
        {
            var reached = AsymmetricPieceReachablePointLocater<TBitBoard, TRotatabilityLocator>.LocateReachablePointsFirstStep(spawn, mob);
            TBitBoard diffAll;
            (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) newBoards, diff = reached;
            var steps = 0;
            Console.WriteLine($"Step {steps}");
            Console.WriteLine(BitBoardUtils.VisualizeOrientations(reached));
            do
            {
                steps++;
                newBoards = AsymmetricPieceReachablePointLocater<TBitBoard, TRotatabilityLocator>.LocateNewReachablePoints(reached, mob);
                Console.WriteLine($"\nStep {steps} Difference:");
                diff.upper = newBoards.upper & ~reached.upper;
                diff.right = newBoards.right & ~reached.right;
                diff.lower = newBoards.lower & ~reached.lower;
                diff.left = newBoards.left & ~reached.left;
                Console.WriteLine(BitBoardUtils.VisualizeOrientations(diff, background));
                reached.upper |= newBoards.upper;
                reached.right |= newBoards.right;
                reached.lower |= newBoards.lower;
                reached.left |= newBoards.left;
                diffAll = diff.upper | diff.right | (diff.lower | diff.left);
            } while (diffAll != TBitBoard.Zero);
            Console.WriteLine($"\nTotal Steps: {steps}");
            Console.WriteLine(BitBoardUtils.VisualizeOrientations(reached, background));
            return reached;
        }
        #endregion
    }
}
