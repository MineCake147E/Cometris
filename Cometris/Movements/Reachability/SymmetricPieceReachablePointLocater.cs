using Cometris.Boards;

namespace Cometris.Movements.Reachability
{
    /// <summary>
    /// <see cref="ISymmetricPieceReachablePointLocater{TBitBoard}"/> implementation for O piece.
    /// </summary>
    /// <typeparam name="TBitBoard"></typeparam>
    public readonly struct SymmetricPieceReachablePointLocater<TBitBoard> : ISymmetricPieceReachablePointLocater<TBitBoard>
        where TBitBoard : unmanaged, IBitBoard<TBitBoard, ushort>
    {
        public static TBitBoard LocateReachablePointsFirstStep(TBitBoard spawn, TBitBoard mobilityBoard) => LocateNewReachablePoints(spawn, mobilityBoard);
        public static TBitBoard LocateNewReachablePoints(TBitBoard reached, TBitBoard mobilityBoard)
        {
            var upperReached = reached;
            // First Horizontal Movement
            upperReached = TBitBoard.FillHorizontalReachable(mobilityBoard, upperReached);
            // First Vertical Movement
            return TBitBoard.FillDropReachable(mobilityBoard, upperReached);
        }

        public static TBitBoard LocateHardDropReachablePoints(TBitBoard spawn, TBitBoard mobilityBoard) => LocateNewReachablePoints(spawn, mobilityBoard);
    }
}
