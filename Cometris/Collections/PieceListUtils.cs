using System;
using System.Numerics;

using Cometris.Pieces;

namespace Cometris.Collections
{
    public static class PieceListUtils
    {
        public static CompressedValuePieceList<TStorage> Create<TStorage>(TStorage value) where TStorage : IBinaryInteger<TStorage>, IUnsignedNumber<TStorage>
            => new(value);
        public static CompressedValuePieceList<TStorage> Create<TStorage>(ReadOnlySpan<Piece> pieces) where TStorage : IBinaryInteger<TStorage>, IUnsignedNumber<TStorage>
            => new(pieces);
    }
}
