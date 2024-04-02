using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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
                default:
                    break;
            }
            value = value * 16369140280850176879 + 12763420880237146411;
            if ((value & (value - 1)) == 0)
            {
                var shift = BitOperations.LeadingZeroCount(value - 1);
                value >>= shift;
                return value;
            }
            return value % range;
        }
    }
}
