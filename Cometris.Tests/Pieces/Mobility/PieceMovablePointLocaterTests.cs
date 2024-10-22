using Cometris.Boards;
using Cometris.Pieces.Mobility;

namespace Cometris.Tests.Pieces.Mobility
{
    [TestFixture(typeof(PartialBitBoard256X2))]
    [TestFixture(typeof(PartialBitBoard512))]
    public class PieceMovablePointLocaterTests<TBitBoard> where TBitBoard : unmanaged, IOperableBitBoard<TBitBoard, ushort>
    {
        [SetUp]
        public void Setup()
        {
        }

        internal static IEnumerable<TestCaseData> PieceTMobilityTestCaseSource()
        {
            yield return new TestCaseData(TBitBoard.Empty);
            yield return new TestCaseData(TBitBoard.FromBoard([0b1111101010000111, 0b1111001000000111, 0b1110001000110111, 0b1110000001000111, 0b1110111101001111, 0b1110000001011111, 0b1111001001000111, 0b1111101000100111, 0b1110001011110111, 0b1110011000000111, 0b1110111001001111, 0b1110000001011111, 0b1111001101000111, 0b1111100001100111, 0b1110001001110111, 0b1110011000000111, 0b1110111011001111, 0b1110000001011111, 0b1111000001000111, 0b1111111111100111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111], FullBitBoard.EmptyRow));
        }
        internal static IEnumerable<TestCaseData> PieceJMobilityTestCaseSource()
        {
            yield return new TestCaseData(TBitBoard.Empty);
            yield return new TestCaseData(TBitBoard.FromBoard([0b1111000011100111, 0b1111001011110111, 0b1111001001110111, 0b1110000001000111, 0b1110101001011111, 0b1111101000001111, 0b1110001011000111, 0b1110111001010111, 0b1110010001010111, 0b1110001101000111, 0b1110101000011111, 0b1111101011001111, 0b1110001001000111, 0b1110110001010111, 0b1110011101110111, 0b1110001000000111, 0b1110101011011111, 0b1111101000001111, 0b1110001000001111, 0b1110111111111111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111], FullBitBoard.EmptyRow));
        }

        internal static IEnumerable<TestCaseData> PieceLMobilityTestCaseSource()
        {
            yield return new TestCaseData(TBitBoard.Empty);
            yield return new TestCaseData(TBitBoard.FromBoard([0b1110011100001111, 0b1110111101001111, 0b1110111001001111, 0b1110001000000111, 0b1111101001010111, 0b1111000001011111, 0b1110001101000111, 0b1110101001110111, 0b1110101000100111, 0b1110001011000111, 0b1111100001010111, 0b1111001101011111, 0b1110001001000111, 0b1110101000110111, 0b1110111011100111, 0b1110000001000111, 0b1111101101010111, 0b1111000001011111, 0b1111000001000111, 0b1111111111110111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111], FullBitBoard.EmptyRow));
        }

        internal static IEnumerable<TestCaseData> PieceSMobilityTestCaseSource()
        {
            yield return new TestCaseData(TBitBoard.Empty);
            yield return new TestCaseData(TBitBoard.FromBoard([0b1110000011001111, 0b1111000001100111, 0b1110001001001111, 0b1110111010011111, 0b1110000000001111, 0b1111001001000111, 0b1110011010001111, 0b1111000001100111, 0b1110001001001111, 0b1110111010011111, 0b1110000000001111, 0b1111001001000111, 0b1110011010001111, 0b1111000000100111, 0b1110001001001111, 0b1110111010011111, 0b1110000000001111, 0b1111001000000111, 0b1110011111111111, 0b1111001111111111, 0b1110010000000111, 0b1110000000000111, 0b1110000000000111], FullBitBoard.EmptyRow));
        }

