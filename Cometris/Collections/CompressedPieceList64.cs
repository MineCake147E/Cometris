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
    public readonly struct CompressedPieceList64 : IReadOnlyList<Piece>, IEquatable<CompressedPieceList64>
    {
        private readonly ulong value;

        public int Count => (BitOperations.LeadingZeroCount(0ul) - BitOperations.LeadingZeroCount(value) + 2) / 3;

        public ulong Value => value;

        public Piece this[int index]
        {
            get
            {
                index *= 3;
                return (Piece)((value >> index) & 7);
            }
        }

        public CompressedPieceList64(ulong value)
        {
            this.value = value;
        }

        //public CompressedKnownPieceList64(ushort id)
        //{
        //    this = new(new BagPermutation(id));
        //}

        //public CompressedKnownPieceList64(BagPermutation value)
        //{
        //    if (Bmi2.X64.IsSupported)
        //    {
        //        this.value = Bmi2.X64.ParallelBitExtract(value.Value, 0x0707_0707_0707_0707ul);
        //    }
        //    else
        //    {
        //        this = Create(value);
        //    }
        //}

        public CompressedPieceList64(params ReadOnlySpan<Piece> pieces)
        {
            var m = 0ul;
            int y = 0;
            foreach (var item in pieces.SliceWhileIfLongerThan(21))
            {
                var k = (byte)item & 7ul;
                m |= k << y;
                y += 3;
            }
            value = m;
        }

        public static CompressedPieceList64 Create<TEnumerable>(TEnumerable pieces) where TEnumerable : IEnumerable<Piece>, allows ref struct
        {
            var m = 0ul;
            int y = 0;
            foreach (var item in pieces)
            {
                var k = (byte)item & 7ul;
                m |= k << y;
                y += 3;
            }
            return new(m & long.MaxValue);
        }

        public CompressedPieceList64 Slice(int start)
        {
            var v = value;
            v >>= start * 3;
            return new CompressedPieceList64(v);
        }

        public CompressedPieceList64 Slice(int start, int length)
        {
            var v = value;
            v >>= start * 3;
            var l = length * 3;
            if (Bmi2.X64.IsSupported)
            {
                v = Bmi2.X64.ZeroHighBits(v, (uint)l);
            }
            else
            {
                v &= ~0ul << l;
            }
            return new CompressedPieceList64(v);
        }

        public BagEnumerator GetEnumerator() => new(value);
        IEnumerator<Piece> IEnumerable<Piece>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct BagEnumerator(ulong value) : IEnumerator<Piece>
        {
            private readonly ulong value = value;
            private int index = -3;

            public readonly Piece Current => (Piece)((value >> index) & 7);

            readonly object IEnumerator.Current => Current;

            public void Dispose() => index = 60;
            public bool MoveNext()
            {
                index += 3;
                return value >> index > 0;
            }

            public void Reset() => index = -3;
        }

        private string GetDebuggerDisplay()
        {
            var e = this;
            var sb = new StringBuilder();
            foreach (var item in e)
            {
                sb.Append(item.ToString());
            }
            return sb.ToString();
        }

        //public BagPieceSet CountAvailablePieces() => BagPieceSet.CreateCountFrom(this);

        public override string ToString() => GetDebuggerDisplay();
        public override bool Equals(object? obj) => obj is CompressedPieceList64 list && Equals(list);
        public bool Equals(CompressedPieceList64 other) => value == other.value;
        public override int GetHashCode() => HashCode.Combine(value);

        public static bool operator ==(CompressedPieceList64 left, CompressedPieceList64 right) => left.Equals(right);
        public static bool operator !=(CompressedPieceList64 left, CompressedPieceList64 right) => !(left == right);
    }
}
