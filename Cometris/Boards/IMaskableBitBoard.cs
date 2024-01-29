using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Cometris.Boards
{
    public partial interface IMaskableBitBoard<TSelf, TLineElement, TVectorLineMask, TCompactLineMask> : IBitBoard<TSelf, TLineElement>
        where TSelf : unmanaged, IBitBoard<TSelf, TLineElement>, IMaskableBitBoard<TSelf, TLineElement, TVectorLineMask, TCompactLineMask>
        where TLineElement : unmanaged, IBinaryInteger<TLineElement>, IUnsignedNumber<TLineElement>
        where TVectorLineMask : struct, IEquatable<TVectorLineMask>
        where TCompactLineMask: unmanaged, IBinaryInteger<TCompactLineMask>
    {
        static abstract TVectorLineMask GetClearableLinesVector(TSelf board);
        static virtual TCompactLineMask GetClearableLinesCompact(TSelf board) => TSelf.CompressMask(TSelf.GetClearableLinesVector(board));
        static abstract TSelf ClearClearableLines(TSelf board, TLineElement fill, out TVectorLineMask clearedLines);

        static abstract TSelf ClearLines(TSelf board, TLineElement fill, TVectorLineMask lines);

        static abstract bool IsSetAt(TVectorLineMask mask, byte index);

        #region Mask Constants
        static abstract TVectorLineMask ZeroMask { get; }
        static abstract TVectorLineMask AllBitsSetMask { get; }
        #endregion

        #region Mask Construction
        static abstract TVectorLineMask CreateMaskFromBoard(TSelf board);
        #endregion

        #region Lines Classification
        static virtual TVectorLineMask GetLineMaskForZeroPerLines(TSelf board) => TSelf.CompareEqualPerLineVector(board, TSelf.Zero);
        static virtual TVectorLineMask GetLineMaskForEmptyPerLines(TSelf board) => TSelf.CompareEqualPerLineVector(board, TSelf.Empty);
        static virtual TVectorLineMask GetLineMaskForInvertedEmptyPerLines(TSelf board) => TSelf.CompareEqualPerLineVector(board, TSelf.InvertedEmpty);
        static virtual TVectorLineMask GetLineMaskForFullPerLines(TSelf board) => TSelf.CompareEqualPerLineVector(board, TSelf.AllBitsSet);
        #endregion

        #region Mask Operators
        static abstract TVectorLineMask MaskUnaryNegation(TVectorLineMask mask);
        static abstract TVectorLineMask MaskAnd(TVectorLineMask left, TVectorLineMask right);
        static abstract TVectorLineMask MaskOr(TVectorLineMask left, TVectorLineMask right);
        static abstract TVectorLineMask MaskXor(TVectorLineMask left, TVectorLineMask right);
        #endregion

        #region Mask Utilities
        static abstract int TrailingZeroCount(TCompactLineMask mask);
        static abstract int PopCount(TCompactLineMask mask);

        static virtual int LineHeight(TCompactLineMask mask) => mask.GetShortestBitLength();
        #endregion

        #region Operator Supplement
        /// <summary>
        /// Blend the <paramref name="left"/> and <paramref name="right"/> according to the line-wise <paramref name="mask"/>.
        /// </summary>
        /// <param name="mask">The line mask. 1 means <paramref name="right"/>, and 0 means <paramref name="left"/>.</param>
        /// <param name="left">The values to be selected when the corresponding bit of <paramref name="mask"/> is 0.</param>
        /// <param name="right">The values to be selected when the corresponding bit of <paramref name="mask"/> is 1.</param>
        /// <returns><paramref name="left"/> &amp; ((<paramref name="left"/> ^ <paramref name="right"/>) &amp; <paramref name="mask"/>)</returns>
        static abstract TSelf LineSelect(TVectorLineMask mask, TSelf left, TSelf right);

        static abstract TCompactLineMask CompressMask(TVectorLineMask mask);

        static abstract TVectorLineMask ExpandMask(TCompactLineMask compactLineMask);
        #endregion

        #region Per-Line Operations

        static abstract TVectorLineMask CompareEqualPerLineVector(TSelf left, TSelf right);
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual TCompactLineMask CompareEqualPerLineCompact(TSelf left, TSelf right) => TSelf.CompressMask(TSelf.CompareEqualPerLineVector(left, right));

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual TVectorLineMask CompareNotEqualPerLineVector(TSelf left, TSelf right) => TSelf.MaskUnaryNegation(TSelf.CompareEqualPerLineVector(left, right));
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual TCompactLineMask CompareNotEqualPerLineCompact(TSelf left, TSelf right) => TSelf.CompressMask(TSelf.CompareNotEqualPerLineVector(left, right));
        #endregion

    }
}
