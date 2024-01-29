using System.Runtime.CompilerServices;

using Cometris.Boards;

namespace Cometris.Pieces.Mobility
{
    public readonly struct PieceOMovablePointLocater<TBitBoard> : ISymmetricPieceMovablePointLocater<TBitBoard> where TBitBoard : unmanaged, IBitBoard<TBitBoard, ushort>
    {
        public static (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) ConvertToAsymmetricMobility(TBitBoard board)
            => TBitBoard.ConvertOPieceToAsymmetricMobility(board);
        public static (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) LocateMovablePoints(TBitBoard bitBoard)
            => ConvertToAsymmetricMobility(LocateSymmetricMovablePoints(bitBoard));
        public static TBitBoard LocateSymmetricMovablePoints(TBitBoard bitBoard)
            => LocateMovablePointsInternal(~bitBoard);
        public static TBitBoard MergeToSymmetricMobility((TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) board)
            => TBitBoard.MergeAsymmetricToOPieceMobility(board);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static TBitBoard LocateMovablePointsInternal(TBitBoard invertedBoard)
        {
            var upperBoard = TBitBoard.ShiftDownOneLine(invertedBoard, FullBitBoard.InvertedEmptyRow);
            var upperRightBoard = upperBoard << 1;
            var rightBoard = invertedBoard << 1;
            var vertical = upperBoard & invertedBoard;
            rightBoard &= upperRightBoard;
            rightBoard &= vertical;
            return rightBoard;
        }
    }
}
