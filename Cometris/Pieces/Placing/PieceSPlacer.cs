using Cometris.Boards;

using MikoMino;

namespace Cometris.Pieces.Placing
{
    public readonly struct PieceSPlacer<TBitBoard> : IPiecePlacer<TBitBoard>
        where TBitBoard : unmanaged, IBitBoard<TBitBoard, ushort>
    {
        public static TBitBoard PlaceUp(int x, int y)
        {
            var uline = (ushort)(0xC000_C000u >> x);
            var mline = (ushort)(0x8001_8001u >> x);
            return TBitBoard.CreateTwoAdjacentLinesUp(y, mline, uline);
        }
        public static TBitBoard PlaceRight(int x, int y)
        {
            var uline = (ushort)(0x8000u >> x);
            var mline = (ushort)(0xC000u >> x);
            var lline = (ushort)(0x4000u >> x);
            return TBitBoard.CreateThreeAdjacentLines(y, lline, mline, uline);
        }
        public static TBitBoard PlaceDown(int x, int y)
        {
            var mline = (ushort)(0xC000_C000u >> x);
            var lline = (ushort)(0x8001_8001u >> x);
            return TBitBoard.CreateTwoAdjacentLinesDown(y, lline, mline);
        }
        public static TBitBoard PlaceLeft(int x, int y)
        {
            var uline = (ushort)(0x0001_0001u >> x);
            var mline = (ushort)(0x8001_8001u >> x);
            var lline = (ushort)(0x8000u >> x);
            return TBitBoard.CreateThreeAdjacentLines(y, lline, mline, uline);
        }
        public static TBitBoard Place(Angle angle, int x, int y) => PiecePlacerImplementationUtils.Place<TBitBoard, PieceIPlacer<TBitBoard>>(angle, x, y);
    }
}