        internal static IEnumerable<TestCaseData> PieceZMobilityTestCaseSource()
        {
            yield return new TestCaseData(TBitBoard.Empty);
            yield return new TestCaseData(TBitBoard.FromBoard([0b1111001100000111, 0b1110011000001111, 0b1111001001000111, 0b1111100101110111, 0b1111000000000111, 0b1110001001001111, 0b1111000101100111, 0b1110011000001111, 0b1111001001000111, 0b1111100101110111, 0b1111000000000111, 0b1110001001001111, 0b1111000101100111, 0b1110010000001111, 0b1111001001000111, 0b1111100101110111, 0b1111000000000111, 0b1110000001001111, 0b1111111111100111, 0b1111111111001111, 0b1110000000100111, 0b1110000000000111, 0b1110000000000111], FullBitBoard.EmptyRow));
        }
        internal static IEnumerable<TestCaseData> PieceIMobilityTestCaseSource()
        {
            yield return new TestCaseData(TBitBoard.Empty);
            yield return new TestCaseData(TBitBoard.FromBoard([0b1111111100000111, 0b1111111101110111, 0b1110000001110111, 0b1110111000000111, 0b1110111011100111, 0b1110000011100111, 0b1111110000000111, 0b1111110001110111, 0b1110000001110111, 0b1110111000000111, 0b1110111011100111, 0b1110000011100111, 0b1111110000000111, 0b1111110001110111, 0b1110000001110111, 0b1110111000000111, 0b1110111011100111, 0b1110000011100111, 0b1111111000000111, 0b1111111111110111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111], FullBitBoard.EmptyRow));
        }
        internal static IEnumerable<TestCaseData> PieceOMobilityTestCaseSource()
        {
            yield return new TestCaseData(TBitBoard.Empty);
            yield return new TestCaseData(TBitBoard.FromBoard([0b1111111111100111, 0b1111111111100111, 0b1111111111001111, 0b1111111111001111, 0b1111111110011111, 0b1111111110011111, 0b1111111100111111, 0b1111111100111111, 0b1111111001111111, 0b1111111001111111, 0b1111110011111111, 0b1111110011111111, 0b1111100111111111, 0b1111100111111111, 0b1111001111111111, 0b1111001111111111, 0b1110011111111111, 0b1110011111111111, 0b1111111001111111, 0b1111111001111111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111], FullBitBoard.EmptyRow));
        }

        [TestCaseSource(nameof(PieceTMobilityTestCaseSource))]
        public void PieceTMovablePointLocaterLocatesCorrectly(TBitBoard board)
        {
            var boards = PieceTMovablePointLocater<TBitBoard>.LocateMovablePoints(board);
            Assert.Pass(BitBoardUtils.VisualizeOrientations(boards));
        }

        [TestCaseSource(nameof(PieceJMobilityTestCaseSource))]
        public void PieceJMovablePointLocaterLocatesCorrectly(TBitBoard board)
        {
            var boards = PieceJMovablePointLocater<TBitBoard>.LocateMovablePoints(board);
            Assert.Pass(BitBoardUtils.VisualizeOrientations(boards));
        }

        [TestCaseSource(nameof(PieceLMobilityTestCaseSource))]
        public void PieceLMovablePointLocaterLocatesCorrectly(TBitBoard board)
        {
            var boards = PieceLMovablePointLocater<TBitBoard>.LocateMovablePoints(board);
            Assert.Pass(BitBoardUtils.VisualizeOrientations(boards));
        }

        [TestCaseSource(nameof(PieceSMobilityTestCaseSource))]
        public void PieceSMovablePointLocaterLocatesCorrectly(TBitBoard board)
        {
            var boards = PieceSMovablePointLocater<TBitBoard>.LocateMovablePoints(board);
            Assert.Pass(BitBoardUtils.VisualizeOrientations(boards));
        }

        [TestCaseSource(nameof(PieceZMobilityTestCaseSource))]
        public void PieceZMovablePointLocaterLocatesCorrectly(TBitBoard board)
        {
            var boards = PieceZMovablePointLocater<TBitBoard>.LocateMovablePoints(board);
            Assert.Pass(BitBoardUtils.VisualizeOrientations(boards));
        }

        [TestCaseSource(nameof(PieceIMobilityTestCaseSource))]
        public void PieceIMovablePointLocaterLocatesCorrectly(TBitBoard board)
        {
            var boards = PieceIMovablePointLocater<TBitBoard>.LocateMovablePoints(board);
            Assert.Pass(BitBoardUtils.VisualizeOrientations(boards));
        }

        [TestCaseSource(nameof(PieceOMobilityTestCaseSource))]
        public void PieceOMovablePointLocaterLocatesCorrectly(TBitBoard board)
        {
            var boards = PieceOMovablePointLocater<TBitBoard>.LocateMovablePoints(board);
            Assert.Pass(BitBoardUtils.VisualizeOrientations(boards));
        }
    }
}