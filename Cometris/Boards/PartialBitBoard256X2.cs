using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;

using Shamisen;

namespace Cometris.Boards
{
    /// <summary>
    /// Bit board that records only the bottom 32 out of 40 lines.<br/>
    /// This structure is for mainstream hardware-accelerated board operations, mainly for target with AVX2 available.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = sizeof(ushort) * Height)]
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public readonly struct PartialBitBoard256X2 : IMaskableBitBoard<PartialBitBoard256X2, ushort, PartialBitBoard256X2, uint>

    {
        /// <summary>
        /// 32 = 2 * <see cref="Vector256{T}.Count"/> for <see cref="ushort"/>.
        /// </summary>
        public const int Height = 32;

        public const int EffectiveWidth = 10;
        [FieldOffset(0)]
        private readonly Vector256<ushort> lower;
        [FieldOffset(32)]
        private readonly Vector256<ushort> upper;
        public Vector256<ushort> Lower => lower;
        public Vector256<ushort> Upper => upper;

        #region Useful Part Values
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector256<ushort> GetLowerIndexVector256() => Vector256.Create(ushort.MinValue, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector256<ushort> GetUpperIndexVector256() => Vector256.Create(16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, (ushort)31);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector256<ushort> GetPositiveOffsetLowerIndexVector256() => Vector256.Create(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, (ushort)16);
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector256<ushort> GetPositiveOffsetUpperIndexVector256() => Vector256.Create(17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, (ushort)32);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector256<ushort> GetNegativeOffsetLowerIndexVector256() => Vector256.Create(ushort.MaxValue, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14);
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector256<ushort> GetNegativeOffsetUpperIndexVector256() => Vector256.Create(15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, (ushort)30);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector256<ushort> GetDoubledLowerIndexVector256() => Vector256.Create(ushort.MinValue, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30);
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector256<ushort> GetDoubledUpperIndexVector256() => Vector256.Create(32, 34, 36, 38, 40, 42, 44, 46, 48, 50, 52, 54, 56, 58, 60, (ushort)62);
        #endregion

        public static bool IsBitwiseOperationHardwareAccelerated => Vector256.IsHardwareAccelerated;

        static int IBitBoard<PartialBitBoard256X2, ushort>.EffectiveWidth => EffectiveWidth;
        public static int StorableWidth => sizeof(ushort) * 8;
        static int IBitBoard<PartialBitBoard256X2, ushort>.Height => Height;

        static ushort IBitBoard<PartialBitBoard256X2, ushort>.EmptyLine => FullBitBoard.EmptyRow;
        static int IBitBoard<PartialBitBoard256X2, ushort>.BitPositionXLeftmost => (16 - EffectiveWidth) / 2 + EffectiveWidth;

        public static bool IsHorizontalConstantShiftHardwareAccelerated => Vector256.IsHardwareAccelerated;
        public static bool IsVerticalShiftSupported => Avx2.IsSupported;
        public static PartialBitBoard256X2 Zero => new();
        public static PartialBitBoard256X2 Empty => new(FullBitBoard.EmptyRow);
        public static bool IsSupported => Vector256.IsHardwareAccelerated;
        public static bool IsHorizontalVariableShiftSupported => false;

        public static int RightmostPaddingWidth => 3;

        public static PartialBitBoard256X2 ZeroMask => new();

        public static PartialBitBoard256X2 AllBitsSetMask => new(ushort.MaxValue);

#pragma warning disable S3358 // Ternary operators should not be nested
        public static int MaxEnregisteredLocals
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get => Avx512BW.VL.IsSupported ? 16 : (Avx2.IsSupported ? 8 : 0);
        }
#pragma warning restore S3358 // Ternary operators should not be nested

        #region Constructors
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public PartialBitBoard256X2(Vector256<ushort> lower, Vector256<ushort> upper)
        {
            this.lower = lower;
            this.upper = upper;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public PartialBitBoard256X2((Vector256<ushort> lower, Vector256<ushort> upper) value)
        {
            lower = value.lower;
            upper = value.upper;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public PartialBitBoard256X2(ReadOnlySpan<ushort> board, ushort fill = FullBitBoard.EmptyRow)
        {
            if (board.IsEmpty)
            {
                this = new(fill);
            }
            this = board.Length >= Height ? LoadUnsafe(ref MemoryMarshal.GetReference(board)) : CreateFromIncompleteBoard(board, fill);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public PartialBitBoard256X2(ushort fill = FullBitBoard.EmptyRow)
        {
            var value = Vector256.Create(fill);
            this = new(value, value);
        }

        [SkipLocalsInit]
        private static PartialBitBoard256X2 CreateFromIncompleteBoard(ReadOnlySpan<ushort> board, ushort fill)
        {
            var ymm0 = Vector256.Create(fill);
            var fHeight = new FixedArray32<ushort>();
            Span<ushort> buffer = fHeight;
            ref var head = ref MemoryMarshal.GetReference(buffer);
            ymm0.StoreUnsafe(ref head);
            ymm0.StoreUnsafe(ref head, 16);
            board.CopyTo(buffer);
            return LoadUnsafe(ref head);
        }

        #region Create
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 CreateFilled(ushort fill) => new(fill);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 CreateSingleBlock(int x, int y)
        {
            var v = 0x8000u >> x;
            var l = GetLowerIndexVector256();
            var u = GetUpperIndexVector256();
            var pos = Vector256.Create((ushort)y);
            var value = Vector256.Create((ushort)v);
            l = Vector256.Equals(l, pos);
            u = Vector256.Equals(u, pos);
            l &= value;
            u &= value;
            return new(l, u);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 CreateSingleLine(ushort line, int y)
        {
            var pos = Vector256.Create((ushort)y);
            var value = Vector256.Create(line);
            var l = GetLowerIndexVector256();
            var u = GetUpperIndexVector256();
            l = Vector256.Equals(l, pos);
            u = Vector256.Equals(u, pos);
            l &= value;
            u &= value;
            return new(l, u);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 CreateTwoLines(int y0, int y1, ushort line0, ushort line1)
        {
            var pos0 = Vector256.Create((ushort)y0);
            var value0 = Vector256.Create(line0);
            var pos1 = Vector256.Create((ushort)y1);
            var value1 = Vector256.Create(line1);
            var l1 = GetLowerIndexVector256();
            var u1 = GetUpperIndexVector256();
            var l0 = Vector256.Equals(l1, pos0);
            var u0 = Vector256.Equals(u1, pos0);
            l0 &= value0;
            l1 = Vector256.Equals(l1, pos1);
            u0 &= value0;
            u1 = Vector256.Equals(u1, pos1);
            l1 &= value1;
            u1 &= value1;
            l0 |= l1;
            u0 |= u1;
            return new(l0, u0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 CreateTwoAdjacentLinesUp(int y, ushort lineMiddle, ushort lineUpper)
        {
            var pos = Vector256.Create((ushort)y);
            var offset = Vector256<ushort>.AllBitsSet;
            var v0 = Vector256.Create(lineMiddle);
            var v1 = Vector256.Create(lineUpper);
            var l = GetLowerIndexVector256();
            var u = GetUpperIndexVector256();
            var posu = pos - offset;
            var l0 = Vector256.Equals(l, pos);
            var u0 = Vector256.Equals(u, pos);
            l0 &= v0;
            var l1 = Vector256.Equals(l, posu);
            u0 &= v0;
            var u1 = Vector256.Equals(u, posu);
            l1 &= v1;
            u1 &= v1;
            l0 |= l1;
            u0 |= u1;
            return new(l0, u0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 CreateTwoAdjacentLinesDown(int y, ushort lineLower, ushort lineMiddle)
        {
            var pos = Vector256.Create((ushort)y);
            var offset = Vector256<ushort>.AllBitsSet;
            var v0 = Vector256.Create(lineMiddle);
            var l = GetLowerIndexVector256();
            var u = GetUpperIndexVector256();
            var v2 = Vector256.Create(lineLower);
            offset += pos;
            var l0 = Vector256.Equals(l, pos);
            var u0 = Vector256.Equals(u, pos);
            l0 &= v0;
            u0 &= v0;
            l = Vector256.Equals(l, offset);
            u = Vector256.Equals(u, offset);
            l &= v2;
            u &= v2;
            l0 |= l;
            u0 |= u;
            return new(l0, u0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 CreateThreeAdjacentLines(int y, ushort lineLower, ushort lineMiddle, ushort lineUpper)
        {
            var pos = Vector256.Create((ushort)y);
            var offset = Vector256<ushort>.AllBitsSet;
            var v0 = Vector256.Create(lineMiddle);
            var v1 = Vector256.Create(lineUpper);
            var l = GetLowerIndexVector256();
            var u = GetUpperIndexVector256();
            var v2 = Vector256.Create(lineLower);
            var posu = pos - offset;
            offset += pos;
            var l0 = Vector256.Equals(l, pos);
            var u0 = Vector256.Equals(u, pos);
            l0 &= v0;
            var l1 = Vector256.Equals(l, posu);
            u0 &= v0;
            var u1 = Vector256.Equals(u, posu);
            l1 &= v1;
            l = Vector256.Equals(l, offset);
            u1 &= v1;
            u = Vector256.Equals(u, offset);
            l0 |= l1;
            l &= v2;
            u0 |= u1;
            u &= v2;
            l0 |= l;
            u0 |= u;
            return new(l0, u0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 CreateVerticalI4Piece(int x, int y)
        {
            y = y + y - 1;
            ushort line = 0x8000;
            line >>= x;
            var pos = Vector256.Create((ushort)y);
            var l = GetDoubledLowerIndexVector256();
            var u = GetDoubledUpperIndexVector256();
            l -= pos;
            u -= pos;
            var lines = Vector256.Create(line);
            pos = Vector256.Create((ushort)4);
            l = Vector256.Abs(l.AsInt16()).AsUInt16();
            u = Vector256.Abs(u.AsInt16()).AsUInt16();
            l = Vector256.GreaterThan(pos.AsInt16(), l.AsInt16()).AsUInt16();
            u = Vector256.GreaterThan(pos.AsInt16(), u.AsInt16()).AsUInt16();
            l &= lines;
            u &= lines;
            return new(l, u);
        }
        #endregion
        #endregion

        #region Line Construction
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static ushort CreateSingleBlockLine(int x) => (ushort)(0x8000u >> x);
        #endregion

        #region Mask Construction
        public static PartialBitBoard256X2 CreateMaskFromBoard(PartialBitBoard256X2 board) => board;
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
                var value = FullBitBoard.EmptyRow;
                index |= bb;
                value = (ushort)(value | bb);
                if ((uint)index < Height)   //Also jumps if index is less than 0
                {
                    value = Unsafe.Add(ref Unsafe.As<PartialBitBoard256X2, ushort>(ref Unsafe.AsRef(in this)), x9);
                }
                return value;
            }
        }

        #region Load/Store
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 LoadUnsafe(ref ushort source, nint elementOffset)
            => new(Vector256.LoadUnsafe(ref source, (nuint)elementOffset), Vector256.LoadUnsafe(ref source, (nuint)elementOffset + (nuint)Vector256<ushort>.Count));

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 LoadUnsafe(ref ushort source, nuint elementOffset = 0)
            => new(Vector256.LoadUnsafe(ref source, elementOffset), Vector256.LoadUnsafe(ref source, elementOffset + (nuint)Vector256<ushort>.Count));

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void StoreUnsafe(PartialBitBoard256X2 board, ref ushort destination, nint elementOffset)
        {
            board.Lower.StoreUnsafe(ref destination, (nuint)elementOffset);
            board.Upper.StoreUnsafe(ref destination, (nuint)elementOffset + (nuint)Vector256<ushort>.Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void StoreUnsafe(PartialBitBoard256X2 board, ref ushort destination, nuint elementOffset = 0)
        {
            board.Lower.StoreUnsafe(ref destination, elementOffset);
            board.Upper.StoreUnsafe(ref destination, elementOffset + (nuint)Vector256<ushort>.Count);
        }
        #endregion

        #region Board Operations
        #region Vertical Shift
        #region ShiftDown

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 ShiftDownOneLine(PartialBitBoard256X2 board, [ConstantExpected] ushort upperFeedValue)
        {
            if (Avx2.IsSupported)
            {
                var ymm2 = Vector128.Create(upperFeedValue).AsUInt16().ToVector256Unsafe();
                var ymm0 = board.Lower;
                var ymm1 = board.Upper;
                var ymm3 = Avx2.Permute2x128(ymm0, ymm1, 0x21);
                ymm2 = Avx2.Permute2x128(ymm1, ymm2, 0x21);
                ymm0 = Avx2.AlignRight(ymm3, ymm0, 2);
                ymm1 = Avx2.AlignRight(ymm2, ymm1, 2);
                return new(ymm0, ymm1);
            }
            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 ShiftDownOneLine(PartialBitBoard256X2 board, PartialBitBoard256X2 upperFeedValue)
        {
            if (Avx2.IsSupported)
            {
                var ymm2 = upperFeedValue.Lower;
                var ymm0 = board.Lower;
                var ymm1 = board.Upper;
                var ymm3 = Avx2.Permute2x128(ymm0, ymm1, 0x21);
                ymm2 = Avx2.Permute2x128(ymm1, ymm2, 0x21);
                ymm0 = Avx2.AlignRight(ymm3, ymm0, 2);
                ymm1 = Avx2.AlignRight(ymm2, ymm1, 2);
                return new(ymm0, ymm1);
            }
            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 ShiftDownTwoLines(PartialBitBoard256X2 board, [ConstantExpected] ushort upperFeedValue)
        {
            if (Avx2.IsSupported)
            {
                var ymm2 = Vector128.Create(upperFeedValue).AsUInt16().ToVector256Unsafe();
                var ymm0 = board.Lower;
                var ymm1 = board.Upper;
                var ymm3 = Avx2.Permute2x128(ymm0, ymm1, 0x21);
                ymm2 = Avx2.Permute2x128(ymm1, ymm2, 0x21);
                ymm0 = Avx2.AlignRight(ymm3, ymm0, 4);
                ymm1 = Avx2.AlignRight(ymm2, ymm1, 4);
                return new(ymm0, ymm1);
            }
            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 ShiftDownTwoLines(PartialBitBoard256X2 board, PartialBitBoard256X2 upperFeedBoard)
        {
            if (Avx2.IsSupported)
            {
                var ymm2 = upperFeedBoard.lower;
                var ymm0 = board.Lower;
                var ymm1 = board.Upper;
                var ymm3 = Avx2.Permute2x128(ymm0, ymm1, 0x21);
                ymm2 = Avx2.Permute2x128(ymm1, ymm2, 0x21);
                ymm0 = Avx2.AlignRight(ymm3, ymm0, 4);
                ymm1 = Avx2.AlignRight(ymm2, ymm1, 4);
                return new(ymm0, ymm1);
            }
            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 ShiftDownFourLines(PartialBitBoard256X2 board, [ConstantExpected] ushort upperFeedValue)
        {
            if (Avx2.IsSupported)
            {
                var ymm2 = Vector128.Create(upperFeedValue).AsUInt16().ToVector256Unsafe();
                var ymm0 = board.Lower;
                var ymm1 = board.Upper;
                var ymm3 = Avx2.Permute2x128(ymm0, ymm1, 0x21);
                ymm2 = Avx2.Permute2x128(ymm1, ymm2, 0x21);
                ymm0 = Avx2.AlignRight(ymm3, ymm0, 8);
                ymm1 = Avx2.AlignRight(ymm2, ymm1, 8);
                return new(ymm0, ymm1);
            }
            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 ShiftDownEightLines(PartialBitBoard256X2 board, [ConstantExpected] ushort upperFeedValue)
        {
            if (Avx2.IsSupported)
            {
                var ymm2 = Vector128.Create(upperFeedValue).AsUInt16().ToVector256Unsafe();
                var ymm0 = board.Lower;
                var ymm1 = board.Upper;
                var ymm3 = Avx2.Permute2x128(ymm0, ymm1, 0x21);
                ymm2 = Avx2.Permute2x128(ymm1, ymm2, 0x21);
                return new(ymm3, ymm2);
            }
            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 ShiftDownSixteenLines(PartialBitBoard256X2 board, [ConstantExpected] ushort upperFeedValue)
        {
            var ymm2 = Vector256.Create(upperFeedValue).AsUInt16();
            var ymm1 = board.Upper;
            return new(ymm1, ymm2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 ShiftDownVariableLines(PartialBitBoard256X2 board, int count, ushort upperFeedValue)
        {
            var cnt = count << -5;
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            var cflag = cnt >> 31;  // Check the 5th bit of count
            cnt <<= 1;
            if (count >= Height)
            {
                return new(upperFeedValue);
            }
            var ymm0 = board.Lower;
            var ymm1 = board.Upper;
            var ymm7 = Vector256.Create(cflag).AsSingle(); // 5th bit of count
            cflag = cnt >> 31;  // Check the 4th bit of count
            cnt <<= 1;
            var ymm4 = Vector256.Create(upperFeedValue).AsUInt16();
            var ymm8 = Vector256.Create(cflag).AsSingle(); // 4th bit of count
            cnt >>>= -3;
            // Shift 16 lines down if cnt >= 16
            ymm0 = Avx.BlendVariable(ymm0.AsSingle(), ymm1.AsSingle(), ymm7).AsUInt16();
            ymm1 = Avx.BlendVariable(ymm1.AsSingle(), ymm4.AsSingle(), ymm7).AsUInt16();
            // Shift 8 lines down if cnt >= 8
            var ymm2 = Avx2.Permute2x128(ymm0, ymm1, 0x21);
            var ymm3 = Avx2.Permute2x128(ymm1, ymm4, 0x21);
            ymm0 = Avx.BlendVariable(ymm0.AsSingle(), ymm2.AsSingle(), ymm8).AsUInt16();
            ymm1 = Avx.BlendVariable(ymm1.AsSingle(), ymm3.AsSingle(), ymm8).AsUInt16();
            // Prepare for the further shift
            ymm3 = Avx2.Permute2x128(ymm0, ymm1, 0x21);
            ymm2 = Avx2.Permute2x128(ymm1, ymm4, 0x21);
            switch (cnt)
            {
                case 7:
                    ymm0 = Avx2.AlignRight(ymm3, ymm0, 2 * 7);
                    ymm1 = Avx2.AlignRight(ymm2, ymm1, 2 * 7);
                    break;
                case 6:
                    ymm0 = Avx2.AlignRight(ymm3, ymm0, 2 * 6);
                    ymm1 = Avx2.AlignRight(ymm2, ymm1, 2 * 6);
                    break;
                case 5:
                    ymm0 = Avx2.AlignRight(ymm3, ymm0, 2 * 5);
                    ymm1 = Avx2.AlignRight(ymm2, ymm1, 2 * 5);
                    break;
                case 4:
                    ymm0 = Avx2.AlignRight(ymm3, ymm0, 2 * 4);
                    ymm1 = Avx2.AlignRight(ymm2, ymm1, 2 * 4);
                    break;
                case 3:
                    ymm0 = Avx2.AlignRight(ymm3, ymm0, 2 * 3);
                    ymm1 = Avx2.AlignRight(ymm2, ymm1, 2 * 3);
                    break;
                case 2:
                    ymm0 = Avx2.AlignRight(ymm3, ymm0, 2 * 2);
                    ymm1 = Avx2.AlignRight(ymm2, ymm1, 2 * 2);
                    break;
                case 1:
                    ymm0 = Avx2.AlignRight(ymm3, ymm0, 2 * 1);
                    ymm1 = Avx2.AlignRight(ymm2, ymm1, 2 * 1);
                    break;
            }
            return new(ymm0, ymm1);
        }
        #endregion

        #region ShiftUp
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 ShiftUpOneLine(PartialBitBoard256X2 board, [ConstantExpected] ushort lowerFeedValue)
        {
            if (Avx2.IsSupported)
            {
                var ymm2 = Vector128.Create(lowerFeedValue).AsUInt16().ToVector256Unsafe();
                var ymm0 = board.Lower;
                var ymm1 = board.Upper;
                var ymm3 = Avx2.Permute2x128(ymm0, ymm2, 0x02);
                ymm2 = Avx2.Permute2x128(ymm1, ymm0, 0x03);
                ymm0 = Avx2.AlignRight(ymm0, ymm3, 14);
                ymm1 = Avx2.AlignRight(ymm1, ymm2, 14).AsUInt16();
                return new(ymm0, ymm1);
            }
            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 ShiftUpOneLine(PartialBitBoard256X2 board, PartialBitBoard256X2 lowerFeedBoard)
        {
            if (Avx2.IsSupported)
            {
                var ymm2 = lowerFeedBoard.upper;
                var ymm0 = board.Lower;
                var ymm1 = board.Upper;
                var ymm3 = Avx2.Permute2x128(ymm0, ymm2, 0x02);
                ymm2 = Avx2.Permute2x128(ymm1, ymm0, 0x03);
                ymm0 = Avx2.AlignRight(ymm0, ymm3, 14);
                ymm1 = Avx2.AlignRight(ymm1, ymm2, 14).AsUInt16();
                return new(ymm0, ymm1);
            }
            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 ShiftUpTwoLines(PartialBitBoard256X2 board, [ConstantExpected] ushort lowerFeedValue)
        {
            if (Avx2.IsSupported)
            {
                var ymm2 = Vector128.Create(lowerFeedValue).AsUInt16().ToVector256Unsafe();
                var ymm0 = board.Lower;
                var ymm1 = board.Upper;
                var ymm3 = Avx2.Permute2x128(ymm0, ymm2, 0x02);
                ymm2 = Avx2.Permute2x128(ymm1, ymm0, 0x03);
                ymm0 = Avx2.AlignRight(ymm0, ymm3, 12);
                ymm1 = Avx2.AlignRight(ymm1, ymm2, 12).AsUInt16();
                return new(ymm0, ymm1);
            }
            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 ShiftUpTwoLines(PartialBitBoard256X2 board, PartialBitBoard256X2 lowerFeedBoard)
        {
            if (Avx2.IsSupported)
            {
                var ymm2 = lowerFeedBoard.upper;
                var ymm0 = board.Lower;
                var ymm1 = board.Upper;
                var ymm3 = Avx2.Permute2x128(ymm0, ymm2, 0x02);
                ymm2 = Avx2.Permute2x128(ymm1, ymm0, 0x03);
                ymm0 = Avx2.AlignRight(ymm0, ymm3, 12);
                ymm1 = Avx2.AlignRight(ymm1, ymm2, 12).AsUInt16();
                return new(ymm0, ymm1);
            }
            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 ShiftUpFourLines(PartialBitBoard256X2 board, [ConstantExpected] ushort lowerFeedValue)
        {
            if (Avx2.IsSupported)
            {
                var ymm2 = Vector128.Create(lowerFeedValue).AsUInt16().ToVector256Unsafe();
                var ymm0 = board.Lower;
                var ymm1 = board.Upper;
                var ymm3 = Avx2.Permute2x128(ymm0, ymm2, 0x02);
                ymm2 = Avx2.Permute2x128(ymm1, ymm0, 0x03);
                ymm0 = Avx2.AlignRight(ymm0, ymm3, 8);
                ymm1 = Avx2.AlignRight(ymm1, ymm2, 8).AsUInt16();
                return new(ymm0, ymm1);
            }
            return default;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 ShiftUpEightLines(PartialBitBoard256X2 board, [ConstantExpected] ushort lowerFeedValue)
        {
            if (Avx2.IsSupported)
            {
                var ymm2 = Vector128.Create(lowerFeedValue).AsUInt16().ToVector256Unsafe();
                var ymm0 = board.Lower;
                var ymm1 = board.Upper;
                var ymm3 = Avx2.Permute2x128(ymm0, ymm2, 0x02);
                ymm2 = Avx2.Permute2x128(ymm1, ymm0, 0x03);
                return new(ymm3, ymm2);
            }
            return default;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 ShiftUpSixteenLines(PartialBitBoard256X2 board, [ConstantExpected] ushort lowerFeedValue)
        {
            var ymm0 = Vector256.Create(lowerFeedValue).AsUInt16();
            var ymm1 = board.lower;
            return new(ymm0, ymm1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 ShiftUpVariableLines(PartialBitBoard256X2 board, int count, ushort lowerFeedValue)
        {
            var cnt = count << -5;
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            var cflag = cnt >> 31;  // Check the 5th bit of count
            cnt <<= 1;
            if (count >= Height)
            {
                return new(lowerFeedValue);
            }
            var ymm0 = board.Lower;
            var ymm1 = board.Upper;
            var ymm6 = Vector256.Create(cflag).AsSingle(); // 5th bit of count
            cflag = cnt >> 31;  // Check the 4th bit of count
            cnt <<= 1;
            var ymm4 = Vector256.Create(lowerFeedValue).AsUInt16();
            var ymm7 = Vector256.Create(cflag).AsSingle(); // 4th bit of count
            cnt >>>= -3;
            // Shift 16 lines down if cnt >= 16
            ymm1 = Avx.BlendVariable(ymm1.AsSingle(), ymm0.AsSingle(), ymm6).AsUInt16();
            ymm0 = Avx.BlendVariable(ymm0.AsSingle(), ymm4.AsSingle(), ymm6).AsUInt16();
            // Shift 8 lines down if cnt >= 8
            var ymm2 = Avx2.Permute2x128(ymm0, ymm4, 0x02);
            var ymm3 = Avx2.Permute2x128(ymm1, ymm0, 0x03);
            ymm0 = Avx.BlendVariable(ymm0.AsSingle(), ymm2.AsSingle(), ymm7).AsUInt16();
            ymm1 = Avx.BlendVariable(ymm1.AsSingle(), ymm3.AsSingle(), ymm7).AsUInt16();
            // Prepare for the further shift
            ymm3 = Avx2.Permute2x128(ymm0, ymm4, 0x02);
            ymm2 = Avx2.Permute2x128(ymm1, ymm0, 0x03);
            switch (cnt)
            {
                case 7:
                    ymm0 = Avx2.AlignRight(ymm0, ymm3, 2 * (8 - 7));
                    ymm1 = Avx2.AlignRight(ymm1, ymm2, 2 * (8 - 7));
                    break;
                case 6:
                    ymm0 = Avx2.AlignRight(ymm0, ymm3, 2 * (8 - 6));
                    ymm1 = Avx2.AlignRight(ymm1, ymm2, 2 * (8 - 6));
                    break;
                case 5:
                    ymm0 = Avx2.AlignRight(ymm0, ymm3, 2 * (8 - 5));
                    ymm1 = Avx2.AlignRight(ymm1, ymm2, 2 * (8 - 5));
                    break;
                case 4:
                    ymm0 = Avx2.AlignRight(ymm0, ymm3, 2 * (8 - 4));
                    ymm1 = Avx2.AlignRight(ymm1, ymm2, 2 * (8 - 4));
                    break;
                case 3:
                    ymm0 = Avx2.AlignRight(ymm0, ymm3, 2 * (8 - 3));
                    ymm1 = Avx2.AlignRight(ymm1, ymm2, 2 * (8 - 3));
                    break;
                case 2:
                    ymm0 = Avx2.AlignRight(ymm0, ymm3, 2 * (8 - 2));
                    ymm1 = Avx2.AlignRight(ymm1, ymm2, 2 * (8 - 2));
                    break;
                case 1:
                    ymm0 = Avx2.AlignRight(ymm0, ymm3, 2 * (8 - 1));
                    ymm1 = Avx2.AlignRight(ymm1, ymm2, 2 * (8 - 1));
                    break;
            }
            return new(ymm0, ymm1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 ShiftUpVariableLines(PartialBitBoard256X2 board, int count, PartialBitBoard256X2 lowerFeedBoard)
        {
            var cnt = count << -5;
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            var cflag = cnt >> 31;  // Check the 5th bit of count
            cnt <<= 1;
            if (count >= Height)
            {
                return lowerFeedBoard;
            }
            var ymm0 = board.Lower;
            var ymm1 = board.Upper;
            var ymm6 = Vector256.Create(cflag).AsSingle(); // 5th bit of count
            cflag = cnt >> 31;  // Check the 4th bit of count
            cnt <<= 1;
            var ymm4 = lowerFeedBoard.lower.AsUInt16();
            var ymm7 = Vector256.Create(cflag).AsSingle(); // 4th bit of count
            cnt >>>= -3;
            // Shift 16 lines down if cnt >= 16
            ymm1 = Avx.BlendVariable(ymm1.AsSingle(), ymm0.AsSingle(), ymm6).AsUInt16();
            ymm0 = Avx.BlendVariable(ymm0.AsSingle(), ymm4.AsSingle(), ymm6).AsUInt16();
            // Shift 8 lines down if cnt >= 8
            var ymm2 = Avx2.Permute2x128(ymm0, ymm4, 0x02);
            var ymm3 = Avx2.Permute2x128(ymm1, ymm0, 0x03);
            ymm0 = Avx.BlendVariable(ymm0.AsSingle(), ymm2.AsSingle(), ymm7).AsUInt16();
            ymm1 = Avx.BlendVariable(ymm1.AsSingle(), ymm3.AsSingle(), ymm7).AsUInt16();
            // Prepare for the further shift
            ymm3 = Avx2.Permute2x128(ymm0, ymm4, 0x02);
            ymm2 = Avx2.Permute2x128(ymm1, ymm0, 0x03);
            switch (cnt)
            {
                case 7:
                    ymm0 = Avx2.AlignRight(ymm0, ymm3, 2 * (8 - 7));
                    ymm1 = Avx2.AlignRight(ymm1, ymm2, 2 * (8 - 7));
                    break;
                case 6:
                    ymm0 = Avx2.AlignRight(ymm0, ymm3, 2 * (8 - 6));
                    ymm1 = Avx2.AlignRight(ymm1, ymm2, 2 * (8 - 6));
                    break;
                case 5:
                    ymm0 = Avx2.AlignRight(ymm0, ymm3, 2 * (8 - 5));
                    ymm1 = Avx2.AlignRight(ymm1, ymm2, 2 * (8 - 5));
                    break;
                case 4:
                    ymm0 = Avx2.AlignRight(ymm0, ymm3, 2 * (8 - 4));
                    ymm1 = Avx2.AlignRight(ymm1, ymm2, 2 * (8 - 4));
                    break;
                case 3:
                    ymm0 = Avx2.AlignRight(ymm0, ymm3, 2 * (8 - 3));
                    ymm1 = Avx2.AlignRight(ymm1, ymm2, 2 * (8 - 3));
                    break;
                case 2:
                    ymm0 = Avx2.AlignRight(ymm0, ymm3, 2 * (8 - 2));
                    ymm1 = Avx2.AlignRight(ymm1, ymm2, 2 * (8 - 2));
                    break;
                case 1:
                    ymm0 = Avx2.AlignRight(ymm0, ymm3, 2 * (8 - 1));
                    ymm1 = Avx2.AlignRight(ymm1, ymm2, 2 * (8 - 1));
                    break;
            }
            return new(ymm0, ymm1);
        }
        #endregion

        #endregion

        #region Horizontal Shift
        #region Shift Right
        #region ShiftRightOneColumn
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 ShiftRightOneColumn(PartialBitBoard256X2 board)
            => new(Vector256.ShiftRightArithmetic(board.Lower.AsInt16(), 1).AsUInt16(), Vector256.ShiftRightArithmetic(board.Upper.AsInt16(), 1).AsUInt16());
        #endregion

        #region ShiftRightTwoColumn
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 ShiftRightTwoColumn(PartialBitBoard256X2 board)
            => new(Vector256.ShiftRightArithmetic(board.Lower.AsInt16(), 2).AsUInt16(), Vector256.ShiftRightArithmetic(board.Upper.AsInt16(), 2).AsUInt16());
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 operator >>(PartialBitBoard256X2 board, [ConstantExpected] int shift)
            => new(Vector256.ShiftRightLogical(board.Lower, shift), Vector256.ShiftRightLogical(board.Upper, shift));
        #endregion

        #region Shift Left
        #region ShiftLeftOneColumn
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 ShiftLeftOneColumn(PartialBitBoard256X2 board)
            => new(Vector256.ShiftLeft(board.Lower, 1), Vector256.ShiftLeft(board.Upper, 1));
        #endregion

        #region ShiftLeftTwoColumn
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 ShiftLeftTwoColumn(PartialBitBoard256X2 board)
            => new(Vector256.ShiftLeft(board.Lower, 2), Vector256.ShiftLeft(board.Upper, 2));
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 operator <<(PartialBitBoard256X2 board, [ConstantExpected] int shift)
            => new(Vector256.ShiftLeft(board.Lower, shift), Vector256.ShiftLeft(board.Upper, shift));
        #endregion
        #endregion

        #region Reachability
        #region FillDropReachable
        public static PartialBitBoard256X2 FillDropReachable(PartialBitBoard256X2 board, PartialBitBoard256X2 reached)
        {
            var b0 = board;
            var r0 = reached;
            var r1 = ShiftDownOneLine(r0, 0);
            var b1 = ShiftDownOneLine(b0, 0);
            r1 = Or1And02(r1, r0, b0);
            b1 &= b0;
            r0 = ShiftDownTwoLines(r1, 0);
            b0 = ShiftDownTwoLines(b1, 0);
            r0 = Or1And02(r0, r1, b1);
            b0 &= b1;
            r1 = ShiftDownFourLines(r0, 0);
            b1 = ShiftDownFourLines(b0, 0);
            r1 = Or1And02(r1, r0, b0);
            b1 &= b0;
            r0 = ShiftDownEightLines(r1, 0);
            b0 = ShiftDownEightLines(b1, 0);
            r0 = Or1And02(r0, r1, b1);
            b0 &= b1;
            r1 = ShiftDownSixteenLines(r0, 0);
            r1 = Or1And02(r1, r0, b0);
            return r1;
        }

        #endregion

        #region FillHorizontalReachable
        public static PartialBitBoard256X2 FillHorizontalReachable(PartialBitBoard256X2 board, PartialBitBoard256X2 reached)
        {
            var b0 = board;
            var r0 = reached;
            var r2 = r0 >> 1;
            var r3 = r0 << 1;
            var b2 = b0 >> 1;
            var b3 = b0 << 1;
            r2 = (r2 & b0) | r0;
            r3 = (r3 & b0) | r0;
            r2 |= r3;
            b2 &= b0;
            b3 &= b0;
            r0 = r2 >> 2;
            var r1 = r2 << 2;
            b0 = b2 >> 2;
            var b1 = b3 << 2;
            r0 = (r0 & b2) | r2;
            r1 = (r1 & b3) | r2;
            r0 |= r1;
            b0 &= b2;
            b1 &= b3;
            r2 = r0 >> 4;
            r3 = r0 << 4;
            b2 = b0 >> 4;
            b3 = b1 << 4;
            r2 = (r2 & b0) | r0;
            r3 = (r3 & b1) | r0;
            r2 |= r3;
            b2 &= b0;
            b3 &= b1;
            r0 = r2 >> 8;
            r1 = r2 << 8;
            r0 = (r0 & b2) | r2;
            r1 = (r1 & b3) | r2;
            r0 |= r1;
            return r0;
        }

        #endregion
        #endregion

        #region OnesComplement
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 operator ~(PartialBitBoard256X2 board)
            => board ^ Vector256<ushort>.AllBitsSet;
        #endregion

        #region ExclusiveOr
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 operator ^(PartialBitBoard256X2 left, PartialBitBoard256X2 right)
            => new(left.Lower ^ right.Lower, left.Upper ^ right.Upper);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 operator ^(PartialBitBoard256X2 left, Vector256<ushort> right)
            => new(left.Lower ^ right, left.Upper ^ right);
        #endregion

        #region BitwiseOr
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 operator |(PartialBitBoard256X2 left, PartialBitBoard256X2 right)
            => new(left.Lower | right.Lower, left.Upper | right.Upper);
        #endregion

        #region BitwiseAnd
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 operator &(PartialBitBoard256X2 left, PartialBitBoard256X2 right)
            => new(left.Lower & right.Lower, left.Upper & right.Upper);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 AndNot(PartialBitBoard256X2 left, PartialBitBoard256X2 right)
            => new(~left.Lower & right.Lower, ~left.Upper & right.Upper);
        #endregion

        #region Operator Supplement
        public static PartialBitBoard256X2 LineSelect(PartialBitBoard256X2 mask, PartialBitBoard256X2 left, PartialBitBoard256X2 right)
        {
            if (Avx2.IsSupported)
            {
                var ymm0 = mask.lower;
                var ymm1 = mask.upper;
                var ymm2 = left.lower;
                var ymm3 = left.upper;
                return new(Avx2.BlendVariable(ymm2, right.lower, ymm0), Avx2.BlendVariable(ymm3, right.upper, ymm1));
            }
            return left ^ ((left ^ right) & mask);
        }

        public static PartialBitBoard256X2 Or1And02(PartialBitBoard256X2 b0, PartialBitBoard256X2 b1, PartialBitBoard256X2 b2) => (b0 & b2) | b1;

        public static uint CompressMask(PartialBitBoard256X2 mask)
        {
            var ymm0 = mask.lower;
            var ymm1 = mask.upper;
            return (ymm1.ExtractMostSignificantBits() << 16) | ((ushort)ymm0.ExtractMostSignificantBits());
        }
        public static PartialBitBoard256X2 ExpandMask(uint compactLineMask)
        {
            var ymm0 = Vector256.Create((short)compactLineMask);
            var ymm1 = Vector256.Create((short)(compactLineMask >> 16));
            var m = Vector256.Create(0x1000_2000_4000_8000, 0x0100_0200_0400_0800, 0x0010_0020_0040_0080, 0x0001_0002_0004_0008).AsInt16();
            ymm0 *= m;
            ymm1 *= m;
            ymm0 >>= 15;
            ymm1 >>= 15;
            return new(ymm0.AsUInt16(), ymm1.AsUInt16());
        }

        #endregion

        #region Per-Line Operations

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 CompareEqualPerLineVector(PartialBitBoard256X2 left, PartialBitBoard256X2 right)
            => new(Vector256.Equals(left.lower, right.lower), Vector256.Equals(left.upper, right.upper));
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 CompareNotEqualPerLineVector(PartialBitBoard256X2 left, PartialBitBoard256X2 right)
                    => new(~Vector256.Equals(left.lower, right.lower), ~Vector256.Equals(left.upper, right.upper));

        #endregion
        #endregion

        #region Mask Operations
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 MaskUnaryNegation(PartialBitBoard256X2 mask) => ~mask;
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 MaskAnd(PartialBitBoard256X2 left, PartialBitBoard256X2 right) => left & right;
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 MaskOr(PartialBitBoard256X2 left, PartialBitBoard256X2 right) => left | right;
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 MaskXor(PartialBitBoard256X2 left, PartialBitBoard256X2 right) => left ^ right;
        #endregion

        #region Mask Utilities
        public static int TrailingZeroCount(uint mask) => BitOperations.TrailingZeroCount(mask);
        public static int PopCount(uint mask) => BitOperations.PopCount(mask);
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static explicit operator (Vector256<ushort> lower, Vector256<ushort> upper)(PartialBitBoard256X2 value) => (value.Lower, value.Upper);
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static explicit operator PartialBitBoard256X2((Vector256<ushort> lower, Vector256<ushort> upper) value) => new(value.lower, value.upper);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool operator ==(PartialBitBoard256X2 left, PartialBitBoard256X2 right)
        {
            var t = Vector256.Xor(left.lower, right.lower);
            t |= Vector256.Xor(left.upper, right.upper);
            return Vector256.EqualsAll(t, Vector256<ushort>.Zero);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool operator !=(PartialBitBoard256X2 left, PartialBitBoard256X2 right) => !(left == right);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        [SkipLocalsInit]
        internal string GetDebuggerDisplay() => GetSquareDisplay();

        private static (ulong s0, ulong s1) GetBraillePattern4Rows(ulong rows)
        {
            ulong res0 = 0, res1 = 0;
            rows = MathI.ReverseBitOrder(rows);
            ulong r3 = (ushort)rows;
            ulong r2 = (ushort)(rows >> 16);
            ulong r1 = (ushort)(rows >> 32);
            ulong r0 = (ushort)(rows >> 48);
            res0 |= Bmi2.X64.ParallelBitDeposit(r0, 0x00C0_00C0_00C0_00C0ul);
            res0 |= Bmi2.X64.ParallelBitDeposit(r1, 0x0024_0024_0024_0024ul);
            res0 |= Bmi2.X64.ParallelBitDeposit(r2, 0x0012_0012_0012_0012ul);
            res0 |= Bmi2.X64.ParallelBitDeposit(r3, 0x0009_0009_0009_0009ul);
            res1 |= Bmi2.X64.ParallelBitDeposit(r0 >> 8, 0x00C0_00C0_00C0_00C0ul);
            res1 |= Bmi2.X64.ParallelBitDeposit(r1 >> 8, 0x0024_0024_0024_0024ul);
            res1 |= Bmi2.X64.ParallelBitDeposit(r2 >> 8, 0x0012_0012_0012_0012ul);
            res1 |= Bmi2.X64.ParallelBitDeposit(r3 >> 8, 0x0009_0009_0009_0009ul);
            return (res0 + 0x2800_2800_2800_2800ul, res1 + 0x2800_2800_2800_2800ul);
        }

        public string GetBrailleDisplay()
        {
            var f8ul = new FixedArray8<ulong>();
            var sb = new StringBuilder();
            Span<ulong> a = f8ul;
            Unsafe.WriteUnaligned(ref Unsafe.As<ulong, byte>(ref MemoryMarshal.GetReference(a)), this);
            var f8 = new FixedArray8<char>();
            Span<char> b = f8;
            var bytes = MemoryMarshal.AsBytes(b);
            for (var i = a.Length - 1; i >= 0; i--)
            {
                var rows = a[i];
                var s = GetBraillePattern4Rows(rows);
                BinaryPrimitives.WriteUInt64LittleEndian(bytes, s.s0);
                BinaryPrimitives.WriteUInt64LittleEndian(bytes.Slice(8), s.s1);
                _ = sb.AppendLine();
                _ = sb.Append(b);
            }
            return sb.ToString();
        }

        public string GetSquareDisplay()
        {
            var sb = new StringBuilder("\n");
            var fHeight = new FixedArray32<ushort>();
            Span<ushort> a = fHeight;
            Unsafe.WriteUnaligned(ref Unsafe.As<ushort, byte>(ref MemoryMarshal.GetReference(a)), this);
            var upperStreak = true;
            var upperStreakCount = 0;
            var previousItem = a[^1];
            for (var i = a.Length - 1; i >= 0; i--)
            {
                var item = a[i];
                if (!upperStreak | item != previousItem)
                {
                    _ = sb.Append($"{i + 1,2}: ");
                    _ = sb.Append(VisualizeLine(previousItem));
                    _ = sb.AppendLine(!upperStreak | (upperStreakCount < 2) ? "" : $" x{upperStreakCount}");
                    upperStreakCount = 1;
                }
                else
                {
                    upperStreakCount++;
                }
                previousItem = item;
            }
            _ = sb.Append($"{0,2}: ");
            _ = sb.Append(VisualizeLine(previousItem));
            _ = sb.Append(!upperStreak | (upperStreakCount < 2) ? "" : $" x{upperStreakCount}");
            return sb.ToString();
        }

        private static string VisualizeLine(ushort line) => Convert.ToString(line, 2).PadLeft(16, '0').Replace('0', '□').Replace('1', '■');
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public override string? ToString() => GetDebuggerDisplay();
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public override bool Equals(object? obj) => obj is PartialBitBoard256X2 x && Equals(x);
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public bool Equals(PartialBitBoard256X2 other) => this == other;

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public override int GetHashCode() => HashCode.Combine(lower, upper);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool GetBlockAt(ushort line, int x) => (((uint)line << x) & 0b0001_0000_0000_0000) > 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool GetBlockAtFullRange(ushort line, int x) => (0x8000u & ((nuint)line << x)) > 0;
        public static (PartialBitBoard256X2 upper, PartialBitBoard256X2 right, PartialBitBoard256X2 lower, PartialBitBoard256X2 left) ConvertVerticalSymmetricToAsymmetricMobility((PartialBitBoard256X2 upper, PartialBitBoard256X2 right) boards)
            => (boards.upper, boards.right, ShiftUpOneLine(boards.upper, 0), boards.right >> 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static (PartialBitBoard256X2 upper, PartialBitBoard256X2 right, PartialBitBoard256X2 lower, PartialBitBoard256X2 left) ConvertHorizontalSymmetricToAsymmetricMobility((PartialBitBoard256X2 upper, PartialBitBoard256X2 right) boards)
            => (boards.upper, boards.right, boards.upper >> 1, ShiftDownOneLine(boards.right, 0));

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public PartialBitBoard256X2 WithLine(ushort line, int y)
        {
            var l = GetLowerIndexVector256();
            var u = GetUpperIndexVector256();
            var lv = lower;
            var uv = upper;
            var pos = Vector256.Create((ushort)y);
            var value = Vector256.Create(line);
            l = Vector256.Equals(l, pos);
            u = Vector256.Equals(u, pos);
            if (Avx2.IsSupported)
            {
                l = Avx2.BlendVariable(lv, value, l);
                u = Avx2.BlendVariable(uv, value, u);
            }
            else
            {
                l = Vector256.ConditionalSelect(l, value, lv);
                u = Vector256.ConditionalSelect(u, value, uv);
            }
            return new(l, u);
        }

        public void Deconstruct(out Vector256<ushort> lower, out Vector256<ushort> upper)
        {
            lower = this.lower;
            upper = this.upper;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 FromBoard(ReadOnlySpan<ushort> board, ushort fill) => new(board, fill);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 ClearClearableLines(PartialBitBoard256X2 board, ushort fill, out PartialBitBoard256X2 clearedLines)
        {
            var ymm2 = Vector256<ushort>.AllBitsSet;
            var ymm0 = Vector256.Equals(board.lower, ymm2);
            var ymm1 = Vector256.Equals(board.upper, ymm2);
            clearedLines = new(ymm0, ymm1);
            ymm2 = ymm0 | ymm1;
            return Vector256.EqualsAll(ymm2, Vector256<ushort>.Zero) ? board : ClearLines(board, fill, ymm0, ymm1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 ClearClearableLines(PartialBitBoard256X2 board, ushort fill) => ClearClearableLines(board, fill, out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 GetClearableLinesVector(PartialBitBoard256X2 board)
        {
            var ymm2 = Vector256<ushort>.AllBitsSet;
            var ymm0 = Vector256.Equals(board.lower, ymm2);
            var ymm1 = Vector256.Equals(board.upper, ymm2);
            return new(ymm0, ymm1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 ClearLines(PartialBitBoard256X2 board, ushort fill, PartialBitBoard256X2 lines) => ClearLines(board, fill, lines.lower, lines.upper);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        [SkipLocalsInit]
        internal static PartialBitBoard256X2 ClearLinesFallback(PartialBitBoard256X2 board, ushort fill, Vector256<ushort> lower, Vector256<ushort> upper)
        {
            var ymm0 = lower;
            var ymm1 = upper;
            // Using PackSignedSaturate because all values from either ymm0 or ymm1 are all one of either 0 or -1.
            ymm0 = Avx2.PackSignedSaturate(ymm0.AsInt16(), ymm1.AsInt16()).AsUInt16();
            ymm0 = Avx2.Permute4x64(ymm0.AsUInt64(), 0b11_01_10_00).AsUInt16();
            var mask = (nint)~ymm0.AsByte().ExtractMostSignificantBits();
            if (mask != unchecked((nint)~0u))
            {
                var fHeight = new FixedArray32<ushort>();
                Span<ushort> res = fHeight;
                ref var dst = ref MemoryMarshal.GetReference(res);
                StoreUnsafe(board, ref dst);
                nint j = 0, k;
                for (nint i = 0; i < Height; i++)
                {
                    var v = Unsafe.Add(ref dst, i);
                    k = j;
                    var f = mask & 1;
                    j += f;
                    mask >>>= 1;
                    Unsafe.Add(ref dst, k) = v;
                }
                res[(int)j..].Fill(fill);
                board = LoadUnsafe(ref dst);
            }
            return board;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        [SkipLocalsInit]
        internal static PartialBitBoard256X2 ClearLines(PartialBitBoard256X2 board, ushort fill, Vector256<ushort> lower, Vector256<ushort> upper)
        {
            if (Bmi2.X64.IsSupported)
            {
                var ymm0 = board.lower.AsUInt64();
                var ymm1 = board.upper.AsUInt64();
                var ymm2 = lower.AsUInt64();
                var ymm3 = upper.AsUInt64();
                var ymm4 = Vector256.Create(fill);
                if (Vector256.EqualsAll(ymm2 | ymm3, default))
                {
                    return new(ymm0.AsUInt16(), ymm1.AsUInt16());
                }
                ymm2 = ~ymm2;
                ymm3 = ~ymm3;
                var fHeight = new FixedArray32<ushort>();
                Span<ushort> res = fHeight;
                ref var head = ref MemoryMarshal.GetReference(res);
                ymm4.StoreUnsafe(ref head);
                ymm4.StoreUnsafe(ref head, (nuint)Vector256<ushort>.Count);
                nint j = 0, k;
                var maskPart = ymm2.GetElement(0);
                var boardPart = ymm0.GetElement(0);
                boardPart = Bmi2.X64.ParallelBitExtract(boardPart, maskPart);
                k = (nint)Popcnt.X64.PopCount(maskPart) >> 4;
                Unsafe.As<ushort, ulong>(ref Unsafe.Add(ref head, j)) = boardPart;
                j += k;
                maskPart = ymm2.GetElement(1);
                var xmm2 = ymm2.GetUpper();
                boardPart = ymm0.GetElement(1);
                var xmm0 = ymm0.GetUpper();
                boardPart = Bmi2.X64.ParallelBitExtract(boardPart, maskPart);
                k = (nint)Popcnt.X64.PopCount(maskPart) >> 4;
                Unsafe.As<ushort, ulong>(ref Unsafe.Add(ref head, j)) = boardPart;
                j += k;
                maskPart = xmm2.GetElement(0);
                boardPart = xmm0.GetElement(0);
                boardPart = Bmi2.X64.ParallelBitExtract(boardPart, maskPart);
                k = (nint)Popcnt.X64.PopCount(maskPart) >> 4;
                Unsafe.As<ushort, ulong>(ref Unsafe.Add(ref head, j)) = boardPart;
                j += k;
                maskPart = xmm2.GetElement(1);
                boardPart = xmm0.GetElement(1);
                boardPart = Bmi2.X64.ParallelBitExtract(boardPart, maskPart);
                k = (nint)Popcnt.X64.PopCount(maskPart) >> 4;
                Unsafe.As<ushort, ulong>(ref Unsafe.Add(ref head, j)) = boardPart;
                j += k;
                maskPart = ymm3.GetElement(0);
                boardPart = ymm1.GetElement(0);
                boardPart = Bmi2.X64.ParallelBitExtract(boardPart, maskPart);
                k = (nint)Popcnt.X64.PopCount(maskPart) >> 4;
                Unsafe.As<ushort, ulong>(ref Unsafe.Add(ref head, j)) = boardPart;
                j += k;
                maskPart = ymm3.GetElement(1);
                xmm2 = ymm3.GetUpper();
                boardPart = ymm1.GetElement(1);
                xmm0 = ymm1.GetUpper();
                boardPart = Bmi2.X64.ParallelBitExtract(boardPart, maskPart);
                k = (nint)Popcnt.X64.PopCount(maskPart) >> 4;
                Unsafe.As<ushort, ulong>(ref Unsafe.Add(ref head, j)) = boardPart;
                j += k;
                maskPart = xmm2.GetElement(0);
                boardPart = xmm0.GetElement(0);
                boardPart = Bmi2.X64.ParallelBitExtract(boardPart, maskPart);
                k = (nint)Popcnt.X64.PopCount(maskPart) >> 4;
                Unsafe.As<ushort, ulong>(ref Unsafe.Add(ref head, j)) = boardPart;
                j += k;
                maskPart = xmm2.GetElement(1);
                boardPart = xmm0.GetElement(1);
                boardPart = Bmi2.X64.ParallelBitExtract(boardPart, maskPart);
                k = (nint)Popcnt.X64.PopCount(maskPart) >> 4;
                Unsafe.As<ushort, ulong>(ref Unsafe.Add(ref head, j)) = boardPart;
                j += k;
                switch (res.Length - j)
                {
                    case >= 4:
                        Unsafe.As<ushort, double>(ref Unsafe.Add(ref head, j)) = ymm4.AsDouble().GetElement(0);
                        break;
                    case 3:
                        Unsafe.Add(ref head, j + 2) = fill;
                        goto case 2;
                    case 2:
                        Unsafe.As<ushort, float>(ref Unsafe.Add(ref head, j)) = ymm4.AsSingle().GetElement(0);
                        break;
                    case 1:
                        Unsafe.Add(ref head, j) = fill;
                        break;
                    default:
                        break;
                }
                ymm0 = Vector256.LoadUnsafe(ref head).AsUInt64();
                ymm1 = Vector256.LoadUnsafe(ref head, (nuint)Vector256<ushort>.Count).AsUInt64();
                return new(ymm0.AsUInt16(), ymm1.AsUInt16());
            }
            return ClearLinesFallback(board, fill, lower, upper);
        }

        #region Board Classification
        public static bool IsBoardZero(PartialBitBoard256X2 board)
            => Vector256.EqualsAll(board.lower | board.upper, Vector256<ushort>.Zero);
        #endregion

        #region Board Statistics
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int TotalBlocks(PartialBitBoard256X2 board)
        {
            var ymm0 = board.lower.AsByte();
            var ymm1 = board.upper.AsByte();
            var ymm2 = Vector256.Create(0x0f0f0f0fu).AsByte();
            var ymm3 = ymm1 & ymm2;
            var ymm5 = ymm0 & ymm2;
            ymm1 &= ~ymm2;
            ymm0 &= ~ymm2;
            // PopCount Lookup table for all 4-bit integer
            var ymm4 = Vector256.Create(byte.MinValue, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4, 0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4);
            ymm1 = Avx2.ShiftRightLogical(ymm1.AsUInt16(), 4).AsByte();
            ymm0 = Avx2.ShiftRightLogical(ymm0.AsUInt16(), 4).AsByte();
            ymm3 = Avx2.Shuffle(ymm4, ymm3);
            ymm5 = Avx2.Shuffle(ymm4, ymm5);
            ymm1 = Avx2.Shuffle(ymm4, ymm1);
            ymm0 = Avx2.Shuffle(ymm4, ymm0);
            ymm1 += ymm3;
            ymm3 = Vector256<byte>.Zero;
            ymm0 += ymm5;
            ymm0 += ymm1;
            ymm0 = Avx2.SumAbsoluteDifferences(ymm0, ymm3).AsByte();
            var xmm0 = Sse2.Add(ymm0.GetLower().AsInt32(), ymm0.GetUpper().AsInt32()).AsInt32();
            var xmm1 = Sse2.Shuffle(xmm0, 0b10_10_10_10);
            xmm0 += xmm1;
            return xmm0.GetElement(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard256X2 BlocksPerLine(PartialBitBoard256X2 board)
        {
            var ymm0 = board.lower.AsByte();
            var ymm1 = board.upper.AsByte();
            var ymm2 = Vector256.Create(0x0f0f0f0fu).AsByte();
            var ymm3 = ymm1 & ymm2;
            var ymm5 = ymm0 & ymm2;
            ymm1 &= ~ymm2;
            ymm0 &= ~ymm2;
            // PopCount Lookup table for all 4-bit integer
            var ymm4 = Vector256.Create(byte.MinValue, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4, 0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4);
            ymm1 = Avx2.ShiftRightLogical(ymm1.AsUInt16(), 4).AsByte();
            ymm0 = Avx2.ShiftRightLogical(ymm0.AsUInt16(), 4).AsByte();
            ymm3 = Avx2.Shuffle(ymm4, ymm3);
            ymm5 = Avx2.Shuffle(ymm4, ymm5);
            ymm1 = Avx2.Shuffle(ymm4, ymm1);
            ymm0 = Avx2.Shuffle(ymm4, ymm0);
            ymm1 += ymm3;
            ymm0 += ymm5;
            ymm3 = Vector256.ShiftLeft(ymm1.AsUInt16(), 8).AsByte();
            ymm5 = Vector256.ShiftLeft(ymm0.AsUInt16(), 8).AsByte();
            ymm1 += ymm3;
            ymm0 += ymm5;
            ymm1 = Vector256.ShiftRightLogical(ymm1.AsUInt16(), 8).AsByte();
            ymm0 = Vector256.ShiftRightLogical(ymm0.AsUInt16(), 8).AsByte();
            return new(ymm0.AsUInt16(), ymm1.AsUInt16());
        }

        public static int LocateAllBlocks(PartialBitBoard256X2 board, IBufferWriter<CompressedPositionsTuple> writer)
        {
            if (IsBoardZero(board)) return 0;
            var count = TotalBlocks(board);
            var compressedCount = (count + 2) / 3;
            var buffer = writer.GetSpan(compressedCount);
            uint compressing = 0;
            var modshift = 0;
            ref var bhead = ref Unsafe.As<PartialBitBoard256X2, ushort>(ref board);
            nint j = 0;
            var i = 0;
            uint ycoord = 0;
            for (; j < Height; j++, ycoord += 1 << 4)
            {
                var line = (uint)Unsafe.Add(ref bhead, j) << 16;
                while (true)
                {
                    var pos = (uint)BitOperations.LeadingZeroCount(line);
                    if (pos >= 16) break;
                    line = MathI.ZeroHighBitsFromHigh((int)pos + 1, line);
                    var coord = ycoord | pos;
                    compressing |= coord << modshift;
                    modshift += 10;
                    if (modshift < 30) continue;
                    modshift = 0;
                    buffer[i] = new(compressing | 0xc000_0000u);
                    compressing = 0;
                    i++;
                }
            }
            if ((uint)(modshift - 1) < 29)  // x - 1 brings 0 to uint.MaxValue
            {
                var mcount = (uint)modshift / 10u;
                buffer[i] = new(compressing | (mcount << 30));
                i++;
            }
            writer.Advance(i);
            return count;
        }

        public static bool IsSetAt(PartialBitBoard256X2 mask, byte index) => mask[index] >> 15 > 0;
        #endregion
    }
}
