using System.Runtime.Intrinsics.X86;

using MikoMino;

namespace Cometris
{
    public static class PointUtils
    {
        public const long PointCompressionRange = 0x0000_003f_0000_000fL;
        public static uint Compress(this Point value)
        {
            var v = value.Value;
            if (Bmi2.X64.IsSupported)
            {
                return (uint)Bmi2.X64.ParallelBitExtract((ulong)v, PointCompressionRange);
            }
            v &= PointCompressionRange;
            var posy = v >>> 28;
            v |= posy;
            return (uint)v;
        }
        public static Point Expand(uint point)
        {
            unchecked
            {
                if (Bmi2.X64.IsSupported)
                {
                    return new((long)Bmi2.X64.ParallelBitDeposit(point, PointCompressionRange));
                }
                var v = (ulong)(point & 0x03FFu);
                v |= v << 28;
                v &= PointCompressionRange;
                return new((long)v);
            }
        }
    }
}
