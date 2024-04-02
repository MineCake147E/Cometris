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
        where TBitBoard : unmanaged, IBitBoard<TBitBoard, ushort>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) LocateMovablePoints(Piece piece, TBitBoard bitBoard);
    }
}
