using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cometris.Boards;
using Cometris.Movements;
using Cometris.Pieces.Mobility;
using Cometris.Pieces.Placing;

using MikoMino;
using MikoMino.Boards;

namespace Cometris.Pieces
{
    public readonly struct GuidelinePieceRegistry<TBitBoard> : IPieceRegistry<TBitBoard>
        where TBitBoard : unmanaged, IOperableBitBoard<TBitBoard, ushort>
    {
        public static (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) LocateMovablePoints(Piece piece, TBitBoard board) => piece switch
        {
            Piece.T => PieceTMovablePointLocater<TBitBoard>.LocateMovablePoints(board),
            Piece.I => PieceIMovablePointLocater<TBitBoard>.LocateMovablePoints(board),
            Piece.O => (PieceOMovablePointLocater<TBitBoard>.LocateSymmetricMovablePoints(board), default, default, default),
            Piece.J => PieceJMovablePointLocater<TBitBoard>.LocateMovablePoints(board),
            Piece.L => PieceLMovablePointLocater<TBitBoard>.LocateMovablePoints(board),
            Piece.S => PieceSMovablePointLocater<TBitBoard>.LocateMovablePoints(board),
            Piece.Z => PieceZMovablePointLocater<TBitBoard>.LocateMovablePoints(board),
            _ => default,
        };
        public static TBitBoard PlacePiece<TPiecePlacement>(TPiecePlacement placement) where TPiecePlacement : unmanaged, IPiecePlacement<TPiecePlacement>
            => placement.Piece switch
        {
            Piece.T => PieceTPlacer<TBitBoard>.Place(placement.Angle, placement.Position.X, placement.Position.Y),
            Piece.I => PieceIPlacer<TBitBoard>.Place(placement.Angle, placement.Position.X, placement.Position.Y),
            Piece.O => PieceOPlacer<TBitBoard>.Place(placement.Angle, placement.Position.X, placement.Position.Y),
            Piece.J => PieceJPlacer<TBitBoard>.Place(placement.Angle, placement.Position.X, placement.Position.Y),
            Piece.L => PieceLPlacer<TBitBoard>.Place(placement.Angle, placement.Position.X, placement.Position.Y),
            Piece.S => PieceSPlacer<TBitBoard>.Place(placement.Angle, placement.Position.X, placement.Position.Y),
            Piece.Z => PieceZPlacer<TBitBoard>.Place(placement.Angle, placement.Position.X, placement.Position.Y),
            _ => default,
        };
    }
}
