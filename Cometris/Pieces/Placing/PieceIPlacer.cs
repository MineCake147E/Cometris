using Cometris.Boards;

using MikoMino;

namespace Cometris.Pieces.Placing
{
    public readonly struct PieceIPlacer<TBitBoard> : IPiecePlacer<TBitBoard>
        where TBitBoard : unmanaged, IBitBoard<TBitBoard, ushort>
    {
        public static TBitBoard PlaceUp(int x, int y)
        {
            var hline = (ushort)(0xE001_E001u >> x);
            return TBitBoard.CreateSingleLine(hline, y);
        }
        public static TBitBoard PlaceRight(int x, int y) => TBitBoard.CreateVerticalI4Piece(x, y);
        public static TBitBoard PlaceDown(int x, int y)
        {
            var hline = (ushort)(0xC003_C003u >> x);
            return TBitBoard.CreateSingleLine(hline, y);
        }
        public static TBitBoard PlaceLeft(int x, int y) => TBitBoard.CreateVerticalI4Piece(x, y + 1);
        public static TBitBoard Place(Angle angle, int x, int y) => PiecePlacerImplementationUtils.Place<TBitBoard, PieceIPlacer<TBitBoard>>(angle, x, y);
    }
}
