using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

using Cometris.Collections;
using Cometris.Utils;

namespace Cometris.Pieces.Permutation
{
    public static class PiecePermutationUtils
    {
        public static CompressedPieceList<TStorage> CreatePermutation<TStorage>(ushort id)
            where TStorage : IBinaryInteger<TStorage>, IUnsignedNumber<TStorage>
        {
            var s = Bmi2.IsSupported ? CalculatePermutationBmi2(id) : CalculatePermutationFallback(id);
            return new(TStorage.CreateTruncating(s));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static uint CalculatePermutationBmi2(ushort id)
        {
            unchecked
            {
                var remaining = (uint)CombinablePieces.All << 1;
                uint k = id;
                var r = 0u;
                if (k < 5040)
                {
                    var y0 = k * 613566757u;    // k / 7
                    ulong x0 = ~(ulong)y0 + 1ul;
                    var y1 = k * 102261127u;    // k / 42
                    var y2 = k * 20452226u;     // k / 210
                    var y4 = k * 1704353u;      // k / 2520
                    var y3 = k * 5113057u;      // k / 840
                    var y5 = k * 852177u;       // k / 5040
                    x0 = (ulong)y0 * 8u + x0;   // y0 * 7
                    var x1 = (ulong)y1 * 3u;    // y1 * 6 >> 1
                    var x2 = (ulong)y2 * 5u;    // y2 * 5
                    var x4 = (ulong)y4 * 3u;    // y4 * 3
                    y0 = (uint)(x0 >> 32);
                    y0 = 1u << (int)y0;
                    y0 = Bmi2.ParallelBitDeposit(y0, remaining);
                    remaining ^= y0;
                    y1 = (uint)(x1 >> 31);
                    y1 = 1u << (int)y1;
                    y1 = Bmi2.ParallelBitDeposit(y1, remaining);
                    r |= uint.TrailingZeroCount(y0);
                    remaining ^= y1;
                    y2 = (uint)(x2 >> 32);
                    y2 = 1u << (int)y2;
                    y2 = Bmi2.ParallelBitDeposit(y2, remaining);
                    r |= uint.TrailingZeroCount(y1) << 3;
                    remaining ^= y2;
                    y3 >>= 30;                  // y3 * 4 >> 32
                    y3 = 1u << (int)y3;
                    y3 = Bmi2.ParallelBitDeposit(y3, remaining);
                    r |= uint.TrailingZeroCount(y2) << 6;
                    remaining ^= y3;
                    y4 = (uint)(x4 >> 32);
                    y4 = 1u << (int)y4;
                    y4 = Bmi2.ParallelBitDeposit(y4, remaining);
                    r |= uint.TrailingZeroCount(y3) << 9;
                    remaining ^= y4;
                    y5 >>= 31;                  // y5 * 2 >> 32
                    y5 = 1u << (int)y5;
                    y5 = Bmi2.ParallelBitDeposit(y5, remaining);
                    r |= uint.TrailingZeroCount(y4) << 12;
                    remaining ^= y5;
                    r |= uint.TrailingZeroCount(y5) << 15;
                    r |= uint.TrailingZeroCount(remaining) << 18;
                }
                return r;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static uint CalculatePermutationFallback(ushort id)
        {
            unchecked
            {
                uint k = id;
                var res = 0b000_111_110_101_100_011_010_001u;
                if (k < 5040)
                {
                    var y0 = k * 613566757u;    // k / 7
                    ulong x0 = ~(ulong)y0 + 1ul;
                    var y1 = k * 102261127u;    // k / 42
                    var y2 = k * 20452226u;     // k / 210
                    var y4 = k * 1704353u;      // k / 2520
                    var y3 = k * 5113057u;      // k / 840
                    var y5 = k * 852177u;       // k / 5040
                    x0 = (ulong)y0 * 8u + x0;   // y0 * 7
                    var x1 = (ulong)y1 * 3u;    // y1 * 6 >> 1
                    var x2 = (ulong)y2 * 5u;    // y2 * 5
                    var x4 = (ulong)y4 * 3u;    // y4 * 3
                    y0 = (uint)(x0 >> 32);
                    res = ExchangePatterns(res, 0, (int)y0);
                    y1 = (uint)(x1 >> 31);
                    res = ExchangePatterns(res, 1, (int)y1 + 1);
                    y2 = (uint)(x2 >> 32);
                    res = ExchangePatterns(res, 2, (int)y2 + 2);
                    y3 >>= 30;                  // y3 * 4 >> 32
                    res = ExchangePatterns(res, 3, (int)y3 + 3);
                    y4 = (uint)(x4 >> 32);
                    res = ExchangePatterns(res, 4, (int)y4 + 4);
                    y5 >>= 31;                  // y5 * 2 >> 32
                    res = ExchangePatterns(res, 5, (int)y5 + 5);
                }
                return res;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint ExchangePatterns(uint bag, int a, int b)
        {
            a *= 3;
            b *= 3;
            var mask = (~0u << a) & ~(~0u << b);
            var value = (bag >> b) & 7;
            var y = (bag & mask) << 3;
            bag &= ~((mask << 3) | mask);
            bag |= y;
            return bag | (value << a);
        }
    }
}
