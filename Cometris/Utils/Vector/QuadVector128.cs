using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace Cometris.Utils.Vector
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly partial struct QuadVector128<T>(Vector128<T> v0, Vector128<T> v1, Vector128<T> v2, Vector128<T> v3) where T : unmanaged
    {
        public readonly Vector128<T> V0 = v0;
        public readonly Vector128<T> V1 = v1;
        public readonly Vector128<T> V2 = v2;
        public readonly Vector128<T> V3 = v3;

        public static QuadVector128<T> Zero => default;
        public static QuadVector128<T> AllBitsSet => ~Zero;
        public static QuadVector128<T> Indices => new(Vector512<T>.Indices);
        public static QuadVector128<T> IndicesPerLane => new(Vector128<T>.Indices);
        public static QuadVector128<T> One => new(Vector128<T>.One);

        public QuadVector128(Vector256<T> lower, Vector256<T> higher) : this(lower.GetLower(), lower.GetUpper(), higher.GetLower(), higher.GetUpper())
        {
        }
        public QuadVector128(Vector512<T> value) : this(value.GetLower(), value.GetUpper())
        {
        }
        public QuadVector128(Vector128<T> value) : this(value, value, value, value)
        {
        }

        public void Deconstruct(out Vector128<T> storage0, out Vector128<T> storage1, out Vector128<T> storage2, out Vector128<T> storage3)
            => (storage0, storage1, storage2, storage3) = (V0, V1, V2, V3);
    }

    public static partial class QuadVector128
    {
        public static byte GetElementConstant(this QuadVector128<byte> vector, [ConstantExpected(Max = 64, Min = 0)] int index)
        {
            index &= 63;
            var v0_64b = vector;
            var v1_16b = (index >> 4) switch
            {
                0 => v0_64b.V0,
                1 => v0_64b.V1,
                2 => v0_64b.V2,
                _ => v0_64b.V3
            };
            index &= 15;
            return v1_16b.GetElement(index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>~<paramref name="left"/> &amp; <paramref name="right"/></returns>
        public static QuadVector128<T> AndNot<T>(QuadVector128<T> left, QuadVector128<T> right) where T : unmanaged
            => ~left & right;

        public static QuadVector128<float> Permute4x32P3332(QuadVector128<float> vector)
        {
            if (Avx.IsSupported)
            {
                return new(Avx.Permute(vector.V0, 0b11_11_11_10), Avx.Permute(vector.V1, 0b11_11_11_10), Avx.Permute(vector.V2, 0b11_11_11_10), Avx.Permute(vector.V3, 0b11_11_11_10));
            }
            if (Sse2.IsSupported)
            {
                return new(Sse2.Shuffle(vector.V0.AsUInt32(), 0b11_11_11_10).AsSingle(), Sse2.Shuffle(vector.V1.AsUInt32(), 0b11_11_11_10).AsSingle(), Sse2.Shuffle(vector.V2.AsUInt32(), 0b11_11_11_10).AsSingle(), Sse2.Shuffle(vector.V3.AsUInt32(), 0b11_11_11_10).AsSingle());
            }
            if (Sse.IsSupported)
            {
                return new(Sse.Shuffle(vector.V0, vector.V0, 0b11_11_11_10), Sse.Shuffle(vector.V1, vector.V1, 0b11_11_11_10), Sse.Shuffle(vector.V2, vector.V2, 0b11_11_11_10), Sse.Shuffle(vector.V3, vector.V3, 0b11_11_11_10));
            }
            // Fallback
            return new(Vector128.Shuffle(vector.V0, Vector128.Create(3, 3, 3, 2)), Vector128.Shuffle(vector.V1, Vector128.Create(3, 3, 3, 2)), Vector128.Shuffle(vector.V2, Vector128.Create(3, 3, 3, 2)), Vector128.Shuffle(vector.V3, Vector128.Create(3, 3, 3, 2)));
        }
    }
}
