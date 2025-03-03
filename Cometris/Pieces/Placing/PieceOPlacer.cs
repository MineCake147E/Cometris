﻿using Cometris.Boards;

using MikoMino;

namespace Cometris.Pieces.Placing
{
    public readonly struct PieceOPlacer<TBitBoard> : IPiecePlacer<TBitBoard>
        where TBitBoard : unmanaged, IOperableBitBoard<TBitBoard, ushort>
    {
        public static TBitBoard PlaceUp(int x, int y)
        {
            var hline = (ushort)(0xC000_C000u >> x);
            return TBitBoard.CreateTwoAdjacentLinesUp(y, hline, hline);
        }
        public static TBitBoard PlaceRight(int x, int y)
        {
            var hline = (ushort)(0xC000_C000u >> x);
            return TBitBoard.CreateTwoAdjacentLinesDown(y, hline, hline);
        }
        public static TBitBoard PlaceDown(int x, int y)
        {
            var hline = (ushort)(0x8001_8001u >> x);
            return TBitBoard.CreateTwoAdjacentLinesDown(y, hline, hline);
        }
        public static TBitBoard PlaceLeft(int x, int y)
        {
            var hline = (ushort)(0x8001_8001u >> x);
            return TBitBoard.CreateTwoAdjacentLinesUp(y, hline, hline);
        }
        public static TBitBoard Place(Angle angle, int x, int y) => PiecePlacerImplementationUtils.Place<TBitBoard, PieceOPlacer<TBitBoard>>(angle, x, y);
    }
}
