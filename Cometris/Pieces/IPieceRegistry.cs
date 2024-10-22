using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Cometris.Boards;
using Cometris.Pieces.Placing;

namespace Cometris.Pieces
{
    public interface IPieceRegistry<TBitBoard>
        where TBitBoard : unmanaged, IOperableBitBoard<TBitBoard, ushort>
    {
        static abstract TBitBoard PlacePiece<TPiecePlacement>(TPiecePlacement placement) where TPiecePlacement : unmanaged, IPiecePlacement<TPiecePlacement>;

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) LocateMovablePoints(Piece piece, TBitBoard board);
    }
}
