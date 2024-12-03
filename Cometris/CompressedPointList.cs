using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

using MikoMino;

namespace Cometris
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public readonly struct CompressedPointList : IReadOnlyList<CompressedPoint>
    {
        public const long PointCompressionRange = PointUtils.PointCompressionRange;
        /// <summary>
        /// Bitfield: 0bNNAA_AAAA_BBBB_CCCC_CCDD_DDEE_EEEE_FFFF<br/>
        /// N: count<br/>
        /// A, C, E: y coordinate for item2, item1, item0, respectively.<br/>
        /// B, D, F: x coordinate for item2, item1, item0, respectively.<br/>
        /// </summary>
        private readonly uint bitfield;

        public int TotalLines
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get => (int)(bitfield >> 30);
        }

        public CompressedPoint this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get
            {
                var shift = index * 10;
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)TotalLines);
                return new(bitfield >> shift);
            }
        }

        public static CompressedPointList Empty => new(0);

        public int Count => TotalLines;

        public uint Value => bitfield;

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public CompressedPointList(Point item0, Point item1, Point item2)
        {
            var v0 = item0.Compress();
            var v1 = item1.Compress() << 10;
            var v2 = item2.Compress() << 20;
            v0 |= v1;
            v2 |= 3u << 30;
            bitfield = v0 | v2;
        }

        [OverloadResolutionPriority(1)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public CompressedPointList(CompressedPoint item0, CompressedPoint item1, CompressedPoint item2)
        {
            var v0 = item0.MaskedValue;
            var v1 = item1.MaskedValue << 10;
            var v2 = item2.MaskedValue << 20;
            v0 |= v1;
            v2 |= 3u << 30;
            bitfield = v0 | v2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public CompressedPointList(Point item0, Point item1)
        {
            var v0 = item0.Compress();
            var v1 = item1.Compress() << 10;
            v0 |= v1;
            v0 |= 2u << 30;
            bitfield = v0;
        }

        [OverloadResolutionPriority(1)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public CompressedPointList(CompressedPoint item0, CompressedPoint item1)
        {
            var v0 = item0.MaskedValue;
            var v1 = item1.MaskedValue << 10;
            v0 |= v1;
            v0 |= 2u << 30;
            bitfield = v0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public CompressedPointList(Point item0)
        {
            var v0 = item0.Compress();
            v0 |= 1u << 30;
            bitfield = v0;
        }

        [OverloadResolutionPriority(1)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public CompressedPointList(CompressedPoint item0)
        {
            var v0 = item0.MaskedValue;
            v0 |= 1u << 30;
            bitfield = v0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public CompressedPointList(uint bitfield)
        {
            this.bitfield = bitfield;
        }

        public Enumerator GetEnumerator() => new(this);

        IEnumerator<CompressedPoint> IEnumerable<CompressedPoint>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private string GetDebuggerDisplay() => string.Join(", ", this.Select(a => a.ToString()));
        public override string? ToString() => GetDebuggerDisplay();

        public struct Enumerator(CompressedPointList value) : IEnumerator<CompressedPoint>
        {
            private readonly uint value = value.bitfield;
            private readonly int maxShift = value.Count * 10;
            private int shift = -10;

            public readonly CompressedPoint Current => new((value >> shift) & CompressedPoint.Mask);

            readonly object IEnumerator.Current => Current;

            public void Dispose() => shift = 31;
            public bool MoveNext()
            {
                var s = shift;
                s += 10;
                shift = s;
                return s < maxShift;
            }
            public void Reset() => shift = -10;
        }
    }

    public static class CompressedPointListExtensions
    {

        public static bool TryAdd(this ref CompressedPointList list, CompressedPoint point, ref CompressedPointList excess)
        {
            var cl = list.Value;
            var nl = cl + (1u << 30);
            var xl = point.MaskedValue;
            var shift = nl >> 30;
            xl <<= (int)shift;
            nl |= xl;
            excess = new(nl);
            if (nl >= cl)
            {
                list = new(nl);
            }
            return nl >= cl;
        }
    }
}
