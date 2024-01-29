using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

using Shamisen;

namespace Cometris.Boards
{
    /// <summary>
    /// Bit board that records only the bottom 24 out of 40 steps.<br/>
    /// This structure is for recording and does not support board operations.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = sizeof(ushort) * Height)]
    public readonly struct PartialBitBoard384
    {
        /// <summary>
        /// 24 = <see cref="Vector256"/>&lt;ushort&gt;.Count + <see cref="Vector128"/>&lt;ushort&gt;.Count
        /// </summary>
        public const int Height = 24;
        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="PartialBitBoard384"/> where all rows have <paramref name="defaultRowValue"/> as their raw value.
        /// </summary>
        /// <param name="defaultRowValue">The default raw row value.</param>
        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public PartialBitBoard384(ushort defaultRowValue)
        {
            ref var x9 = ref Unsafe.As<PartialBitBoard384, ushort>(ref Unsafe.AsRef(in this));
            if (Vector<ushort>.Count == 16)
            {
                var v0 = Vector256.Create(defaultRowValue);
                Unsafe.As<ushort, Vector256<ushort>>(ref Unsafe.Add(ref x9, 0)) = v0;
                Unsafe.As<ushort, Vector128<ushort>>(ref Unsafe.Add(ref x9, 16)) = v0.GetLower();
            }
            else
            {
                var a = Vector128.Create(defaultRowValue);
                Unsafe.As<ushort, Vector128<ushort>>(ref Unsafe.Add(ref x9, 0)) = a;
                Unsafe.As<ushort, Vector128<ushort>>(ref Unsafe.Add(ref x9, 8)) = a;
                Unsafe.As<ushort, Vector128<ushort>>(ref Unsafe.Add(ref x9, 16)) = a;
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PartialBitBoard384"/> with the bottom 24 rows of <see cref="FullBitBoard"/>.
        /// </summary>
        /// <param name="board">The source <see cref="FullBitBoard"/>.</param>
        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public PartialBitBoard384(FullBitBoard board)
        {
            Unsafe.AsRef(in this) = Unsafe.As<FullBitBoard, PartialBitBoard384>(ref board);
        }

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public PartialBitBoard384(Vector256<ushort> lower, Vector128<ushort> upper)
        {
            StoreVectorBoard256(lower, upper, out Unsafe.AsRef(this));
        }

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public PartialBitBoard384(Vector128<ushort> lower, Vector128<ushort> middle, Vector128<ushort> upper)
        {
            StoreVectorBoard128(lower, middle, upper, out Unsafe.AsRef(this));
        }
        #endregion

        /// <summary>
        /// Gets the "raw" data of row at y = <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The y coordinate.</param>
        /// <returns>The raw data of row at y = <paramref name="index"/>.</returns>
        public ushort this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get
            {
                nint x9 = index;
                var bb = index >> 31;
                ushort value = 0xE007;
                index |= bb;
                value = (ushort)(value | bb);
                if ((uint)index < Height)   //Also jumps if index is less than 0
                {
                    value = Unsafe.Add(ref Unsafe.As<PartialBitBoard384, ushort>(ref Unsafe.AsRef(in this)), x9);
                }
                return value;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if (index is >= 0 and < Height) Unsafe.Add(ref Unsafe.As<PartialBitBoard384, ushort>(ref Unsafe.AsRef(in this)), index) = value;
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

        #region Load

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void LoadVectorBoard256(ref PartialBitBoard384 board, out Vector256<ushort> lower, out Vector128<ushort> upper)
        {
            lower = Unsafe.As<PartialBitBoard384, Vector256<ushort>>(ref board);
            upper = Unsafe.As<PartialBitBoard384, Vector128<ushort>>(ref Unsafe.AddByteOffset(ref board, Vector256<byte>.Count));
        }

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void LoadVectorBoard128(ref PartialBitBoard384 board, out Vector128<ushort> lower, out Vector128<ushort> middle, out Vector128<ushort> upper)
        {
            lower = Unsafe.As<PartialBitBoard384, Vector128<ushort>>(ref board);
            middle = Unsafe.As<PartialBitBoard384, Vector128<ushort>>(ref Unsafe.AddByteOffset(ref board, Vector128<byte>.Count));
            upper = Unsafe.As<PartialBitBoard384, Vector128<ushort>>(ref Unsafe.AddByteOffset(ref board, Vector256<byte>.Count));
        }

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void Deconstruct(in PartialBitBoard384 board, out Vector256<ushort> lower, out Vector128<ushort> upper)
            => LoadVectorBoard256(ref Unsafe.AsRef(board), out lower, out upper);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void Deconstruct(in PartialBitBoard384 board, out Vector128<ushort> lower, out Vector128<ushort> middle, out Vector128<ushort> upper)
            => LoadVectorBoard128(ref Unsafe.AsRef(board), out lower, out middle, out upper);
        #endregion

        #region Store
        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void StoreVectorBoard256(Vector256<ushort> lower, Vector128<ushort> upper, out PartialBitBoard384 board)
        {
            Unsafe.As<PartialBitBoard384, Vector256<ushort>>(ref board) = lower;
            Unsafe.As<PartialBitBoard384, Vector128<ushort>>(ref Unsafe.AddByteOffset(ref board, Vector256<byte>.Count)) = upper;
        }

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void StoreVectorBoard128(Vector128<ushort> lower, Vector128<ushort> middle, Vector128<ushort> upper, out PartialBitBoard384 board)
        {
            Unsafe.As<PartialBitBoard384, Vector128<ushort>>(ref board) = lower;
            Unsafe.As<PartialBitBoard384, Vector128<ushort>>(ref Unsafe.AddByteOffset(ref board, Vector128<byte>.Count)) = middle;
            Unsafe.As<PartialBitBoard384, Vector128<ushort>>(ref Unsafe.AddByteOffset(ref board, Vector256<byte>.Count)) = upper;
        }
        #endregion

    }
}
