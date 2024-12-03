using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.Wasm;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace Cometris.Utils
{
    public static partial class VectorUtils
    {
        public static Vector128<byte> VectorTableLookup(Vector128<byte> table, Vector128<byte> indices)
        {
            if (AdvSimd.Arm64.IsSupported)
            {
                return AdvSimd.Arm64.VectorTableLookup(table, indices);
            }
            else if (Ssse3.IsSupported)
            {
                return Ssse3.Shuffle(table, indices);
            }
            else if (PackedSimd.IsSupported)
            {
                return PackedSimd.Swizzle(table, indices);
            }
            return Vector128.Shuffle(table, indices);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="mask"></param>
        /// <returns>mask ? right : left</returns>
        public static Vector128<ushort> BlendVariable(Vector128<ushort> left, Vector128<ushort> right, Vector128<ushort> mask)
        {
            if (AdvSimd.IsSupported)
            {
#pragma warning disable S2234 // Arguments should be passed in the same order as the method parameters
                return AdvSimd.BitwiseSelect(mask, right, left);
#pragma warning restore S2234 // Arguments should be passed in the same order as the method parameters
            }
            if (Sse41.IsSupported)
            {
                return Sse41.BlendVariable(left, right, mask);
            }
            // Fallback
#pragma warning disable S2234 // Arguments should be passed in the same order as the method parameters
            return Vector128.ConditionalSelect(mask, right, left);
#pragma warning restore S2234 // Arguments should be passed in the same order as the method parameters
        }

        public static Vector128<byte> BlendVariable(Vector128<byte> left, Vector128<byte> right, Vector128<byte> mask)
        {
            if (AdvSimd.IsSupported)
            {
#pragma warning disable S2234 // Arguments should be passed in the same order as the method parameters
                return AdvSimd.BitwiseSelect(mask, right, left);
#pragma warning restore S2234 // Arguments should be passed in the same order as the method parameters
            }
            if (Sse41.IsSupported)
            {
                return Sse41.BlendVariable(left, right, mask);
            }
            // Fallback
#pragma warning disable S2234 // Arguments should be passed in the same order as the method parameters
            return Vector128.ConditionalSelect(mask, right, left);
#pragma warning restore S2234 // Arguments should be passed in the same order as the method parameters
        }
        public static ushort GetElementVariable(this Vector128<ushort> value, int index)
        {
            var v0_8h = value;
            if (AdvSimd.IsSupported)
            {
                var v1_4h = Vector64.Create((byte)(index * 2)) + Vector64<byte>.Indices;
                return AdvSimd.VectorTableLookup(v0_8h.AsByte(), v1_4h).AsUInt16().GetElement(0);
            }
            if (Ssse3.IsSupported)
            {
                var xmm1 = Vector128.Create((byte)(index * 2)) + Vector128<byte>.Indices;
                return Ssse3.Shuffle(v0_8h.AsByte(), xmm1).AsUInt16().GetElement(0);
            }
            // Fallback
            return v0_8h.GetElement(index);
        }

        public static Vector128<ushort> PopCountPerLine(Vector128<ushort> value)
        {
            var v7_16b = Vector128.Create((byte)0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4);
            var v6_16b = Vector128.Create((byte)0x0f);
            var v5_16b = Vector128.Create((ushort)0xff).AsByte();
            var v0_16b = value.AsByte();
            var v1_16b = (v0_16b.AsUInt16() >> 4).AsByte();
            v0_16b &= v6_16b;
            v1_16b &= v6_16b;
            v0_16b = VectorTableLookup(v7_16b, v0_16b);
            v1_16b = VectorTableLookup(v7_16b, v1_16b);
            v0_16b += v1_16b;
            v1_16b = (v0_16b.AsUInt16() >> 8).AsByte();
            v0_16b &= v5_16b;
            v0_16b += v1_16b;
            return v0_16b.AsUInt16();
        }

        #region Arithmetics

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<byte> AddSaturate(Vector128<byte> left, Vector128<byte> right)
        {
            if (AdvSimd.IsSupported)
            {
                return AdvSimd.AddSaturate(left, right);
            }
            if (PackedSimd.IsSupported)
            {
                return PackedSimd.AddSaturate(left, right);
            }
            if (Sse2.IsSupported)
            {
                return Sse2.AddSaturate(left, right);
            }
            var v0_16b = left + right;
            var v1_16b = Vector128.LessThan(v0_16b, left);
            v0_16b |= v1_16b;
            return v0_16b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<byte> SubtractSaturate(Vector128<byte> left, Vector128<byte> right)
        {
            if (AdvSimd.IsSupported)
            {
                return AdvSimd.SubtractSaturate(left, right);
            }
            if (PackedSimd.IsSupported)
            {
                return PackedSimd.SubtractSaturate(left, right);
            }
            if (Sse2.IsSupported)
            {
                return Sse2.SubtractSaturate(left, right);
            }
            var v0_16b = left - right;
            var v1_16b = Vector128.LessThanOrEqual(v0_16b, left);
            v0_16b &= v1_16b;
            return v0_16b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<ushort> MultiplyAdd(Vector128<ushort> addend, Vector128<ushort> left, Vector128<ushort> right)
            => AdvSimd.IsSupported ? AdvSimd.MultiplyAdd(addend, left, right) : addend + left * right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<ushort> MultiplyHigh(Vector128<ushort> left, Vector128<ushort> right)
        {
            if (Sse2.IsSupported)
            {
                return Sse2.MultiplyHigh(left, right);
            }
            var v0_8h = left;
            var v1_8h = right;
            var v2_4s = MultiplyWideningLower(v0_8h, v1_8h);
            var v3_4s = MultiplyWideningUpper(v0_8h, v1_8h);
            if (AdvSimd.IsSupported)
            {
                var v2_4h = AdvSimd.ShiftRightLogicalNarrowingLower(v2_4s, 16);
                return AdvSimd.ShiftRightLogicalNarrowingSaturateUpper(v2_4h, v3_4s, 16);
            }
            v2_4s >>= 16;
            v3_4s >>= 16;
            if (PackedSimd.IsSupported)
            {
                return PackedSimd.ConvertNarrowingSaturateUnsigned(v2_4s.AsInt32(), v3_4s.AsInt32());
            }
            else
            {
                // Fallback
                return Vector128.Narrow(v2_4s, v3_4s);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<uint> MultiplyWideningLower(Vector128<ushort> left, Vector128<ushort> right)
        {
            if (AdvSimd.IsSupported)
            {
                return AdvSimd.MultiplyWideningLower(left.GetLower(), right.GetLower());
            }
            if (PackedSimd.IsSupported)
            {
                return PackedSimd.MultiplyWideningLower(left, right);
            }
            // Fallback
            return Vector128.WidenLower(left) * Vector128.WidenLower(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<uint> MultiplyWideningUpper(Vector128<ushort> left, Vector128<ushort> right)
        {
            if (AdvSimd.IsSupported)
            {
                return AdvSimd.MultiplyWideningUpper(left, right);
            }
            if (PackedSimd.IsSupported)
            {
                return PackedSimd.MultiplyWideningUpper(left, right);
            }
            // Fallback
            return Vector128.WidenUpper(left) * Vector128.WidenUpper(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<byte> NarrowLower(Vector128<ushort> vector)
        {
            if (Avx512BW.VL.IsSupported)
            {
                return Avx512BW.VL.ConvertToVector128Byte(vector);
            }
            if (Sse2.IsSupported)
            {
                return Sse2.PackUnsignedSaturate(vector.AsInt16(), default);
            }
            if (AdvSimd.IsSupported)
            {
                return AdvSimd.ExtractNarrowingLower(vector).ToVector128();
            }
            // Fallback
            return Vector128.Narrow(vector, default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<byte> NarrowLowerUnsafe(Vector128<ushort> vector)
        {
            if (Avx512BW.VL.IsSupported)
            {
                return Avx512BW.VL.ConvertToVector128Byte(vector);
            }
            if (Sse2.IsSupported)
            {
                return Sse2.PackUnsignedSaturate(vector.AsInt16(), vector.AsInt16());
            }
            if (AdvSimd.IsSupported)
            {
                return AdvSimd.ExtractNarrowingLower(vector).ToVector128();
            }
            // Fallback
            return Vector128.Narrow(vector, vector);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint MinAcrossLower(Vector128<byte> vector)
        {
            var v0_16b = vector;
            if (Sse41.IsSupported)
            {
                var xmm0 = Sse41.ConvertToVector128Int16(v0_16b).AsUInt16();
                xmm0 = Sse41.MinHorizontal(xmm0);
                return xmm0.GetElement(0);
            }
            else if (AdvSimd.Arm64.IsSupported)
            {
                return AdvSimd.Arm64.MinAcross(v0_16b.GetLower()).GetElement(0);
            }
            else if (AdvSimd.IsSupported)
            {
                var v0_8b = v0_16b.GetLower();
                v0_8b = AdvSimd.MinPairwise(v0_8b, v0_8b);
                v0_8b = AdvSimd.MinPairwise(v0_8b, v0_8b);
                v0_8b = AdvSimd.MinPairwise(v0_8b, v0_8b);
                return v0_8b.GetElement(0);
            }
            else
            {
                var v1_16b = v0_16b.AsUInt64() >> 32;
                var v2_16b = Vector128.Min(v0_16b, v1_16b.AsByte());
                v1_16b = v2_16b.AsUInt64() >> 16;
                v2_16b = Vector128.Min(v2_16b, v1_16b.AsByte());
                v1_16b = v2_16b.AsUInt64() >> 8;
                v2_16b = Vector128.Min(v2_16b, v1_16b.AsByte());
                return v2_16b.GetElement(0);
            }
        }

        #endregion

        #region Broadcast
        internal static Vector128<byte> BroadcastFirstElementByte(Vector128<byte> vector)
        {
            if (Avx2.IsSupported)
            {
                return Avx2.BroadcastScalarToVector128(vector);
            }
            if (Ssse3.IsSupported)
            {
                return Ssse3.Shuffle(vector, Vector128<byte>.Zero);
            }
            if (AdvSimd.IsSupported)
            {
                return AdvSimd.DuplicateSelectedScalarToVector128(vector, 0);
            }
            // Fallback
            return Vector128.Create(vector.GetElement(0));
        }

        internal static Vector128<sbyte> BroadcastFirstElementSByte(Vector128<sbyte> vector)
        {
            if (Avx2.IsSupported)
            {
                return Avx2.BroadcastScalarToVector128(vector);
            }
            if (Ssse3.IsSupported)
            {
                return Ssse3.Shuffle(vector, Vector128<sbyte>.Zero);
            }
            if (AdvSimd.IsSupported)
            {
                return AdvSimd.DuplicateSelectedScalarToVector128(vector, 0);
            }
            // Fallback
            return Vector128.Create(vector.GetElement(0));
        }

        internal static Vector128<ushort> BroadcastFirstElementUInt16(Vector128<ushort> vector)
        {
            if (Avx2.IsSupported)
            {
                return Avx2.BroadcastScalarToVector128(vector);
            }
            if (Ssse3.IsSupported)
            {
                return Ssse3.Shuffle(vector.AsByte(), Vector128<byte>.Indices & Vector128<byte>.One).AsUInt16();
            }
            if (AdvSimd.IsSupported)
            {
                return AdvSimd.DuplicateSelectedScalarToVector128(vector, 0);
            }
            // Fallback
            return Vector128.Create(vector.GetElement(0));
        }

        internal static Vector128<short> BroadcastFirstElementInt16(Vector128<short> vector)
        {
            if (Avx2.IsSupported)
            {
                return Avx2.BroadcastScalarToVector128(vector);
            }
            if (Ssse3.IsSupported)
            {
                return Ssse3.Shuffle(vector.AsByte(), Vector128<byte>.Indices & Vector128<byte>.One).AsInt16();
            }
            if (AdvSimd.IsSupported)
            {
                return AdvSimd.DuplicateSelectedScalarToVector128(vector, 0);
            }
            // Fallback
            return Vector128.Create(vector.GetElement(0));
        }

        internal static Vector128<uint> BroadcastFirstElementUInt32(Vector128<uint> vector)
        {
            if (Avx2.IsSupported)
            {
                return Avx2.BroadcastScalarToVector128(vector);
            }
            if (Sse2.IsSupported)
            {
                return Sse2.Shuffle(vector, 0);
            }
            if (Sse.IsSupported)
            {
                return Sse.Shuffle(vector.AsSingle(), vector.AsSingle(), 0).AsUInt32();
            }
            if (AdvSimd.IsSupported)
            {
                return AdvSimd.DuplicateSelectedScalarToVector128(vector, 0);
            }
            // Fallback
            return Vector128.Create(vector.GetElement(0));
        }

        internal static Vector128<int> BroadcastFirstElementInt32(Vector128<int> vector)
        {
            if (Avx2.IsSupported)
            {
                return Avx2.BroadcastScalarToVector128(vector);
            }
            if (Sse2.IsSupported)
            {
                return Sse2.Shuffle(vector, 0);
            }
            if (Sse.IsSupported)
            {
                return Sse.Shuffle(vector.AsSingle(), vector.AsSingle(), 0).AsInt32();
            }
            if (AdvSimd.IsSupported)
            {
                return AdvSimd.DuplicateSelectedScalarToVector128(vector, 0);
            }
            // Fallback
            return Vector128.Create(vector.GetElement(0));
        }

        internal static Vector128<float> BroadcastFirstElementSingle(Vector128<float> vector)
        {
            if (Avx2.IsSupported)
            {
                return Avx2.BroadcastScalarToVector128(vector);
            }
            if (Avx.IsSupported)
            {
                return Avx.Permute(vector, 0);
            }
            if (Sse.IsSupported)
            {
                return Sse.Shuffle(vector, vector, 0);
            }
            if (AdvSimd.IsSupported)
            {
                return AdvSimd.DuplicateSelectedScalarToVector128(vector, 0);
            }
            // Fallback
            return Vector128.Create(vector.GetElement(0));
        }

        internal static Vector128<ulong> BroadcastFirstElementUInt64(Vector128<ulong> vector)
        {
            if (Avx2.IsSupported)
            {
                return Avx2.BroadcastScalarToVector128(vector);
            }
            if (Avx.IsSupported)
            {
                return Avx.Permute(vector.AsDouble(), 0).AsUInt64();
            }
            if (AdvSimd.Arm64.IsSupported)
            {
                return AdvSimd.Arm64.DuplicateSelectedScalarToVector128(vector, 0);
            }
            // Fallback
            return Vector128.Create(vector.GetElement(0));
        }

        internal static Vector128<long> BroadcastFirstElementInt64(Vector128<long> vector)
        {
            if (Avx2.IsSupported)
            {
                return Avx2.BroadcastScalarToVector128(vector);
            }
            if (Avx.IsSupported)
            {
                return Avx.Permute(vector.AsDouble(), 0).AsInt64();
            }
            if (AdvSimd.Arm64.IsSupported)
            {
                return AdvSimd.Arm64.DuplicateSelectedScalarToVector128(vector, 0);
            }
            // Fallback
            return Vector128.Create(vector.GetElement(0));
        }

        internal static Vector128<double> BroadcastFirstElementDouble(Vector128<double> vector)
        {
            if (Avx2.IsSupported)
            {
                return Avx2.BroadcastScalarToVector128(vector);
            }
            if (Avx.IsSupported)
            {
                return Avx.Permute(vector, 0);
            }
            if (AdvSimd.Arm64.IsSupported)
            {
                return AdvSimd.Arm64.DuplicateSelectedScalarToVector128(vector, 0);
            }
            // Fallback
            return Vector128.Create(vector.GetElement(0));
        }

        #endregion

    }
}
