using System.Runtime.CompilerServices;

using Cometris.Boards;

namespace Cometris.Pieces.Mobility
{
    public readonly struct PieceTMovablePointLocater<TBitBoard> : IAsymmetricPieceMovablePointLocater<TBitBoard> where TBitBoard : unmanaged, IBitBoard<TBitBoard, ushort>
    {
        public static (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) LocateMovablePoints(TBitBoard bitBoard)
            => LocateMovablePointsInternal(~bitBoard);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) LocateMovablePointsInternal(TBitBoard invertedBoard)
        {
            var upperBoard = invertedBoard & TBitBoard.ShiftDownOneLine(invertedBoard, FullBitBoard.InvertedEmptyRow);
            var rightBoard = invertedBoard & (invertedBoard << 1);
            var lowerBoard = TBitBoard.ShiftUpOneLine(invertedBoard, 0);
            var leftBoard = invertedBoard >> 1;
            var vertical = upperBoard & lowerBoard;
            var horizontal = leftBoard & rightBoard;
            upperBoard &= horizontal;
            rightBoard &= vertical;
            lowerBoard &= horizontal;
            leftBoard &= vertical;
            return (upperBoard, rightBoard, lowerBoard, leftBoard);
        }
    }
}
