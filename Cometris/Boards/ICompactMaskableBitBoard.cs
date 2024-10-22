using System;
using System.Numerics;
using System.Runtime.CompilerServices;

using Cometris.Pieces;
using MikoMino.Game;

namespace Cometris.Boards
{
    public partial interface ICompactMaskableBitBoard<TSelf, TLineElement, TVectorLineMask, TCompactLineMask> : IMaskableBitBoard<TSelf, TLineElement, TVectorLineMask>, IHashBitBoard<TSelf, TLineElement>
        where TSelf : unmanaged, IOperableBitBoard<TSelf, TLineElement>, ICompactMaskableBitBoard<TSelf, TLineElement, TVectorLineMask, TCompactLineMask>
        where TLineElement : unmanaged, IBinaryInteger<TLineElement>, IUnsignedNumber<TLineElement>
        where TVectorLineMask : struct, IEquatable<TVectorLineMask>
        where TCompactLineMask: unmanaged, IBinaryInteger<TCompactLineMask>
    {
        static virtual TCompactLineMask GetClearableLinesCompact(TSelf board) => TSelf.CompressMask(TSelf.GetClearableLinesVector(board));
        #region Mask Utilities
        static abstract int TrailingZeroCount(TCompactLineMask mask);
        static abstract int PopCount(TCompactLineMask mask);

        static virtual int LineHeight(TCompactLineMask mask) => mask.GetShortestBitLength();
        #endregion

        static abstract TCompactLineMask CompressMask(TVectorLineMask mask);

        static abstract TVectorLineMask ExpandMask(TCompactLineMask compactLineMask);

        #region Per-Line Operations
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual TCompactLineMask CompareEqualPerLineCompact(TSelf left, TSelf right) => TSelf.CompressMask(TSelf.CompareEqualPerLineVector(left, right));
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual TCompactLineMask CompareNotEqualPerLineCompact(TSelf left, TSelf right) => TSelf.CompressMask(TSelf.CompareNotEqualPerLineVector(left, right));
        #endregion

    }
}
