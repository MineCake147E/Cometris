using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Cometris.Collections
{
    public sealed class ListPool<T>
    {
        private const ulong SpecialSizeFlags = 0x1_0104_0102_0200;

        private static int MaxSize => (int)BitOperations.RoundUpToPowerOf2((uint)Array.MaxLength >>> 1);

        private readonly ConcurrentBag<List<T>>[] specialSizedPools = new ConcurrentBag<List<T>>[BitOperations.PopCount(SpecialSizeFlags)];

        private readonly ConcurrentBag<List<T>>[] powersOfTwoPools = new ConcurrentBag<List<T>>[1 + BitOperations.LeadingZeroCount(4u) - BitOperations.LeadingZeroCount(BitOperations.RoundUpToPowerOf2((uint)Array.MaxLength >>> 1))];

        internal ConcurrentBag<List<T>>? GetPoolOfSize(int size)
        {
            if ((uint)size > (uint)MaxSize)
            {
                ArgumentOutOfRangeException.ThrowIfNegative(size);
                ArgumentOutOfRangeException.ThrowIfGreaterThan(size, MaxSize);
                return null;
            }
            if (size < 64)
            {
                var s = SpecialSizeFlags >> size;
                var localSpecialSizedPools = specialSizedPools;
                var k = BitOperations.PopCount(SpecialSizeFlags) - BitOperations.PopCount(s);
                if ((s & 1) > 0 && (uint)k < (uint)localSpecialSizedPools.Length)
                {
                    return localSpecialSizedPools[k];
                }
            }
            var localPools = powersOfTwoPools;
            var l = int.Max(0, BitOperations.LeadingZeroCount(4u) - BitOperations.LeadingZeroCount((uint)size));
            return localPools[l];
        }
    }
}
