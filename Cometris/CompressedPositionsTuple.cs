using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Cometris
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public readonly struct CompressedPositionsTuple : IReadOnlyList<Point>
    {
        public const long PointCompressionRange = PointUtils.PointCompressionRange;
        /// <summary>
        /// Bitfield: 0bnnaa_aaaa_bbbb_cccc_ccdd_ddee_eeee_ffff
        /// n: count
        /// a, c, e: y coordinate for item2, item1, item0, respectively.
        /// b, d, f: x coordinate for item2, item1, item0, respectively.
        /// </summary>
        private readonly uint bitfield;

        public int TotalLines
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get => (int)(bitfield >> 30);
        }

        public Point this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get
            {
                var shift = index * 10;
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)TotalLines);
                return PointUtils.Expand(bitfield >> shift);
            }
        }

        public static CompressedPositionsTuple Empty => new(0);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public CompressedPositionsTuple(Point item0, Point item1, Point item2)
        {
            var v0 = item0.Compress();
            var v1 = item1.Compress() << 10;
            var v2 = item2.Compress() << 20;
            v0 |= v1;
            v2 |= 3u << 30;
            bitfield = v0 | v2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public CompressedPositionsTuple(Point item0, Point item1)
        {
            var v0 = item0.Compress();
            var v1 = item1.Compress() << 10;
            v0 |= v1;
            v0 |= 2u << 30;
            bitfield = v0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public CompressedPositionsTuple(Point item0)
        {
            var v0 = item0.Compress();
            v0 |= 1u << 30;
            bitfield = v0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public CompressedPositionsTuple(uint bitfield)
        {
            this.bitfield = bitfield;
        }

        public IEnumerator<Point> GetEnumerator()
        {
            var count = TotalLines;
            var shift = 0;
            switch (count)
            {
                case 3:
                    yield return PointUtils.Expand(bitfield >> shift);
                    shift += 10;
                    goto case 2;
                case 2:
                    yield return PointUtils.Expand(bitfield >> shift);
                    shift += 10;
                    goto case 1;
                case 1:
                    yield return PointUtils.Expand(bitfield >> shift);
                    break;
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private string GetDebuggerDisplay() => string.Join(", ", this.Select(a => a.ToString()));
    }
}
