using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using MikoMino;

namespace Cometris
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public readonly struct CompressedPoint
    {
        public const uint Mask = 0x03FFu;

        /// <summary>
        /// Bitfield: 0b****_**YY_YYYY_XXXX<br/>
        /// *: Ignored<br/>
        /// Y: <see cref="Point.Y"/> in range [0,63]<br/>
        /// X: <see cref="Point.X"/> in range [0,15]<br/>
        /// </summary>
        private readonly ushort value;

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public CompressedPoint(int x, int y)
        {
            y &= 0x3F;
            var v = (uint)x & 0x0fu;
            v |= (uint)y << 4;
            value = (ushort)v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public CompressedPoint(Point position)
        {
            value = (ushort)position.Compress();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public CompressedPoint(ushort value)
        {
            this.value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public CompressedPoint(uint value)
        {
            this.value = (ushort)value;
        }

        public int X
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => unchecked(value & 0x0f);
        }

        public int Y
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => unchecked(value >>> 4);
        }

        public ushort Value => value;
        public uint MaskedValue => value & Mask;

        public Point AsPoint() => PointUtils.Expand(value);

        public static explicit operator Point(CompressedPoint value) => PointUtils.Expand(value.value);

        public static explicit operator CompressedPoint(Point value) => new(value);
        private string GetDebuggerDisplay() => $"<{X}, {Y}>";
        public override string? ToString() => GetDebuggerDisplay();
    }
}
