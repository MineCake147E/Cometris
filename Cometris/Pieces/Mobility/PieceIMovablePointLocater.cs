using System.Runtime.CompilerServices;

using Cometris.Boards;

namespace Cometris.Pieces.Mobility
{
    public readonly struct PieceIMovablePointLocater<TBitBoard> : ITwoRotationSymmetricPieceMovablePointLocater<TBitBoard> where TBitBoard : unmanaged, IBitBoard<TBitBoard, ushort>
    {
        public static (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) ConvertToAsymmetricMobility((TBitBoard upper, TBitBoard right) boards)
            => TBitBoard.ConvertHorizontalSymmetricToAsymmetricMobility(boards);
        public static (TBitBoard upper, TBitBoard right) MergeToTwoRotationSymmetricMobility((TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) boards)
        {
            var upper = boards.upper;
            var right = boards.right;
            upper |= boards.lower << 1;
            right |= TBitBoard.ShiftUpOneLine(boards.left, TBitBoard.ZeroLine);
            return (upper, right);
        }
        public static (TBitBoard upper, TBitBoard right) LocateSymmetricMovablePoints(TBitBoard bitBoard)
            => LocateMovablePointsInternal(~bitBoard);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static (TBitBoard upper, TBitBoard right) LocateMovablePointsInternal(TBitBoard invertedBoard)
        {
            var upperBoard = TBitBoard.ShiftDownOneLine(invertedBoard, FullBitBoard.InvertedEmptyRow);
            var rightBoard = invertedBoard << 1;
            var lowerBoard = TBitBoard.ShiftUpOneLine(invertedBoard, 0);
            var leftBoard = invertedBoard >> 1;
            var vertical = upperBoard & lowerBoard;
            var horizontal = leftBoard & rightBoard;
            vertical &= invertedBoard;
            horizontal &= invertedBoard;
            var rightBoard2 = invertedBoard << 2;
            var lowerBoard2 = TBitBoard.ShiftUpTwoLines(invertedBoard, FullBitBoard.InvertedEmptyRow);
            rightBoard2 &= horizontal;
            lowerBoard2 &= vertical;
            return (rightBoard2, lowerBoard2);
        }

        public static (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) LocateMovablePoints(TBitBoard bitBoard)
            => ConvertToAsymmetricMobility(LocateSymmetricMovablePoints(bitBoard));
    }
}
