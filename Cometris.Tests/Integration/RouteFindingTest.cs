using Cometris.Boards;
using Cometris.Pieces;

namespace Cometris.Tests.Integration
{
    public class RouteFindingTest
    {
        internal static IEnumerable<TestCaseData> FindPCRouteTestCaseSource()
        {
            yield return new TestCaseData(PartialBitBoard256X2.Empty, new[] { Piece.L, Piece.S, Piece.T, Piece.O, Piece.Z, Piece.J, Piece.I, Piece.T, Piece.J, Piece.I });
            yield return new TestCaseData(PartialBitBoard256X2.Empty, new[] { Piece.L, Piece.S, Piece.J, Piece.Z, Piece.T, Piece.O, Piece.I, Piece.J, Piece.T, Piece.I });
        }
        [TestCaseSource(nameof(FindPCRouteTestCaseSource))]
        public void FindsPCRoute<TBitBoard>(TBitBoard start, params Piece[] pieces)
            where TBitBoard : unmanaged, IOperableBitBoard<TBitBoard, ushort>
        {

        }
    }
}
