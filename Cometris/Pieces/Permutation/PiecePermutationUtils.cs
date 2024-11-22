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
        public static CompressedValuePieceList<uint> CreatePermutation(ushort id)
            => new(CalculatePermutation(id));

        public static CompressedValuePieceList<TStorage> CreatePermutation<TStorage>(ushort id) where TStorage : IBinaryInteger<TStorage>, IUnsignedNumber<TStorage>
            => new(TStorage.CreateTruncating(CalculatePermutation(id)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static uint CalculatePermutation(ushort id)
        {
            unchecked
            {
                uint k = id;
                var src = 0b0000_0111_0110_0101_0100_0011_0010_0001u;
                var res = 0u;
                var y0 = k * 613566757u;    // k / 7
                var y1 = k * 102261127u;    // k / 42
                var y2 = k * 20452226u;     // k / 210
                ulong x0 = ~(ulong)y0 + 1ul;
                var y4 = k * 1704353u;      // k / 2520
                var y3 = k * 5113057u;      // k / 840
                var y5 = k * 852177u;       // k / 5040
                x0 = y0 * 8ul + x0;   // y0 * 7
                var x1 = y1 * 2ul + y1;    // y1 * 6 >> 1
                var x2 = y2 * 4ul + y2;    // y2 * 5
                y0 = (uint)(x0 >> 32);
                y1 = (uint)(x1 >> 31);
                res |= ExtractAt(ref src, (int)y0);
                y2 = (uint)(x2 >> 32);
                var x4 = y4 * 2ul + y4;    // y4 * 3
                res |= ExtractAt(ref src, (int)y1) << 3;
                y3 >>= 30;                  // y3 * 4 >> 32
                res |= ExtractAt(ref src, (int)y2) << 6;
                y4 = (uint)(x4 >> 32);
                res |= ExtractAt(ref src, (int)y3) << 9;
                y5 >>= 31;                  // y5 * 2 >> 32
                res |= ExtractAt(ref src, (int)y4) << 12;
                res |= ExtractAtLastStep(ref src, (int)y5) << 15;
                res |= src << 18;
                return res;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static uint ExtractAt(ref uint src, int index)
            {
                var mi = index * 4;
                var n = src >> 4;
                // Workaround for https://github.com/dotnet/runtime/issues/61955 : use 64bit shift instead of 32bit shift
                var mask = nuint.MaxValue > uint.MaxValue ? (uint)((nuint)~0u << mi) : (~0u << mi);
                n &= mask;
                var p = src & ~mask;
                n |= p;
                var res = (src >> mi) & 0x0fu;
                src = n;
                return res;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static uint ExtractAtLastStep(ref uint src, int index)
            {
                var mi = index * 4;
                var mi2 = 4 - mi;
                var res = (src >> mi) & 0x0fu;
                var n = (src >> mi2) & 0x0fu;
                src = n;
                return res;
            }
        }
    }
}
