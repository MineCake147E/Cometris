using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

using Shamisen;

namespace Cometris.Boards
{
    /// <summary>
    /// Bit board that records all 40 lines.<br/>
    /// This structure is for exceptional hardware-accelerated board operations.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = sizeof(ushort) * Height)]
    public readonly struct FullBitBoard
    {
        public const int Height = 40;

        public const ushort EmptyRow = 0xE007;

        public const ushort FullRow = ushort.MaxValue;

        public const ushort InvertedEmptyRow = 0x1ff8;

        public const ushort InvertedFullRow = ushort.MinValue;

        #region Constructors
        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public FullBitBoard(ushort defaultRowValue)
        {
            Unsafe.SkipInit(out this);
            ref var x9 = ref Unsafe.As<FullBitBoard, ushort>(ref Unsafe.AsRef(in this));
#if NET8_0_OR_GREATER
            if (Vector<ushort>.Count == Vector512<ushort>.Count)
            {
                var v0 = Vector512.Create(defaultRowValue);
                v0.StoreUnsafe(ref Unsafe.Add(ref x9, 0));
                v0.GetLower().GetLower().StoreUnsafe(ref Unsafe.Add(ref x9, 32));
                return;
            }
#endif
            if (Vector<ushort>.Count == Vector256<ushort>.Count)
            {
                var v0 = Vector256.Create(defaultRowValue);
                v0.StoreUnsafe(ref Unsafe.Add(ref x9, 0));
                v0.StoreUnsafe(ref Unsafe.Add(ref x9, 16));
                v0.GetLower().StoreUnsafe(ref Unsafe.Add(ref x9, 32));
            }
            else
            {
                var a = Vector128.Create(defaultRowValue);
                Unsafe.As<ushort, Vector128<ushort>>(ref Unsafe.Add(ref x9, 0)) = a;
                Unsafe.As<ushort, Vector128<ushort>>(ref Unsafe.Add(ref x9, 8)) = a;
                Unsafe.As<ushort, Vector128<ushort>>(ref Unsafe.Add(ref x9, 16)) = a;
                Unsafe.As<ushort, Vector128<ushort>>(ref Unsafe.Add(ref x9, 24)) = a;
                Unsafe.As<ushort, Vector128<ushort>>(ref Unsafe.Add(ref x9, 32)) = a;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public FullBitBoard() : this(EmptyRow)
        {
        }

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static OperationStatus ReadFromArray(ReadOnlySpan<ushort> region, out FullBitBoard output)
        {
            if (region.Length < Height)
            {
                return OperationStatus.NeedMoreData;
            }
            CopyRegionToBitBoard(ref MemoryMarshal.GetReference(region), ref output);
            return OperationStatus.Done;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static void CopyRegionToBitBoard(ref ushort head, ref FullBitBoard output)
        {
            ref var x9 = ref Unsafe.As<FullBitBoard, ushort>(ref output);
            if (Vector<ushort>.Count == Vector256<ushort>.Count)
            {
                var v0 = Vector256.LoadUnsafe(ref head, 0 * (nuint)Vector256<ushort>.Count);
                var v1 = Vector256.LoadUnsafe(ref head, 1 * (nuint)Vector256<ushort>.Count);
                var v2 = Vector128.LoadUnsafe(ref head, 4 * (nuint)Vector128<ushort>.Count);
                Unsafe.As<ushort, Vector256<ushort>>(ref Unsafe.Add(ref x9, 0)) = v0;
                Unsafe.As<ushort, Vector256<ushort>>(ref Unsafe.Add(ref x9, 16)) = v1;
                Unsafe.As<ushort, Vector128<ushort>>(ref Unsafe.Add(ref x9, 32)) = v2;
            }
            else
            {
                var v0 = Vector128.LoadUnsafe(ref head, 0 * (nuint)Vector128<ushort>.Count);
                var v1 = Vector128.LoadUnsafe(ref head, 1 * (nuint)Vector128<ushort>.Count);
                var v2 = Vector128.LoadUnsafe(ref head, 2 * (nuint)Vector128<ushort>.Count);
                var v3 = Vector128.LoadUnsafe(ref head, 3 * (nuint)Vector128<ushort>.Count);
                var v4 = Vector128.LoadUnsafe(ref head, 4 * (nuint)Vector128<ushort>.Count);
                v0.StoreUnsafe(ref x9, 0 * (nuint)Vector128<ushort>.Count);
                v1.StoreUnsafe(ref x9, 1 * (nuint)Vector128<ushort>.Count);
                v2.StoreUnsafe(ref x9, 2 * (nuint)Vector128<ushort>.Count);
                v3.StoreUnsafe(ref x9, 3 * (nuint)Vector128<ushort>.Count);
                v4.StoreUnsafe(ref x9, 4 * (nuint)Vector128<ushort>.Count);
            }
        }
        #endregion
        /// <summary>
        /// Gets the "raw" data of row at y = <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The y coordinate.</param>
        /// <returns>The raw data of row at y = <paramref name="index"/> in the format like ###0123456789###.</returns>
        public ushort this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get
            {
                nint x9 = index;
                var bb = index >> 31;
                var value = EmptyRow;
                index |= bb;
                value = (ushort)(value | bb);
                if ((uint)index < Height)   //Also jumps if index is less than 0
                {
                    value = Unsafe.Add(ref Unsafe.As<FullBitBoard, ushort>(ref Unsafe.AsRef(in this)), x9);
                }
                return value;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if (index is >= 0 and < Height) Unsafe.Add(ref Unsafe.As<FullBitBoard, ushort>(ref Unsafe.AsRef(in this)), index) = value;
            }
        }

        /// <summary>
        /// Returns whether the block is at (<paramref name="x"/>,<paramref name="y"/>), or not.
        /// </summary>
        /// <param name="y">The y coordinate.</param>
        /// <param name="x">The x coordinate.</param>
        /// <returns><see langword="true"/> if there is a block at coordinate (<paramref name="x"/>,<paramref name="y"/>), otherwise, <see langword="false"/>.</returns>
        public bool this[int y, int x]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get => this[y] << x == 0x1000;
        }
    }
}
