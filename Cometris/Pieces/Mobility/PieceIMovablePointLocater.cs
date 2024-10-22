using System.Runtime.CompilerServices;

using Cometris.Boards;

namespace Cometris.Pieces.Mobility
{
    public readonly struct PieceIMovablePointLocater<TBitBoard> : ITwoRotationSymmetricPieceMovablePointLocater<TBitBoard> where TBitBoard : unmanaged, IOperableBitBoard<TBitBoard, ushort>
    {
        public static (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) ConvertToAsymmetricMobility((TBitBoard upper, TBitBoard right) boards)
            => TBitBoard.ConvertHorizontalSymmetricToAsymmetricMobility(boards);
        public static (TBitBoard upper, TBitBoard right) MergeToTwoRotationSymmetricMobility((TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) boards)
        {
            var upper = boards.upper;
            var right = boards.right;
            upper |= boards.lower << 1;
            right |= TBitBoard.ShiftUpOneLine(boards.left, TBitBoard.Zero);
            return (upper, right);
        }
        public static (TBitBoard upper, TBitBoard right) LocateSymmetricMovablePoints(TBitBoard bitBoard)
            => LocateMovablePointsInternal(~bitBoard);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static (TBitBoard upper, TBitBoard right) LocateMovablePointsInternal(TBitBoard invertedBoard)
        {
            var upperBoard = TBitBoard.ShiftDownOneLine(invertedBoard, TBitBoard.InvertedEmpty);
            var rightBoard = invertedBoard << 1;
            var lowerBoard = TBitBoard.ShiftUpOneLine(invertedBoard, TBitBoard.Zero);
            var leftBoard = invertedBoard >> 1;
            var vertical = upperBoard & lowerBoard;
            var horizontal = leftBoard & rightBoard;
            vertical &= invertedBoard;
            horizontal &= invertedBoard;
            var rightBoard2 = invertedBoard << 2;
            var lowerBoard2 = TBitBoard.ShiftUpTwoLines(invertedBoard, TBitBoard.Zero);
            rightBoard2 &= horizontal;
            lowerBoard2 &= vertical;
            return (rightBoard2, lowerBoard2);
        }

        public static (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) LocateMovablePoints(TBitBoard bitBoard)
            => ConvertToAsymmetricMobility(LocateSymmetricMovablePoints(bitBoard));
    }
}
