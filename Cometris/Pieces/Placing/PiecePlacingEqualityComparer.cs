using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Cometris.Boards;

namespace Cometris.Pieces.Placing
{
    public readonly struct PiecePlacingEqualityComparer<TPiecePlacement, TPieceRegistry, TBitBoard>(TBitBoard baseBoard) : IEqualityComparer<TPiecePlacement>
        where TPiecePlacement : unmanaged, IPiecePlacement<TPiecePlacement>
        where TPieceRegistry : unmanaged, IPieceRegistry<TBitBoard>
        where TBitBoard : unmanaged, IOperableBitBoard<TBitBoard, ushort>
    {
        public bool Equals(TPiecePlacement x, TPiecePlacement y)
        {
            var b = baseBoard;
            var xb = TPieceRegistry.PlacePiece(x);
            var yb = TPieceRegistry.PlacePiece(y);
            xb |= b;
            yb |= b;
            return xb == yb;
        }
        public int GetHashCode([DisallowNull] TPiecePlacement obj) => (baseBoard | TPieceRegistry.PlacePiece(obj)).GetHashCode();
    }
}
