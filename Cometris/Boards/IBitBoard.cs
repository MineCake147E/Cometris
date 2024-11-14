using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Cometris.Boards
{
    public interface IBitBoard<TSelf, TLineElement> : IEquatable<TSelf>, IEqualityOperators<TSelf, TSelf, bool>, IReadOnlyList<TLineElement>
        where TSelf : unmanaged, IBitBoard<TSelf, TLineElement>
        where TLineElement : unmanaged, IBinaryNumber<TLineElement>, IUnsignedNumber<TLineElement>
    {
        #region Static Members

        #region Line Constants
        static abstract TLineElement EmptyLine { get; }

        static virtual TLineElement FullLine => TLineElement.AllBitsSet;

        static virtual TLineElement InvertedEmptyLine => ~TSelf.EmptyLine;

        static virtual TLineElement ZeroLine => TLineElement.Zero;

        #endregion

        #region Board Constants
        static abstract TSelf AllBitsSet { get; }
        static abstract TSelf Empty { get; }

        static abstract TSelf InvertedEmpty { get; }

        static virtual TSelf Zero => default;

        #endregion

        /// <summary>
        /// The shift value that <typeparamref name="TLineElement"/> &gt;&gt; (<see cref="BitPositionXLeftmost"/> - 1) &amp; 1 represents the value of the leftmost column.
        /// </summary>
        static abstract int BitPositionXLeftmost { get; }

        static virtual int BitPositionXRightmost => TSelf.BitPositionXLeftmost - TSelf.EffectiveWidth;

        static abstract int EffectiveWidth { get; }

        static abstract int Height { get; }

        static virtual int LeftmostPaddingWidth => TSelf.RightmostPaddingWidth;

        static abstract int RightmostPaddingWidth { get; }

        static virtual int StorableWidth => 8 * Unsafe.SizeOf<TLineElement>();

        static virtual int TotalStorableBlocks => TSelf.Height * TSelf.StorableWidth;

        static virtual int TotalEffectiveBlocks => TSelf.Height * TSelf.EffectiveWidth;
        #endregion

        int IReadOnlyCollection<TLineElement>.Count => TSelf.Height;

        new BoardLineEnumerable<TSelf, TLineElement>.Enumerator GetEnumerator() => new((TSelf)this);

        IEnumerator<TLineElement> IEnumerable<TLineElement>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        int GetHashCode();
    }
}
