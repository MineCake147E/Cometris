﻿using System.Runtime.CompilerServices;

using Cometris.Boards;

namespace Cometris.Pieces.Mobility
{
    public readonly struct PieceJMovablePointLocater<TBitBoard> : IAsymmetricPieceMovablePointLocater<TBitBoard> where TBitBoard : unmanaged, IOperableBitBoard<TBitBoard, ushort>
    {
        public static (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) LocateMovablePoints(TBitBoard bitBoard)
            => LocateMovablePointsInternal(~bitBoard);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) LocateMovablePointsInternal(TBitBoard invertedBoard)
        {
            var rightBoard = invertedBoard << 1;
            var leftBoard = invertedBoard >> 1;
            var upperBoard = TBitBoard.ShiftDownOneLine(invertedBoard, TBitBoard.InvertedEmpty);
            var lowerBoard = TBitBoard.ShiftUpOneLine(invertedBoard, TBitBoard.Zero);
            var vertical = upperBoard & lowerBoard;
            var horizontal = leftBoard & rightBoard;
            vertical &= invertedBoard;
            horizontal &= invertedBoard;
            var upperRightBoard = upperBoard << 1;
            var upperLeftBoard = upperBoard >> 1;
            var lowerRightBoard = lowerBoard << 1;
            var lowerLeftBoard = lowerBoard >> 1;
            upperLeftBoard &= horizontal;
            upperRightBoard &= vertical;
            lowerRightBoard &= horizontal;
            lowerLeftBoard &= vertical;
            return (upperLeftBoard, upperRightBoard, lowerRightBoard, lowerLeftBoard);
        }
    }
}
