using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using MikoMino.Boards.LineSets;

namespace Cometris.Pieces.Hashing
{
    public static class PlacementHash
    {
        public static ulong CalculateHashCode<TPlacement>(this TPlacement placement)
            where TPlacement : unmanaged, IPiecePlacement<TPlacement>
            => TPlacement.CalculateHashCode(placement);

        public static ulong CalculateRangeLimitedHash(ulong value, ulong range)
        {
            switch (range)
            {
                case < 2:
                    return 0;
                case 65536:
                    return (ushort)value;
                default:
                    break;
            }
            value = value * 16369140280850176879 + 12763420880237146411;
            if ((range & (range - 1)) == 0)
            {
                var shift = BitOperations.LeadingZeroCount(range - 1);
                var mask = ulong.MaxValue >> shift;
                var k = (value & ~mask) >> (BitOperations.LeadingZeroCount(ulong.MinValue) - shift);
                value ^= k & ~(ulong)ushort.MaxValue;
                return value & mask;
            }
            return value % range;
        }
    }
}
