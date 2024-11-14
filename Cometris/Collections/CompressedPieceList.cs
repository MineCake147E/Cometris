using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

using Cometris.Pieces;

using ModernMemory;

namespace Cometris.Collections
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public readonly struct CompressedPieceList<TStorage> : IReadOnlyList<Piece>, IEquatable<CompressedPieceList<TStorage>>
        where TStorage : IBinaryInteger<TStorage>, IUnsignedNumber<TStorage>
    {
        private readonly TStorage value;

        public int Count => (value.GetShortestBitLength() + 2) / 3;

        public static int MaxCapacity => TStorage.AllBitsSet.GetShortestBitLength() / 3;

        public TStorage Value => value;

        public Piece this[int index]
        {
            get
            {
                index *= 3;
                return (Piece)(uint.CreateTruncating(value >> index) & 7u);
            }
        }

        public CompressedPieceList(TStorage value)
        {
            this.value = value;
        }

        //public CompressedKnownPieceList(ushort id)
        //{
        //    this = new(new BagPermutation(id));
        //}

        //public CompressedKnownPieceList(BagPermutation value)
        //{
        //    if (Bmi2.X.IsSupported)
        //    {
        //        this.value = Bmi2.X.ParallelBitExtract(value.Value, 0x0707_0707_0707_0707ul);
        //    }
        //    else
        //    {
        //        this = Create(value);
        //    }
        //}

        public CompressedPieceList(params ReadOnlySpan<Piece> pieces)
        {
            var m = TStorage.Zero;
            int y = 0;
            foreach (var item in pieces.SliceWhileIfLongerThan(21))
            {
                var k = TStorage.CreateTruncating((byte)item & 7u);
                m |= k << y;
                y += 3;
            }
            value = m;
        }

        public static CompressedPieceList<TStorage> Create<TEnumerable>(TEnumerable pieces) where TEnumerable : IEnumerable<Piece>, allows ref struct
        {
            var m = TStorage.Zero;
            int y = 0;
            foreach (var item in pieces)
            {
                var k = TStorage.CreateTruncating((byte)item & 7u);
                m |= k << y;
                y += 3;
            }
            return new(m);
        }

        public CompressedPieceList<TStorage> Slice(int start)
        {
            var v = value;
            v >>= start * 3;
            return new(v);
        }

        public CompressedPieceList<TStorage> Slice(int start, int length)
        {
            var v = value;
            v >>= start * 3;
            var l = length * 3;
            v &= ~(TStorage.AllBitsSet << l);
            return new(v);
        }

        public BagEnumerator GetEnumerator() => new(value);
        IEnumerator<Piece> IEnumerable<Piece>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct BagEnumerator(TStorage value) : IEnumerator<Piece>
        {
            private readonly TStorage value = value;
            private int index = -3;

            public readonly Piece Current => (Piece)(uint.CreateTruncating(value >> index) & 7u);

            readonly object IEnumerator.Current => Current;

            public void Dispose() => index = 60;
            public bool MoveNext()
            {
                index += 3;
                return value >> index > TStorage.Zero;
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

        //public BagPieceSet CountAvailablePieces() => BagPieceSet.CreateCountFrom(this);

        public override string ToString() => GetDebuggerDisplay();
        public override bool Equals(object? obj) => obj is CompressedPieceList<TStorage> list && Equals(list);
        public bool Equals(CompressedPieceList<TStorage> other) => value == other.value;
        public override int GetHashCode() => HashCode.Combine(value);

        public static bool operator ==(CompressedPieceList<TStorage> left, CompressedPieceList<TStorage> right) => left.Equals(right);
        public static bool operator !=(CompressedPieceList<TStorage> left, CompressedPieceList<TStorage> right) => !(left == right);
    }

    public static class CompressedPieceList
    {
        public static CompressedPieceList<TStorage> Create<TStorage>(TStorage value) where TStorage : IBinaryInteger<TStorage>, IUnsignedNumber<TStorage>
            => new(value);
        public static CompressedPieceList<TStorage> Create<TStorage>(ReadOnlySpan<Piece> pieces) where TStorage : IBinaryInteger<TStorage>, IUnsignedNumber<TStorage>
            => new(pieces);
    }
}
