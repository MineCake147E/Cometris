using System.Runtime.CompilerServices;

using Cometris.Boards;

namespace Cometris.Pieces.Mobility
{
    public readonly struct PieceZMovablePointLocater<TBitBoard> : ITwoRotationSymmetricPieceMovablePointLocater<TBitBoard> where TBitBoard : unmanaged, IBitBoard<TBitBoard, ushort>
    {
        public static (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) ConvertToAsymmetricMobility((TBitBoard upper, TBitBoard right) boards)
            => TBitBoard.ConvertVerticalSymmetricToAsymmetricMobility(boards);
        public static (TBitBoard upper, TBitBoard right) MergeToTwoRotationSymmetricMobility((TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) boards)
            => TBitBoard.MergeAsymmetricToVerticalSymmetricMobility(boards);
        public static (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) LocateMovablePoints(TBitBoard bitBoard)
            => ConvertToAsymmetricMobility(LocateSymmetricMovablePoints(bitBoard));
        public static (TBitBoard upper, TBitBoard right) LocateSymmetricMovablePoints(TBitBoard bitBoard)
            => LocateMovablePointsInternal(~bitBoard);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static (TBitBoard upper, TBitBoard right) LocateMovablePointsInternal(TBitBoard invertedBoard)
        {
            var rightBoard = invertedBoard << 1;
            var horizontal = rightBoard & invertedBoard;
            var upperBoard = TBitBoard.ShiftDownOneLine(invertedBoard, FullBitBoard.InvertedEmptyRow);
            var lowerBoard = TBitBoard.ShiftUpOneLine(invertedBoard, 0);
            var upperRightBoard = upperBoard << 1;
            var upperLeftBoard = upperBoard >> 1;
            upperBoard &= horizontal;
            upperRightBoard &= horizontal;
            upperBoard &= upperLeftBoard;
            upperRightBoard &= lowerBoard;
            return (upperBoard, upperRightBoard);
        }
    }
}
