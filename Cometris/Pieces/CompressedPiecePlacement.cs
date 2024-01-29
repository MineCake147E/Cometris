using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cometris.Pieces
{
    [StructLayout(LayoutKind.Explicit, Size = sizeof(ushort))]
    public readonly struct CompressedPiecePlacement : IPiecePlacement<CompressedPiecePlacement>
    {
        public const long PointCompressionRange = PointUtils.PointCompressionRange;

        /// <summary>
        /// Bitfield: 0bmmma_aayy_yyyy_xxxx<br/>
        /// m: <see cref="Piece"/><br/>
        /// a: <see cref="Angle"/><br/>
        /// y: <see cref="Point.Y"/> for <see cref="Position"/> in range [0,63]<br/>
        /// x: <see cref="Point.X"/> for <see cref="Position"/> in range [0,15]<br/>
        /// Note that the x represents the value in the full range.
        /// </summary>
        [FieldOffset(0)]
        private readonly ushort value;

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
    }
}
