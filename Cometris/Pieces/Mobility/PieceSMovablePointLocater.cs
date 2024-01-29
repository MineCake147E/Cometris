using System;
using System.Runtime.CompilerServices;

using Cometris.Boards;

namespace Cometris.Pieces.Mobility
{
    public readonly struct PieceSMovablePointLocater<TBitBoard> : ITwoRotationSymmetricPieceMovablePointLocater<TBitBoard> where TBitBoard : unmanaged, IBitBoard<TBitBoard, ushort>
    {
        public static (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) ConvertToAsymmetricMobility((TBitBoard upper, TBitBoard right) boards)
            => TBitBoard.ConvertVerticalSymmetricToAsymmetricMobility(boards);
        public static (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) LocateMovablePoints(TBitBoard bitBoard)
            => ConvertToAsymmetricMobility(LocateSymmetricMovablePoints(bitBoard));
        public static (TBitBoard upper, TBitBoard right) LocateSymmetricMovablePoints(TBitBoard bitBoard)
            => LocateMovablePointsInternal(~bitBoard);
        public static (TBitBoard upper, TBitBoard right) MergeToTwoRotationSymmetricMobility((TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) boards) => throw new NotImplementedException();

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static (TBitBoard upper, TBitBoard right) LocateMovablePointsInternal(TBitBoard invertedBoard)
        {
            var upperBoard = TBitBoard.ShiftDownOneLine(invertedBoard, FullBitBoard.InvertedEmptyRow);
            var lowerRightBoard = TBitBoard.ShiftUpOneLine(invertedBoard, 0);
            var upperRightBoard = upperBoard << 1;
            var vertical = upperBoard & invertedBoard;
            lowerRightBoard <<= 1;
            var rightBoard = invertedBoard << 1;
            var leftBoard = invertedBoard >> 1;
            leftBoard &= vertical;
            rightBoard &= vertical;
            leftBoard &= upperRightBoard;
            rightBoard &= lowerRightBoard;
            return (leftBoard, rightBoard);
        }
    }
}
