using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

using Cometris.Pieces;

using ModernMemory;

namespace Cometris.Collections
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public readonly struct CompressedValuePieceList<TStorage> : IReadOnlyList<Piece>, IEquatable<CompressedValuePieceList<TStorage>>
        where TStorage : IBinaryInteger<TStorage>, IUnsignedNumber<TStorage>
    {
        private readonly TStorage value;

        public int Count => (value.GetShortestBitLength() + 2) / 3;

        public static int MaxCapacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => TStorage.AllBitsSet.GetShortestBitLength() / 3;
        }

        public TStorage Value => value;

        public Piece this[int index]
        {
            get
            {
                index *= 3;
                return (Piece)(uint.CreateTruncating(value >>> index) & 7u);
            }
        }

        public CompressedValuePieceList(TStorage value)
        {
            this.value = value;
        }

        public CompressedValuePieceList(params ReadOnlySpan<Piece> pieces)
        {
            var m = TStorage.Zero;
            int y = 0;
            int c = MaxCapacity;
            foreach (var item in pieces)
            {
                var k = TStorage.CreateTruncating((byte)item & 7u);
                m |= k << y;
                y += 3;
                if (--c <= 0) break;
            }
            value = m;
        }

        public static CompressedValuePieceList<TStorage> Create<TEnumerable>(TEnumerable pieces) where TEnumerable : IEnumerable<Piece>, allows ref struct
        {
            var m = TStorage.Zero;
            int y = 0;
            int c = MaxCapacity;
            foreach (var item in pieces)
            {
                var k = TStorage.CreateTruncating((byte)item & 7u);
                m |= k << y;
                y += 3;
                if (--c <= 0) break;
            }
            return new(m);
        }

        public CompressedValuePieceList<TStorage> Slice(int start)
        {
            var v = value;
            v >>= start * 3;
            return new(v);
        }

        public CompressedValuePieceList<TStorage> Slice(int start, int length)
        {
            var v = value;
            v >>= start * 3;
            var l = length * 3;
            v &= ~(TStorage.AllBitsSet << l);
            return new(v);
        }

        public Enumerator GetEnumerator() => new(value);
        IEnumerator<Piece> IEnumerable<Piece>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator(TStorage value) : IEnumerator<Piece>
        {
            private readonly TStorage value = value;
            private int index = -3;

            public readonly Piece Current => (Piece)(uint.CreateTruncating(value >>> index) & 7u);

            readonly object IEnumerator.Current => Current;

            public void Dispose() => index = 60;
            public bool MoveNext()
            {
                index += 3;
                return value >>> index > TStorage.Zero;
            }

            public void Reset() => index = -3;
        }

        private string GetDebuggerDisplay()
        {
            var e = this;
            var sb = new StringBuilder();
            foreach (var item in e)
            {
                _ = sb.Append(item.ToString());
            }
            return sb.ToString();
        }

        public override string ToString() => GetDebuggerDisplay();
        public override bool Equals(object? obj) => obj is CompressedValuePieceList<TStorage> list && Equals(list);
        public bool Equals(CompressedValuePieceList<TStorage> other) => value == other.value;
        public override int GetHashCode() => HashCode.Combine(value);

        public static bool operator ==(CompressedValuePieceList<TStorage> left, CompressedValuePieceList<TStorage> right) => left.Equals(right);
        public static bool operator !=(CompressedValuePieceList<TStorage> left, CompressedValuePieceList<TStorage> right) => !(left == right);
    }
}
