using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

using Cometris.Pieces.Hashing;

using MikoMino;

namespace Cometris.Pieces
{
    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public readonly struct CompressedPiecePlacement : IPiecePlacement<CompressedPiecePlacement>, IEquatable<CompressedPiecePlacement>
    {
        public const long PointCompressionRange = PointUtils.PointCompressionRange;

        /// <summary>
        /// Bitfield: 0bmmma_aayy_yyyy_xxxx<br/>
        /// m: <see cref="Piece"/><br/>
        /// a: <see cref="Angle"/> with support for symmetric behavior of I piece<br/>
        /// y: <see cref="Point.Y"/> for <see cref="Position"/> in range [0,63]<br/>
        /// x: <see cref="Point.X"/> for <see cref="Position"/> in range [0,15]<br/>
        /// Note that the x represents the value in the full range.
        /// </summary>
        private readonly ushort value;

        private const ushort PieceMask = 0b1110_0000_0000_0000;
        private const ushort AngleMask = 0b0001_1100_0000_0000;
        private const ushort PositionMask = 0b0000_0011_1111_1111;

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public CompressedPiecePlacement(ushort value)
        {
            this.value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public CompressedPiecePlacement(Point position)
        {
            var posv = position.Compress();
            value = (ushort)posv;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public CompressedPiecePlacement(Point position, Piece piece, Angle angle)
        {
            var posv = position.Compress();
            var pcv = (uint)piece & 0x7u;
            var anv = (uint)angle & 0x7u;
            pcv <<= 13;
            anv <<= 10;
            pcv |= anv;
            posv |= pcv;
            value = (ushort)posv;
        }

        public Piece Piece => (Piece)(value >> 13);
        public Point Position => ExtractPosition(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static Point ExtractPosition(ushort value) => PointUtils.Expand(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static (Piece piece, Angle angle) ExtractPieceAndAngle(ushort value)
        {
            var v = (uint)value >> 10;
            var y = v >> 3;
            var x = 0x07 & v;
            return ((Piece)y, (Angle)x);
        }

        public CompressedPiecePlacement WithPosition(Point position) => WithPosition((ushort)position.Compress());

        public CompressedPiecePlacement WithPosition(ushort compressedPosition)
        {
            var v = value & ~(uint)PositionMask;
            v |= compressedPosition & (uint)PositionMask;
            return new((ushort)v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static ulong CalculateHashCode(CompressedPiecePlacement value)
        {
            var v = value.value * 14982881503786724057ul + 11582317019772649373ul;
            var xmm0 = Vector128.Create(v);
            var xmm30 = Vector128.Create(14816230996000849003ul, 12605818395371984261ul);
            xmm0 = Pclmulqdq.CarrylessMultiply(xmm0, xmm30, 0x10);
            xmm0 ^= xmm30;
            var xmm31 = Vector128.Create(0x0f0f_0f0f_0f0f_0f0ful);
            var xmm29 = Vector128.Create(65, 81, 1, 17, 69, 16, 68, 20, 85, 80, 84, byte.MinValue, 21, 5, 64, 4);
            var xmm28 = Vector128.Create(170, 8, 128, 42, 160, 162, 130, 40, 168, 32, 34, 10, 2, 138, 0, 136);
            //var xmm27 = Vector128.Create(12, 4, 8, 10, 5, 14, 13, 3, 7, 6, 11, 1, 2, 15, 9, byte.MinValue);
            var xmm1 = Vector128.AndNot(xmm0, xmm31);
            xmm0 &= xmm31;
            xmm1 >>= 4;
            xmm0 = Ssse3.Shuffle(xmm29, xmm0.AsByte()).AsUInt64();
            xmm1 = Ssse3.Shuffle(xmm28, xmm1.AsByte()).AsUInt64();
            xmm0 ^= xmm1;
            xmm0 = Pclmulqdq.CarrylessMultiply(xmm0, xmm30, 0x10);
            xmm0 ^= xmm30;
            xmm1 = Vector128.AndNot(xmm0, xmm31);
            xmm0 &= xmm31;
            xmm1 >>= 4;
            xmm0 = Ssse3.Shuffle(xmm28, xmm0.AsByte()).AsUInt64();
            xmm1 = Ssse3.Shuffle(xmm29, xmm1.AsByte()).AsUInt64();
            xmm0 ^= xmm1;
            return xmm0.GetElement(0);
        }

        public Angle Angle => (Angle)((value >> 10) & 0x7);

        internal ushort Value => value;

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static explicit operator PiecePlacement(CompressedPiecePlacement value)
        {
            var v = value.value;
            var position = ExtractPosition(v);
            (var piece, var angle) = ExtractPieceAndAngle(v);
            return new(position, piece, angle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static explicit operator CompressedPiecePlacement(PiecePlacement value) => new(value.Position, value.Piece, value.Angle);

        public static bool operator ==(CompressedPiecePlacement left, CompressedPiecePlacement right) => left.Equals(right);
        public static bool operator !=(CompressedPiecePlacement left, CompressedPiecePlacement right) => !(left == right);

        private string GetDebuggerDisplay() => $"[{Piece}, {Angle}, {Position}]";
        public override string? ToString() => GetDebuggerDisplay();
        public override bool Equals(object? obj) => obj is CompressedPiecePlacement placement && Equals(placement);
        public bool Equals(CompressedPiecePlacement other) => value == other.value;
        public override int GetHashCode() => value.GetHashCode();
    }
}
