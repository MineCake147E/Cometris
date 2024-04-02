using Cometris.Boards;

using MikoMino;

namespace Cometris.Pieces.Placing
{
    public readonly struct PieceJPlacer<TBitBoard> : IPiecePlacer<TBitBoard>
        where TBitBoard : unmanaged, IBitBoard<TBitBoard, ushort>
    {
        public static TBitBoard PlaceUp(int x, int y)
        {
            var hline = (ushort)(0xC001_C001u >> x);
            var vline = (ushort)(0x0001_0001u >> x);
            return TBitBoard.CreateTwoAdjacentLinesUp(y, hline, vline);
        }
        public static TBitBoard PlaceRight(int x, int y)
        {
            var vline = (ushort)(0x8000u >> x);
            var mline = (ushort)(0xC000u >> x);
            return TBitBoard.CreateThreeAdjacentLines(y, vline, vline, mline);
        }
        public static TBitBoard PlaceDown(int x, int y)
        {
            var hline = (ushort)(0xC001_C001u >> x);
            var vline = (ushort)(0x4000u >> x);
            return TBitBoard.CreateTwoAdjacentLinesDown(y, vline, hline);
        }
        public static TBitBoard PlaceLeft(int x, int y)
        {
            var mline = (ushort)(0x8001_8001u >> x);
            var vline = (ushort)(0x8000u >> x);
            return TBitBoard.CreateThreeAdjacentLines(y, mline, vline, vline);
        }
        public static TBitBoard Place(Angle angle, int x, int y) => PiecePlacerImplementationUtils.Place<TBitBoard, PieceIPlacer<TBitBoard>>(angle, x, y);
    }
}