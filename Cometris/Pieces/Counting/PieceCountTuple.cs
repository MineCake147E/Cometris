using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Cometris.Pieces.Counting
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public readonly struct PieceCountTuple : IEquatable<PieceCountTuple>
    {
        private readonly double medium;

        public ulong Value => BitConverter.DoubleToUInt64Bits(medium);

        public ulong MaskedValue => Value & ~0xfful;

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

        public static PieceCountTuple CreateCountFrom<TEnumerable>(TEnumerable pieces) where TEnumerable : IEnumerable<Piece>
        {
            var m = new PieceCountTuple();
            foreach (var item in pieces)
            {
                m = m.AddPieces(item);
            }
            return m;
        }

        public byte this[Piece piece]
        {
            get
            {
                var x8 = (byte)piece * 8;
                var x9 = BitConverter.DoubleToUInt64Bits(medium);
                return (byte)(x9 >> x8);
            }
        }

        public readonly PieceCountTuple AddPieces(Piece piece, sbyte count = 1)
        {
            var x8 = (byte)piece * 8;
            var x9 = (ulong)(byte)count << x8;
            var v0_8b = Vector128.CreateScalarUnsafe(medium).AsByte();
            var v1_8b = Vector128.CreateScalarUnsafe(x9).AsByte();
            return new(v0_8b + v1_8b);
        }

        public readonly PieceCountTuple WithPiece(Piece piece, sbyte count)
        {
            var v0_8b = Vector128.CreateScalarUnsafe(medium).AsByte();
            var v1_8b = Vector128.Create((byte)piece);
            var v2_8b = Vector128.Create((byte)count);
            v1_8b = Vector128.Equals(v1_8b, Vector128<byte>.Indices);
            return new(Vector128.ConditionalSelect(v1_8b, v2_8b, v0_8b));
        }

        public PieceCountTuple RemoveMinimum()
        {
            var v0_16b = Vector128.CreateScalarUnsafe(medium).AsByte();
            v0_16b |= Vector128.CreateScalar(byte.MaxValue);
            int min;
            if (Sse41.IsSupported)
            {
                var xmm0 = Sse41.ConvertToVector128Int16(v0_16b).AsUInt16();
                xmm0 = Sse41.MinHorizontal(xmm0);
                min = xmm0.GetElement(0);
            }
            else if (AdvSimd.Arm64.IsSupported)
            {
                min = AdvSimd.Arm64.MinAcross(v0_16b).GetElement(0);
            }
            else if (AdvSimd.IsSupported)
            {
                var v0_8b = v0_16b.GetLower();
                v0_8b = AdvSimd.MinPairwise(v0_8b, v0_8b);
                v0_8b = AdvSimd.MinPairwise(v0_8b, v0_8b);
                v0_8b = AdvSimd.MinPairwise(v0_8b, v0_8b);
                min = v0_8b.GetElement(0);
            }
            else
            {
                var v1_16b = v0_16b.AsUInt64() >> 32;
                var v2_16b = Vector128.Min(v0_16b, v1_16b.AsByte());
                v1_16b = v2_16b.AsUInt64() >> 16;
                v2_16b = Vector128.Min(v2_16b, v1_16b.AsByte());
                v1_16b = v2_16b.AsUInt64() >> 8;
                v2_16b = Vector128.Min(v2_16b, v1_16b.AsByte());
                min = v2_16b.GetElement(0);
            }
            var v3_16b = Vector128.Create((byte)min);
            v0_16b -= v3_16b;
            return new(v0_16b);
        }

        public static PieceCountTuple operator +(PieceCountTuple left, PieceCountTuple right)
        {
            var v0_8b = Vector128.CreateScalarUnsafe(left.medium).AsByte();
            var v1_8b = Vector128.CreateScalarUnsafe(right.medium).AsByte();
            return new(v0_8b + v1_8b);
        }

        public static bool operator ==(PieceCountTuple left, PieceCountTuple right) => left.Equals(right);
        public static bool operator !=(PieceCountTuple left, PieceCountTuple right) => !(left == right);

        public double GetInternalValue() => medium;

        private string GetDebuggerDisplay()
        {
            var sb = new StringBuilder();
            ReadOnlySpan<Piece> pieces = [Piece.T, Piece.I, Piece.O, Piece.J, Piece.L, Piece.S, Piece.Z];
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
    }
}
