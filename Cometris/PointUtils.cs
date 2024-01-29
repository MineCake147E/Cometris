using System.Runtime.Intrinsics.X86;

using MikoMino;

namespace Cometris
{
    public static class PointUtils
    {
        public const long PointCompressionRange = 0x0000_003f_0000_000fL;
        public static uint Compress(this Point value)
        {
            var posv = value.Value;
            if (Bmi2.X64.IsSupported)
            {
                return (uint)Bmi2.X64.ParallelBitExtract((ulong)posv, PointCompressionRange);
            }
            posv &= PointCompressionRange;
            var posy = posv >>> 28;
            posv |= posy;
            return (uint)posv;
        }
        public static Point Expand(uint point)
        {
            if (Bmi2.X64.IsSupported)
            {
                return new((long)Bmi2.X64.ParallelBitDeposit(point, PointCompressionRange));
            }
            var posv = (ulong)(point & 0x03FFu);
            posv |= posv << 28;
            posv &= PointCompressionRange;
            return new((long)posv);
        }
    }
}
