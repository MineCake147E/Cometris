using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Cometris.Pieces;

using MikoMino.Game;

namespace Cometris.Boards
{
    public interface IMaskableBitBoard<TSelf, TLineElement, TVectorLineMask> : IOperableBitBoard<TSelf, TLineElement>
        where TSelf : unmanaged, IOperableBitBoard<TSelf, TLineElement>, IMaskableBitBoard<TSelf, TLineElement, TVectorLineMask>
        where TLineElement : unmanaged, IBinaryInteger<TLineElement>, IUnsignedNumber<TLineElement>
        where TVectorLineMask : struct, IEquatable<TVectorLineMask>
    {
        static abstract TVectorLineMask GetClearableLinesVector(TSelf board);

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

        #region Mask Operators
        static abstract TVectorLineMask MaskUnaryNegation(TVectorLineMask mask);
        static abstract TVectorLineMask MaskAnd(TVectorLineMask left, TVectorLineMask right);
        static abstract TVectorLineMask MaskOr(TVectorLineMask left, TVectorLineMask right);
        static abstract TVectorLineMask MaskXor(TVectorLineMask left, TVectorLineMask right);

        static virtual bool IsMaskZero(TVectorLineMask mask) => TSelf.ZeroMask.Equals(mask);
        #endregion

        #region Lines Classification
        static virtual TVectorLineMask GetLineMaskForZeroPerLines(TSelf board) => TSelf.CompareEqualPerLineVector(board, TSelf.Zero);
        static virtual TVectorLineMask GetLineMaskForEmptyPerLines(TSelf board) => TSelf.CompareEqualPerLineVector(board, TSelf.Empty);
        static virtual TVectorLineMask GetLineMaskForInvertedEmptyPerLines(TSelf board) => TSelf.CompareEqualPerLineVector(board, TSelf.InvertedEmpty);
        static virtual TVectorLineMask GetLineMaskForFullPerLines(TSelf board) => TSelf.CompareEqualPerLineVector(board, TSelf.AllBitsSet);
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
        #endregion
        #region Per-Line Operations
        static abstract TVectorLineMask CompareEqualPerLineVector(TSelf left, TSelf right);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual TVectorLineMask CompareNotEqualPerLineVector(TSelf left, TSelf right) => TSelf.MaskUnaryNegation(TSelf.CompareEqualPerLineVector(left, right));

        #endregion
    }
}
