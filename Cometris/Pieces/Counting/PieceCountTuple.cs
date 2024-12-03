using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.Wasm;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

using Cometris.Collections;
using Cometris.Utils;

namespace Cometris.Pieces.Counting
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public readonly struct PieceCountTuple : IEquatable<PieceCountTuple>, IReadOnlyDictionary<Piece, byte>
    {
        private readonly double medium;

        public ulong Value => BitConverter.DoubleToUInt64Bits(medium);

        public ulong MaskedValue => Value & ~0xfful;

        [SuppressMessage("Major Code Smell", "S1168:Empty arrays and collections should be returned instead of null", Justification = "Slow if applied")]
        public static PieceCountTuple Zero => default;

        IEnumerable<Piece> IReadOnlyDictionary<Piece, byte>.Keys => [.. PiecesUtils.AllPieces];
        IEnumerable<byte> IReadOnlyDictionary<Piece, byte>.Values
        {
            get
            {
                var k = this;
                return Enumerable.Select([.. PiecesUtils.AllPieces], a => k[a]);
            }
        }

        public int Count => 8;
        #region Constructors and Create methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PieceCountTuple(byte countForAllPieces)
        {
            this = new(Vector128.Create(countForAllPieces));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PieceCountTuple(Piece initialPiece, byte count = 1)
        {
            var x8 = (byte)initialPiece * 8;
            var s0 = BitConverter.UInt64BitsToDouble((ulong)count << x8);
            medium = s0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PieceCountTuple(double medium)
        {
            this.medium = medium;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PieceCountTuple(ulong medium)
        {
            this.medium = BitConverter.UInt64BitsToDouble(medium);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PieceCountTuple(Vector128<byte> medium)
        {
            this.medium = medium.AsDouble().GetElement(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PieceCountTuple(BagPieceSet pieces)
        {
            if (Bmi2.X64.IsSupported)
            {
                var m = (ulong)pieces.Value;
                medium = BitConverter.UInt64BitsToDouble(Bmi2.X64.ParallelBitDeposit(m, 0x0101_0101_0101_0100ul));
            }
            else if (Vector64.IsHardwareAccelerated)
            {
                var v0_16b = Vector64.Create((byte)pieces.Value);
                v0_16b &= Vector64.Create(0, 1, 2, 4, 8, 16, 32, 64).AsByte();
                medium = Vector64.Min(Vector64<byte>.One, v0_16b).AsDouble().GetElement(0);
            }
            else if (Vector128.IsHardwareAccelerated)
            {
                var v0_16b = Vector128.Create((byte)pieces.Value);
                v0_16b &= Vector128.Create(0, 1, 2, 4, 8, 16, 32, 64, 0, 0, 0, 0, 0, 0, 0, 0).AsByte();
                medium = Vector128.Min(Vector128<byte>.One, v0_16b).AsDouble().GetElement(0);
            }
            else
            {
                this = CreateCountFrom(pieces);
            }
        }

        [OverloadResolutionPriority(1)]
        public static PieceCountTuple CreateCountFrom(ReadOnlySpan<Piece> pieces)
        {
            var m = new PieceCountTuple();
            foreach (var item in pieces)
            {
                m = m.Add(item);
            }
            return m;
        }

        public static PieceCountTuple CreateCountFrom<TEnumerable>(TEnumerable pieces) where TEnumerable : IEnumerable<Piece>, allows ref struct
        {
            var m = new PieceCountTuple();
            foreach (var item in pieces)
            {
                m = m.Add(item);
            }
            return m;
        }

        [OverloadResolutionPriority(1)]
        public static PieceCountTuple CreateSaturatedCountFrom(ReadOnlySpan<Piece> pieces)
        {
            var m = new PieceCountTuple();
            foreach (var item in pieces)
            {
                m = m.AddSaturate(item);
            }
            return m;
        }

        public static PieceCountTuple CreateSaturatedCountFrom<TEnumerable>(TEnumerable pieces) where TEnumerable : IEnumerable<Piece>, allows ref struct
        {
            var m = new PieceCountTuple();
            foreach (var item in pieces)
            {
                m = m.AddSaturate(item);
            }
            return m;
        }

        #endregion
        public byte this[Piece piece]
        {
            get
            {
                var x8 = (byte)piece * 8;
                var x9 = BitConverter.DoubleToUInt64Bits(medium);
                return (byte)(x9 >> x8);
            }
        }

        public PieceCountTuple Add(Piece piece, sbyte count = 1)
        {
            var x8 = (byte)piece * 8;
            var x9 = (ulong)(byte)count << x8;
            var v0_8b = Vector128.CreateScalarUnsafe(medium).AsByte();
            var v1_8b = Vector128.CreateScalarUnsafe(x9).AsByte();
            return new(v0_8b + v1_8b);
        }

        public PieceCountTuple AddSaturate(Piece piece, byte count = 1)
            => AddSaturate(this, new(piece, count));

        public PieceCountTuple WithPiece(Piece piece, byte count)
        {
            var v0_8b = Vector128.CreateScalarUnsafe(medium).AsByte();
            var v1_8b = Vector128.Create((byte)piece);
            v1_8b = Vector128.Equals(v1_8b, Vector128<byte>.Indices);
            return new(Vector128.ConditionalSelect(v1_8b, Vector128.Create(count), v0_8b));
        }

        public uint CalculateMinimumCount()
        {
            var v0_16b = Vector128.CreateScalarUnsafe(medium).AsByte();
            v0_16b |= Vector128.CreateScalar(byte.MaxValue);
            return VectorUtils.MinAcrossLower(v0_16b);
        }

        public PieceCountTuple RemoveMinimum()
        {
            var v0_16b = Vector128.CreateScalarUnsafe(medium).AsByte();
            v0_16b |= Vector128.CreateScalar(byte.MaxValue);
            uint min = VectorUtils.MinAcrossLower(v0_16b);
            var v3_16b = Vector128.Create((byte)min);
            v0_16b -= v3_16b;
            return new(v0_16b);
        }

        public static PieceCountTuple AddSaturate(PieceCountTuple left, PieceCountTuple right)
            => new(VectorUtils.AddSaturate(Vector128.CreateScalarUnsafe(left.medium).AsByte(), Vector128.CreateScalarUnsafe(right.medium).AsByte()));

        public static PieceCountTuple operator +(PieceCountTuple left, PieceCountTuple right)
        {
            var v0_8b = Vector128.CreateScalarUnsafe(left.medium).AsByte();
            var v1_8b = Vector128.CreateScalarUnsafe(right.medium).AsByte();
            return new(v0_8b + v1_8b);
        }

        public static bool operator ==(PieceCountTuple left, PieceCountTuple right) => left.Equals(right);
        public static bool operator !=(PieceCountTuple left, PieceCountTuple right) => !(left == right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetInternalValue() => medium;

        private string GetDebuggerDisplay()
        {
            var sb = new StringBuilder();
            var pieces = PiecesUtils.AllValidPieces;
            foreach (var item in pieces)
            {
                _ = sb.Append($"{item}: {this[item]}, ");
            }
            _ = sb.Append($"{Piece.None}: {this[Piece.None]}");
            return sb.ToString();
        }

        public override string ToString() => GetDebuggerDisplay();
        public override bool Equals(object? obj) => obj is PieceCountTuple tuple && Equals(tuple);
        public bool Equals(PieceCountTuple other) => MaskedValue == other.MaskedValue;
        public override int GetHashCode() => HashCode.Combine(MaskedValue);
        public bool ContainsKey(Piece key) => (uint)key < 8;
        public bool TryGetValue(Piece key, [MaybeNullWhen(false)] out byte value)
        {
            value = this[key];
            return (uint)key < 8;
        }
        public Enumerator GetEnumerator() => new(Value);

        IEnumerator<KeyValuePair<Piece, byte>> IEnumerable<KeyValuePair<Piece, byte>>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<KeyValuePair<Piece, byte>>)this).GetEnumerator();

        public struct Enumerator(ulong value) : IEnumerator<KeyValuePair<Piece, byte>>
        {
            private readonly ulong originalValue = value;
            private int index = -8;

            public readonly KeyValuePair<Piece, byte> Current => new((Piece)(index >>> 3), (byte)(originalValue >> index));

            readonly object IEnumerator.Current => Current;

            public void Dispose() => index = 0;
            public bool MoveNext()
            {
                var k = index += 8;
                return k < 64;
            }
            public void Reset() => index = -8;
        }
    }
}
