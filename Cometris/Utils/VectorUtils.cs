using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.Wasm;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace Cometris.Utils
{
    public static class VectorUtils
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
                var v1_4h = Vector64.Create((byte)(index * 2));
                return AdvSimd.VectorTableLookup(v0_8h.AsByte(), v1_4h).AsUInt16().GetElement(0);
            }
            if (Ssse3.IsSupported)
            {
                var xmm1 = Vector128.Create((byte)(index * 2));
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
    }
}
