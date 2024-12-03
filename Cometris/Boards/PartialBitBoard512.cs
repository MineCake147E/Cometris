using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

using Cometris.Pieces;
using Cometris.Utils;

using MikoMino.Game;

using Shamisen;

namespace Cometris.Boards
{
#if NET8_0_OR_GREATER

    /// <summary>
    /// Bit board that records only the bottom 32 out of 40 lines.<br/>
    /// This structure is for mainstream hardware-accelerated board operations, mainly for target with AVX-512 available.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = sizeof(ushort) * Height)]
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public readonly partial struct PartialBitBoard512 : ICompactMaskableBitBoard<PartialBitBoard512, ushort, Vector512<ushort>, uint>
    {
        /// <summary>
        /// 32 = <see cref="Vector512{T}.Count"/> for <see cref="ushort"/>.
        /// </summary>
        public const int Height = PartialBitBoard256X2.Height;

        public const int EffectiveWidth = PartialBitBoard256X2.EffectiveWidth;

#pragma warning disable IDE0032 // Use auto property
        private readonly Vector512<ushort> storage;
#pragma warning restore IDE0032 // Use auto property

        public Vector512<ushort> Value => storage;

        #region Useful Part Values

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector512<ushort> GetIndexVector512()
            => Vector512.Create(ushort.MinValue, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector512<ushort> GetPositiveOffsetIndexVector512()
            => Vector512.Create((ushort)1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector512<ushort> GetDoublePositiveOffsetIndexVector512()
            => Vector512.Create((ushort)2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector512<ushort> GetNegativeOffsetIndexVector512()
            => Vector512.Create(ushort.MaxValue, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector512<ushort> GetDoubleNegativeOffsetIndexVector512()
            => Vector512.Create(ushort.MaxValue - 1, ushort.MaxValue, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector512<ushort> GetDoubledIndexVector512()
            => Vector512.Create(ushort.MinValue, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36, 38, 40, 42, 44, 46, 48, 50, 52, 54, 56, 58, 60, 62);
        #endregion
        #region Hardware Capability

        public static bool IsBitwiseOperationHardwareAccelerated => Avx512BW.IsSupported;
        public static bool IsHorizontalConstantShiftHardwareAccelerated => Avx512BW.IsSupported;
        public static bool IsHorizontalVariableShiftSupported => Avx512BW.IsSupported;
        public static bool IsSupported => Avx512BW.IsSupported;
        public static bool IsVerticalShiftSupported => Avx512BW.IsSupported;
        #endregion
        #region Board Constants
        public static ushort EmptyLine => FullBitBoard.EmptyRow;
        public static ushort InvertedEmptyLine => FullBitBoard.InvertedEmptyRow;
        public static PartialBitBoard512 Empty => new(FullBitBoard.EmptyRow);

        public static PartialBitBoard512 InvertedEmpty => new(FullBitBoard.InvertedEmptyRow);

        public static PartialBitBoard512 Zero => new(Vector128<ushort>.Zero.ToVector256Unsafe().ToVector512Unsafe());

        static int IBitBoard<PartialBitBoard512, ushort>.BitPositionXLeftmost => (16 - EffectiveWidth) / 2 + EffectiveWidth;
        static int IBitBoard<PartialBitBoard512, ushort>.Height => Height;
        public static int RightmostPaddingWidth => 3;
        public static int StorableWidth => sizeof(ushort) * 8;

        public static Vector512<ushort> ZeroMask => Vector128<ushort>.Zero.ToVector256Unsafe().ToVector512Unsafe();
        public static Vector512<ushort> AllBitsSetMask => Vector512<ulong>.AllBitsSet.AsUInt16();

        public static PartialBitBoard512 AllBitsSet => new(Vector512<ulong>.AllBitsSet.AsUInt16());

        public static int MaxEnregisteredLocals => Avx512BW.IsSupported ? 32 : 0;

        static int IBitBoard<PartialBitBoard512, ushort>.EffectiveWidth => EffectiveWidth;
        #endregion

        public ushort this[int y]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get
            {
                var control = Vector512.CreateScalarUnsafe((ushort)y);
                var v = Avx512BW.PermuteVar32x16(storage, control).AsUInt32().GetElement(0);
                var bb = (uint)(y >> 31);
                var value = FullBitBoard.EmptyRow | bb;
                if ((uint)y < (uint)Vector512<ushort>.Count) value = v;   //Also jumps if index is less than 0
                return (ushort)value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static explicit operator PartialBitBoard256X2(PartialBitBoard512 board) => Unsafe.BitCast<PartialBitBoard512, PartialBitBoard256X2>(board);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static explicit operator PartialBitBoard512(PartialBitBoard256X2 board) => Unsafe.BitCast<PartialBitBoard256X2, PartialBitBoard512>(board);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static implicit operator PartialBitBoard512(Vector512<ushort> board) => Unsafe.BitCast<Vector512<ushort>, PartialBitBoard512>(board);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static implicit operator Vector512<ushort>(PartialBitBoard512 board) => Unsafe.BitCast<PartialBitBoard512, Vector512<ushort>>(board);

        #region Board Construction

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public PartialBitBoard512(Vector512<ushort> value)
        {
            storage = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public PartialBitBoard512(ReadOnlySpan<ushort> board, ushort fill = FullBitBoard.EmptyRow)
        {
            this = FromBoard(board, fill);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public PartialBitBoard512(ushort fill = FullBitBoard.EmptyRow)
        {
            var value = Vector512.Create(fill);
            this = new(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 FromBoard(ReadOnlySpan<ushort> board, ushort fill)
        {
            var b = board;
            if ((uint)b.Length >= Height)
            {
                return LoadUnsafe(ref MemoryMarshal.GetReference(b));
            }
            var filled = Vector512.Create(fill);
            return b.IsEmpty ? (PartialBitBoard512)filled : CreateFromIncompleteBoard(b, filled);
        }

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static PartialBitBoard512 CreateFromIncompleteBoard(ReadOnlySpan<ushort> board, Vector512<ushort> filled)
        {
            var b = board;
            var zmm0 = filled;
            var len = (nuint)b.Length;
            var len2 = len & (nuint.MaxValue >> 1 >> BitOperations.LeadingZeroCount(len));
            var pos = GetIndexVector512() - Vector512.Create((ushort)len2);
            ref var rsi = ref MemoryMarshal.GetReference(b);
            ref var r9 = ref Unsafe.As<ushort, byte>(ref Unsafe.Add(ref rsi, len2));
            ref var r10 = ref Unsafe.As<ushort, byte>(ref rsi);
            if (len2 > 0)
            {
                zmm0 = len switch
                {
                    >= 16 => zmm0.WithLower(Vector256.LoadUnsafe(ref rsi, len2)),
                    >= 8 => Avx512F.InsertVector128(zmm0, Vector128.LoadUnsafe(ref rsi, len2), 0),
                    >= 4 => zmm0.AsDouble().WithElement(0, Unsafe.ReadUnaligned<double>(ref r9)).AsUInt16(),
                    _ => zmm0.AsSingle().WithElement(0, Unsafe.ReadUnaligned<float>(ref r9)).AsUInt16(),
                };
                zmm0 = Avx512BW.PermuteVar32x16(zmm0, pos);
            }
            zmm0 = len switch
            {
                >= 16 => zmm0.WithLower(Vector256.LoadUnsafe(ref rsi)),
                >= 8 => Avx512F.InsertVector128(zmm0, Vector128.LoadUnsafe(ref rsi), 0),
                >= 4 => zmm0.AsDouble().WithElement(0, Unsafe.ReadUnaligned<double>(ref r10)).AsUInt16(),
                >= 2 => zmm0.AsSingle().WithElement(0, Unsafe.ReadUnaligned<float>(ref r10)).AsUInt16(),
                _ => zmm0.WithElement(0, rsi),
            };
            return zmm0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public PartialBitBoard512 WithLine(ushort line, int y)
        {
            var zmm2 = GetIndexVector512();
            var zmm0 = Vector512.Create((ushort)y);
            var zmm3 = storage;
            var xmm1 = Vector128.CreateScalarUnsafe(line);
            zmm0 = Vector512.Equals(zmm0, zmm2);
            return new(Avx512BW.BlendVariable(zmm3, Avx512BW.BroadcastScalarToVector512(xmm1), zmm0));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 CreateSingleBlock(int x, int y) => CreateSingleLine((ushort)(0x8000u >> x), y);
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 CreateSingleLine(ushort line, int y)
        {
            var zmm0 = Vector512.Create((ushort)y);
            var xmm1 = Vector128.CreateScalarUnsafe(line);
            var zmm2 = GetIndexVector512();
            zmm0 = Vector512.Equals(zmm0, zmm2);
            return new(Avx512BW.BlendVariable(Vector512<ushort>.Zero, Avx512BW.BroadcastScalarToVector512(xmm1), zmm0));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 CreateTwoLines(int y0, int y1, ushort line0, ushort line1)
        {
            var zmm0 = Vector512.Create((ushort)y0);
            var zmm1 = Vector512.Create(line0);
            var zmm2 = Vector512.Create((ushort)y1);
            var zmm3 = Vector512.Create(line1);
            var zmm4 = GetIndexVector512();
            zmm0 = Vector512.Equals(zmm0, zmm4);
            zmm2 = Vector512.Equals(zmm2, zmm4);
            zmm0 = Avx512BW.BlendVariable(Vector512<ushort>.Zero, zmm1, zmm0);
            zmm2 = Avx512BW.BlendVariable(Vector512<ushort>.Zero, zmm3, zmm2);
            return new(zmm0 | zmm2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 CreateThreeAdjacentLines(int y, ushort lineLower, ushort lineMiddle, ushort lineUpper)
        {
            var r9 = (ulong)lineUpper << 32;
            var zmm0 = Vector512.Create((ushort)y);
            var r10 = lineLower | ((ulong)lineMiddle << 16);
            r10 |= r9;
            zmm0 = GetPositiveOffsetIndexVector512() - zmm0;
            var zmm1 = Vector128.CreateScalar(r10).ToVector256Unsafe().ToVector512Unsafe().AsUInt16();
            zmm0 = Avx512BW.PermuteVar32x16(zmm1, zmm0);
            return new(zmm0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 CreateTwoAdjacentLinesUp(int y, ushort lineMiddle, ushort lineUpper)
        {
            var zmm0 = Vector512.Create((ushort)y);
            var r9 = lineMiddle | ((uint)lineUpper << 16);
            zmm0 = GetIndexVector512() - zmm0;
            var zmm1 = Vector128.CreateScalar(r9).ToVector256Unsafe().ToVector512Unsafe().AsUInt16();
            zmm1 = Avx512BW.PermuteVar32x16(zmm1, zmm0);
            return new(zmm1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 CreateTwoAdjacentLinesDown(int y, ushort lineLower, ushort lineMiddle)
        {
            var zmm0 = Vector512.Create((ushort)y);
            var r9 = lineLower | ((uint)lineMiddle << 16);
            zmm0 = GetPositiveOffsetIndexVector512() - zmm0;
            var zmm1 = Vector128.CreateScalar(r9).ToVector256Unsafe().ToVector512Unsafe().AsUInt16();
            zmm1 = Avx512BW.PermuteVar32x16(zmm1, zmm0);
            return new(zmm1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 CreateVerticalI4Piece(int x, int y)
        {
            var zmm0 = Vector512.Create((ushort)y);
            var line = 0x8000_8000_8000_8000ul;
            line >>= x;
            var zmm1 = GetDoublePositiveOffsetIndexVector512() - zmm0;
            var zmm2 = Vector128.CreateScalar(line).ToVector256Unsafe().ToVector512Unsafe().AsUInt16();
            zmm1 = Avx512BW.PermuteVar32x16(zmm2, zmm1);
            return new(zmm1);
        }
        #endregion

        #region Line Construction
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static ushort CreateSingleBlockLine(int x) => (ushort)(0x8000u >> x);
        #endregion

        #region Mask Construction
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector512<ushort> CreateMaskFromBoard(PartialBitBoard512 board) => board.storage;
        #endregion

        #region Board Load/Store
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 LoadUnsafe(ref ushort source, nint elementOffset)
    => new(Vector512.LoadUnsafe(ref source, (nuint)elementOffset));

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 LoadUnsafe(ref ushort source, nuint elementOffset = 0)
            => new(Vector512.LoadUnsafe(ref source, elementOffset));

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void StoreUnsafe(PartialBitBoard512 board, ref ushort destination, nint elementOffset)
            => board.storage.StoreUnsafe(ref destination, (nuint)elementOffset);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void StoreUnsafe(PartialBitBoard512 board, ref ushort destination, nuint elementOffset = 0)
            => board.storage.StoreUnsafe(ref destination, elementOffset);

        #endregion

        #region Board Operations

        #region Operator Overloads
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 operator &(PartialBitBoard512 left, PartialBitBoard512 right) => new(left.storage & right.storage);
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 operator |(PartialBitBoard512 left, PartialBitBoard512 right) => new(left.storage | right.storage);
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 operator ^(PartialBitBoard512 left, PartialBitBoard512 right) => new(left.storage ^ right.storage);
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 operator ~(PartialBitBoard512 value) => new((~value.storage.AsUInt32()).AsUInt16());
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 operator <<(PartialBitBoard512 left, [ConstantExpected] int right) => new(Vector512.ShiftLeft(left.storage, right));
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 operator >>(PartialBitBoard512 left, [ConstantExpected] int right) => new(Vector512.ShiftRightLogical(left.storage, right));
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool operator ==(PartialBitBoard512 left, PartialBitBoard512 right) => left.storage == right.storage;
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool operator !=(PartialBitBoard512 left, PartialBitBoard512 right) => left.storage != right.storage;
        #endregion

        #region Operator Supplement
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 AndNot(PartialBitBoard512 left, PartialBitBoard512 right) => new(Avx512F.AndNot(left.storage, right.storage));

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector512<ushort> AndNot(Vector512<ushort> left, Vector512<ushort> right) => Avx512F.AndNot(left, right);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 Or1And02(PartialBitBoard512 b0, PartialBitBoard512 b1, PartialBitBoard512 b2)
            => new(Avx512F.TernaryLogic(b0.storage, b1.storage, b2.storage, 0xEC));

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 BitwiseSelect(PartialBitBoard512 mask, PartialBitBoard512 left, PartialBitBoard512 right)
            => new(Vector512.ConditionalSelect(mask.storage, left.storage, right.storage));

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 OrAll(PartialBitBoard512 b0, PartialBitBoard512 b1, PartialBitBoard512 b2)
            => new(Avx512F.TernaryLogic(b0.storage, b1.storage, b2.storage, 0xFE));

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector512<ushort> OrAll(Vector512<ushort> b0, Vector512<ushort> b1, Vector512<ushort> b2)
            => Avx512F.TernaryLogic(b0, b1, b2, 0xFE);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 AndAll(PartialBitBoard512 b0, PartialBitBoard512 b1, PartialBitBoard512 b2)
            => new(Avx512F.TernaryLogic(b0.storage, b1.storage, b2.storage, 0x80));

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 And0Or12(PartialBitBoard512 b0, PartialBitBoard512 b1, PartialBitBoard512 b2)
            => new(Avx512F.TernaryLogic(b0.storage, b1.storage, b2.storage, (byte)(TernaryOperations.A & (TernaryOperations.OrBC))));
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 AndNot0Or12(PartialBitBoard512 b0, PartialBitBoard512 b1, PartialBitBoard512 b2)
                    => new(Avx512F.TernaryLogic(b0.storage, b1.storage, b2.storage, (byte)(TernaryOperations.NotA & (TernaryOperations.OrBC))));
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 AndNot0And12(PartialBitBoard512 b0, PartialBitBoard512 b1, PartialBitBoard512 b2)
                    => new(Avx512F.TernaryLogic(b0.storage, b1.storage, b2.storage, (byte)(TernaryOperations.NotA & (TernaryOperations.AndBC))));

        public static uint CompressMask(Vector512<ushort> mask) => (uint)mask.ExtractMostSignificantBits();

        public static Vector512<ushort> ExpandMask(uint compactLineMask)
        {
            var zmm0 = Vector512.Create(compactLineMask).AsUInt16();
            zmm0 = Avx512BW.Shuffle(zmm0.AsByte(), Vector512.Create(0, 0, 0x0101_0101_0101_0101ul, 0x0101_0101_0101_0101ul, 0x0202_0202_0202_0202ul, 0x0202_0202_0202_0202ul, 0x0303_0303_0303_0303ul, 0x303_0303_0303_0303ul).AsByte()).AsUInt16();
            zmm0 = Avx512BW.ShiftLeftLogicalVariable(zmm0, Vector512.Create(0x0004_0005_0006_0007ul, 0x0000_0001_0002_0003ul, 0x0004_0005_0006_0007ul, 0x0000_0001_0002_0003ul, 0x0004_0005_0006_0007ul, 0x0000_0001_0002_0003ul, 0x0004_0005_0006_0007ul, 0x0000_0001_0002_0003ul).AsUInt16());
            return Avx512BW.ShiftRightArithmetic(zmm0.AsInt16(), 15).AsUInt16();
        }
        #endregion

        #region Tuple Operations
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static (PartialBitBoard512 upper, PartialBitBoard512 right, PartialBitBoard512 lower, PartialBitBoard512 left) Or4Sets((PartialBitBoard512 upper, PartialBitBoard512 right, PartialBitBoard512 lower, PartialBitBoard512 left) board, (PartialBitBoard512 upper, PartialBitBoard512 right, PartialBitBoard512 lower, PartialBitBoard512 left) reached)
        {
            (var upperBoard, var rightBoard, var lowerBoard, var leftBoard) = (board.upper.storage, board.right.storage, board.lower.storage, board.left.storage);
            (var upperReached, var rightReached, var lowerReached, var leftReached) = (reached.upper.storage, reached.right.storage, reached.lower.storage, reached.left.storage);
            var upper = upperBoard | upperReached;
            var right = rightBoard | rightReached;
            var lower = lowerBoard | lowerReached;
            var left = leftBoard | leftReached;
            return (upper, right, lower, left);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static (PartialBitBoard512 upper, PartialBitBoard512 right, PartialBitBoard512 lower, PartialBitBoard512 left) And4Sets((PartialBitBoard512 upper, PartialBitBoard512 right, PartialBitBoard512 lower, PartialBitBoard512 left) board, (PartialBitBoard512 upper, PartialBitBoard512 right, PartialBitBoard512 lower, PartialBitBoard512 left) reached)
        {
            (var upperBoard, var rightBoard, var lowerBoard, var leftBoard) = (board.upper.storage, board.right.storage, board.lower.storage, board.left.storage);
            (var upperReached, var rightReached, var lowerReached, var leftReached) = (reached.upper.storage, reached.right.storage, reached.lower.storage, reached.left.storage);
            var upper = upperBoard & upperReached;
            var right = rightBoard & rightReached;
            var lower = lowerBoard & lowerReached;
            var left = leftBoard & leftReached;
            return (upper, right, lower, left);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static (PartialBitBoard512 upper, PartialBitBoard512 right, PartialBitBoard512 lower, PartialBitBoard512 left) AndNot4Sets((PartialBitBoard512 upper, PartialBitBoard512 right, PartialBitBoard512 lower, PartialBitBoard512 left) left, (PartialBitBoard512 upper, PartialBitBoard512 right, PartialBitBoard512 lower, PartialBitBoard512 left) right)
        {
            (var upperNegated, var rightNegated, var lowerNegated, var leftNegated) = (left.upper.storage, left.right.storage, left.lower.storage, left.left.storage);
            (var upperOperand, var rightOperand, var lowerOperand, var leftOperand) = (right.upper.storage, right.right.storage, right.lower.storage, right.left.storage);
            var tempU = Avx512F.AndNot(upperNegated, upperOperand);
            var tempR = Avx512F.AndNot(rightNegated, rightOperand);
            var tempD = Avx512F.AndNot(lowerNegated, lowerOperand);
            var tempL = Avx512F.AndNot(leftNegated, leftOperand);
            return (tempU, tempR, tempD, tempL);
        }
        #endregion

        #region Vertical Shift
        #region Shift Down
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 ShiftDownOneLine(PartialBitBoard512 board, ushort upperFeedValue)
        {
            var zmm0 = board.storage;
            var zmm1 = Vector128.CreateScalar(upperFeedValue).ToVector256Unsafe().ToVector512Unsafe();
            var zmm2 = Avx512F.AlignRight32(zmm1.AsUInt32(), zmm0.AsUInt32(), 4).AsUInt16();
            return new(Avx512BW.AlignRight(zmm2.AsByte(), zmm0.AsByte(), 2).AsUInt16());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 ShiftDownOneLine(PartialBitBoard512 board, PartialBitBoard512 upperFeedValue)
        {
            var zmm0 = board.storage;
            var zmm1 = upperFeedValue.storage;
            var zmm2 = Avx512F.AlignRight32(zmm1.AsUInt32(), zmm0.AsUInt32(), 4).AsUInt16();
            return new(Avx512BW.AlignRight(zmm2.AsByte(), zmm0.AsByte(), 2).AsUInt16());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector512<ushort> ShiftDownOneLine(Vector512<ushort> board, ushort upperFeedValue)
        {
            var zmm0 = board;
            var zmm1 = Vector128.CreateScalar(upperFeedValue).ToVector256Unsafe().ToVector512Unsafe();
            var zmm2 = Avx512F.AlignRight32(zmm1.AsUInt32(), zmm0.AsUInt32(), 4).AsUInt16();
            return Avx512BW.AlignRight(zmm2.AsByte(), zmm0.AsByte(), 2).AsUInt16();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector512<ushort> ShiftDownOneLine(Vector512<ushort> board, Vector512<ushort> upperFeedValue)
        {
            var zmm0 = board;
            var zmm1 = upperFeedValue;
            var zmm2 = Avx512F.AlignRight32(zmm1.AsUInt32(), zmm0.AsUInt32(), 4).AsUInt16();
            return Avx512BW.AlignRight(zmm2.AsByte(), zmm0.AsByte(), 2).AsUInt16();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 ShiftDownTwoLines(PartialBitBoard512 board, ushort upperFeedValue)
        {
            var zmm1 = Vector128.Create(upperFeedValue).ToVector256Unsafe().ToVector512Unsafe().AsUInt32();
            var zmm0 = board.storage.AsUInt32();
            return new(Avx512F.AlignRight32(zmm1, zmm0, 1).AsUInt16());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 ShiftDownTwoLines(PartialBitBoard512 board, PartialBitBoard512 upperFeedBoard)
        {
            var zmm1 = upperFeedBoard.storage.AsUInt32();
            var zmm0 = board.storage.AsUInt32();
            return new(Avx512F.AlignRight32(zmm1, zmm0, 1).AsUInt16());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector512<ushort> ShiftDownTwoLines(Vector512<ushort> board, ushort upperFeedValue)
        {
            var zmm1 = Vector128.Create(upperFeedValue).ToVector256Unsafe().ToVector512Unsafe().AsUInt32();
            var zmm0 = board.AsUInt32();
            return Avx512F.AlignRight32(zmm1, zmm0, 1).AsUInt16();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector512<ushort> ShiftDownTwoLines(Vector512<ushort> board, Vector512<ushort> upperFeedBoard)
        {
            var zmm1 = upperFeedBoard.AsUInt32();
            var zmm0 = board.AsUInt32();
            return Avx512F.AlignRight32(zmm1, zmm0, 1).AsUInt16();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 ShiftDownFourLines(PartialBitBoard512 board, ushort upperFeedValue)
        {
            var zmm1 = Vector128.Create(upperFeedValue).ToVector256Unsafe().ToVector512Unsafe().AsUInt32();
            var zmm0 = board.storage.AsUInt32();
            return new(Avx512F.AlignRight32(zmm1, zmm0, 2).AsUInt16());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 ShiftDownEightLines(PartialBitBoard512 board, ushort upperFeedValue)
        {
            var zmm1 = Vector128.Create(upperFeedValue).ToVector256Unsafe().ToVector512Unsafe().AsUInt32();
            var zmm0 = board.storage.AsUInt32();
            return new(Avx512F.AlignRight32(zmm1, zmm0, 4).AsUInt16());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 ShiftDown16Lines(PartialBitBoard512 board, ushort upperFeedValue)
        {
            var ymm1 = Vector256.Create(upperFeedValue);
            var ymm0 = board.storage.GetUpper();
            return new(Vector512.Create(ymm0, ymm1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 ShiftDownVariableLines(PartialBitBoard512 board, int count, ushort upperFeedValue)
        {
            var zmm0 = board.storage;
            var zmm1 = Vector512.Create(upperFeedValue);
            if (Avx512Vbmi.IsSupported)
            {
                var zmm2 = Vector512<byte>.Indices + Vector512.Create((byte)(count * 2));
                return new(Avx512Vbmi.PermuteVar64x8x2(zmm0.AsByte(), zmm2, zmm1.AsByte()).AsUInt16());
            }
            else
            {
                var zmm2 = Vector512<ushort>.Indices + Vector512.Create((ushort)count);
                return new(Avx512BW.PermuteVar32x16x2(zmm0, zmm2, zmm1));
            }
        }
        #endregion
        #region Shift Up
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 ShiftUpOneLine(PartialBitBoard512 board, ushort lowerFeedValue)
        {
            var zmm0 = board.storage;
            var zmm1 = Vector512.Create(lowerFeedValue);
            var zmm2 = Avx512F.AlignRight32(zmm0.AsUInt32(), zmm1.AsUInt32(), 12).AsUInt16();
            return new(Avx512BW.AlignRight(zmm0.AsByte(), zmm2.AsByte(), 14).AsUInt16());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 ShiftUpOneLine(PartialBitBoard512 board, PartialBitBoard512 lowerFeedBoard)
        {
            var zmm0 = board.storage;
            var zmm1 = lowerFeedBoard.storage;
            var zmm2 = Avx512F.AlignRight32(zmm0.AsUInt32(), zmm1.AsUInt32(), 12).AsUInt16();
            return new(Avx512BW.AlignRight(zmm0.AsByte(), zmm2.AsByte(), 14).AsUInt16());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector512<ushort> ShiftUpOneLine(Vector512<ushort> board, ushort lowerFeedValue)
        {
            var zmm0 = board;
            var zmm1 = Vector512.Create(lowerFeedValue);
            var zmm2 = Avx512F.AlignRight32(zmm0.AsUInt32(), zmm1.AsUInt32(), 12).AsUInt16();
            return Avx512BW.AlignRight(zmm0.AsByte(), zmm2.AsByte(), 14).AsUInt16();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector512<ushort> ShiftUpOneLine(Vector512<ushort> board, Vector512<ushort> lowerFeedBoard)
        {
            var zmm0 = board;
            var zmm1 = lowerFeedBoard;
            var zmm2 = Avx512F.AlignRight32(zmm0.AsUInt32(), zmm1.AsUInt32(), 12).AsUInt16();
            return Avx512BW.AlignRight(zmm0.AsByte(), zmm2.AsByte(), 14).AsUInt16();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 ShiftUpTwoLines(PartialBitBoard512 board, ushort lowerFeedValue)
        {
            var zmm1 = Vector512.Create(lowerFeedValue).AsUInt32();
            var zmm0 = board.storage;
            return new(Avx512F.AlignRight32(zmm0.AsUInt32(), zmm1, 15).AsUInt16());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 ShiftUpTwoLines(PartialBitBoard512 board, PartialBitBoard512 lowerFeedBoard)
        {
            var zmm1 = lowerFeedBoard.storage;
            var zmm0 = board.storage;
            return new(Avx512F.AlignRight32(zmm0.AsUInt32(), zmm1.AsUInt32(), 15).AsUInt16());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector512<ushort> ShiftUpTwoLines(Vector512<ushort> board, ushort lowerFeedValue)
        {
            var zmm1 = Vector512.Create(lowerFeedValue).AsUInt32();
            var zmm0 = board;
            return Avx512F.AlignRight32(zmm0.AsUInt32(), zmm1, 15).AsUInt16();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector512<ushort> ShiftUpTwoLines(Vector512<ushort> board, Vector512<ushort> lowerFeedBoard)
        {
            var zmm1 = lowerFeedBoard;
            var zmm0 = board;
            return Avx512F.AlignRight32(zmm0.AsUInt32(), zmm1.AsUInt32(), 15).AsUInt16();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 ShiftUpFourLines(PartialBitBoard512 board, ushort lowerFeedValue)
        {
            var zmm1 = Vector512.Create(lowerFeedValue).AsUInt32();
            var zmm0 = board.storage;
            return new(Avx512F.AlignRight32(zmm0.AsUInt32(), zmm1, 14).AsUInt16());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 ShiftUpEightLines(PartialBitBoard512 board, ushort lowerFeedValue)
        {
            var zmm1 = Vector512.Create(lowerFeedValue).AsUInt32();
            var zmm0 = board.storage;
            return new(Avx512F.AlignRight32(zmm0.AsUInt32(), zmm1, 12).AsUInt16());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 ShiftUp16Lines(PartialBitBoard512 board, ushort lowerFeedValue)
        {
            var ymm1 = Vector256.Create(lowerFeedValue);
            var ymm0 = board.storage.GetLower();
            return new(Vector512.Create(ymm1, ymm0));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 ShiftUpVariableLines(PartialBitBoard512 board, int count, ushort lowerFeedValue)
        {
            var zmm0 = board.storage;
            var zmm1 = Vector512.Create(lowerFeedValue);
            if (Avx512Vbmi.IsSupported)
            {
                var zmm2 = Vector512<byte>.Indices - Vector512.Create((byte)(count * 2));
                return new(Avx512Vbmi.PermuteVar64x8x2(zmm0.AsByte(), zmm2, zmm1.AsByte()).AsUInt16());
            }
            else
            {
                var zmm2 = Vector512<ushort>.Indices - Vector512.Create((ushort)count);
                return new(Avx512BW.PermuteVar32x16x2(zmm0, zmm2, zmm1));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 ShiftUpVariableLines(PartialBitBoard512 board, int count, PartialBitBoard512 lowerFeedBoard)
        {
            var zmm0 = board.storage;
            var zmm1 = lowerFeedBoard.storage;
            if (Avx512Vbmi.IsSupported)
            {
                var zmm2 = Vector512<byte>.Indices - Vector512.Create((byte)(count * 2));
                return new(Avx512Vbmi.PermuteVar64x8x2(zmm0.AsByte(), zmm2, zmm1.AsByte()).AsUInt16());
            }
            else
            {
                var zmm2 = Vector512<ushort>.Indices - Vector512.Create((ushort)count);
                return new(Avx512BW.PermuteVar32x16x2(zmm0, zmm2, zmm1));
            }
        }

        #endregion
        #endregion

        #region AVX-512 Utils

        public static PartialBitBoard512 TernaryLogic(PartialBitBoard512 a, PartialBitBoard512 b, PartialBitBoard512 c, [ConstantExpected] byte control)
            => new(Avx512F.TernaryLogic(a.storage, b.storage, c.storage, control));
        #endregion

        #region Reachability
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 FillDropReachable(PartialBitBoard512 board, PartialBitBoard512 reached)
        {
            var zmm31 = Vector256<uint>.Zero.ToVector512Unsafe();
            var zmm0 = board.storage.AsUInt32();
            var zmm1 = reached.storage.AsUInt32();
            var zmm3 = Avx512F.AlignRight32(zmm31, zmm1, 4);
            var zmm2 = Avx512F.AlignRight32(zmm31, zmm0, 4);
            zmm3 = Avx512BW.AlignRight(zmm3.AsByte(), zmm1.AsByte(), 2).AsUInt32();
            zmm2 = Avx512BW.AlignRight(zmm2.AsByte(), zmm0.AsByte(), 2).AsUInt32();
            const int OrBAndAC = 0xEC;
            zmm3 = Avx512F.TernaryLogic(zmm3, zmm1, zmm0, OrBAndAC);
            zmm2 &= zmm0;
            zmm1 = Avx512F.AlignRight32(zmm31, zmm3, 1);
            zmm0 = Avx512F.AlignRight32(zmm31, zmm2, 1);
            zmm1 = Avx512F.TernaryLogic(zmm1, zmm3, zmm2, OrBAndAC);
            zmm0 &= zmm2;
            zmm3 = Avx512F.AlignRight32(zmm31, zmm1, 2);
            zmm2 = Avx512F.AlignRight32(zmm31, zmm0, 2);
            zmm3 = Avx512F.TernaryLogic(zmm3, zmm1, zmm0, OrBAndAC);
            zmm2 &= zmm0;
            zmm1 = Avx512F.AlignRight32(zmm31, zmm3, 4);
            zmm0 = Avx512F.AlignRight32(zmm31, zmm2, 4);
            zmm1 = Avx512F.TernaryLogic(zmm1, zmm3, zmm2, OrBAndAC);
            zmm0 &= zmm2;
            zmm3 = Avx512F.AlignRight32(zmm31, zmm1, 8);
            zmm3 = Avx512F.TernaryLogic(zmm3, zmm1, zmm0, OrBAndAC);
            return new(zmm3.AsUInt16());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static (PartialBitBoard512 upper, PartialBitBoard512 right, PartialBitBoard512 lower, PartialBitBoard512 left) FillDropReachable4Sets((PartialBitBoard512 upper, PartialBitBoard512 right, PartialBitBoard512 lower, PartialBitBoard512 left) board, (PartialBitBoard512 upper, PartialBitBoard512 right, PartialBitBoard512 lower, PartialBitBoard512 left) reached)
        {
            var zmm31 = Vector128<uint>.Zero.ToVector256Unsafe().ToVector512Unsafe();
            var zmm30 = Vector128.CreateScalar(ushort.MaxValue).ToVector256Unsafe().ToVector512Unsafe().AsUInt32();
            const int OrBAndAC = 0xEC;
            var zmm1 = reached.upper.storage.AsUInt32();
            var zmm5 = reached.right.storage.AsUInt32();
            var zmm9 = reached.lower.storage.AsUInt32();
            var zmm13 = reached.left.storage.AsUInt32();
            var zmm0 = board.upper.storage.AsUInt32();
            var zmm4 = board.right.storage.AsUInt32();
            var zmm8 = board.lower.storage.AsUInt32();
            var zmm12 = board.left.storage.AsUInt32();
            var zmm3 = Avx512F.AndNot(zmm30, zmm1);
            zmm3 = Avx512BW.PermuteVar32x16(zmm3.AsUInt16(), Vector512.Create(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, ushort.MinValue)).AsUInt32();
            var zmm7 = Avx512F.AndNot(zmm30, zmm5);
            zmm7 = Avx512BW.PermuteVar32x16(zmm7.AsUInt16(), Vector512.Create(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, ushort.MinValue)).AsUInt32();
            var zmm11 = Avx512F.AndNot(zmm30, zmm9);
            zmm11 = Avx512BW.PermuteVar32x16(zmm11.AsUInt16(), Vector512.Create(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, ushort.MinValue)).AsUInt32();
            var zmm15 = Avx512F.AndNot(zmm30, zmm13);
            zmm15 = Avx512BW.PermuteVar32x16(zmm15.AsUInt16(), Vector512.Create(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, ushort.MinValue)).AsUInt32();
            var zmm2 = Avx512F.AndNot(zmm30, zmm0);
            zmm2 = Avx512BW.PermuteVar32x16(zmm2.AsUInt16(), Vector512.Create(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, ushort.MinValue)).AsUInt32();
            var zmm6 = Avx512F.AndNot(zmm30, zmm4);
            zmm6 = Avx512BW.PermuteVar32x16(zmm6.AsUInt16(), Vector512.Create(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, ushort.MinValue)).AsUInt32();
            var zmm10 = Avx512F.AndNot(zmm30, zmm8);
            zmm10 = Avx512BW.PermuteVar32x16(zmm10.AsUInt16(), Vector512.Create(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, ushort.MinValue)).AsUInt32();
            var zmm14 = Avx512F.AndNot(zmm30, zmm12);
            zmm14 = Avx512BW.PermuteVar32x16(zmm14.AsUInt16(), Vector512.Create(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, ushort.MinValue)).AsUInt32();
            zmm3 = Avx512F.TernaryLogic(zmm3, zmm1, zmm0, OrBAndAC);
            zmm7 = Avx512F.TernaryLogic(zmm7, zmm5, zmm4, OrBAndAC);
            zmm11 = Avx512F.TernaryLogic(zmm11, zmm9, zmm8, OrBAndAC);
            zmm15 = Avx512F.TernaryLogic(zmm15, zmm13, zmm12, OrBAndAC);
            zmm2 &= zmm0;
            zmm6 &= zmm4;
            zmm10 &= zmm8;
            zmm14 &= zmm12;
            zmm1 = Avx512F.AlignRight32(zmm31, zmm3, 1);
            zmm5 = Avx512F.AlignRight32(zmm31, zmm7, 1);
            zmm9 = Avx512F.AlignRight32(zmm31, zmm11, 1);
            zmm13 = Avx512F.AlignRight32(zmm31, zmm15, 1);
            zmm0 = Avx512F.AlignRight32(zmm31, zmm2, 1);
            zmm1 = Avx512F.TernaryLogic(zmm1, zmm3, zmm2, OrBAndAC);
            zmm4 = Avx512F.AlignRight32(zmm31, zmm6, 1);
            zmm5 = Avx512F.TernaryLogic(zmm5, zmm7, zmm6, OrBAndAC);
            zmm8 = Avx512F.AlignRight32(zmm31, zmm10, 1);
            zmm9 = Avx512F.TernaryLogic(zmm9, zmm11, zmm10, OrBAndAC);
            zmm12 = Avx512F.AlignRight32(zmm31, zmm14, 1);
            zmm13 = Avx512F.TernaryLogic(zmm13, zmm15, zmm14, OrBAndAC);
            zmm0 &= zmm2;
            zmm3 = Avx512F.AlignRight32(zmm31, zmm1, 2);
            zmm4 &= zmm6;
            zmm7 = Avx512F.AlignRight32(zmm31, zmm5, 2);
            zmm8 &= zmm10;
            zmm11 = Avx512F.AlignRight32(zmm31, zmm9, 2);
            zmm12 &= zmm14;
            zmm15 = Avx512F.AlignRight32(zmm31, zmm13, 2);
            zmm2 = Avx512F.AlignRight32(zmm31, zmm0, 2);
            zmm3 = Avx512F.TernaryLogic(zmm3, zmm1, zmm0, OrBAndAC);
            zmm6 = Avx512F.AlignRight32(zmm31, zmm4, 2);
            zmm7 = Avx512F.TernaryLogic(zmm7, zmm5, zmm4, OrBAndAC);
            zmm10 = Avx512F.AlignRight32(zmm31, zmm8, 2);
            zmm11 = Avx512F.TernaryLogic(zmm11, zmm9, zmm8, OrBAndAC);
            zmm14 = Avx512F.AlignRight32(zmm31, zmm12, 2);
            zmm15 = Avx512F.TernaryLogic(zmm15, zmm13, zmm12, OrBAndAC);
            zmm2 &= zmm0;
            zmm1 = Avx512F.AlignRight32(zmm31, zmm3, 4);
            zmm6 &= zmm4;
            zmm5 = Avx512F.AlignRight32(zmm31, zmm7, 4);
            zmm10 &= zmm8;
            zmm9 = Avx512F.AlignRight32(zmm31, zmm11, 4);
            zmm14 &= zmm12;
            zmm13 = Avx512F.AlignRight32(zmm31, zmm15, 4);
            zmm0 = Avx512F.AlignRight32(zmm31, zmm2, 4);
            zmm1 = Avx512F.TernaryLogic(zmm1, zmm3, zmm2, OrBAndAC);
            zmm4 = Avx512F.AlignRight32(zmm31, zmm6, 4);
            zmm5 = Avx512F.TernaryLogic(zmm5, zmm7, zmm6, OrBAndAC);
            zmm8 = Avx512F.AlignRight32(zmm31, zmm10, 4);
            zmm9 = Avx512F.TernaryLogic(zmm9, zmm11, zmm10, OrBAndAC);
            zmm12 = Avx512F.AlignRight32(zmm31, zmm14, 4);
            zmm13 = Avx512F.TernaryLogic(zmm13, zmm15, zmm14, OrBAndAC);
            zmm0 &= zmm2;
            zmm3 = Avx512F.AlignRight32(zmm31, zmm1, 8);
            zmm4 &= zmm6;
            zmm7 = Avx512F.AlignRight32(zmm31, zmm5, 8);
            zmm8 &= zmm10;
            zmm11 = Avx512F.AlignRight32(zmm31, zmm9, 8);
            zmm3 = Avx512F.TernaryLogic(zmm3, zmm1, zmm0, OrBAndAC);
            zmm12 &= zmm14;
            zmm7 = Avx512F.TernaryLogic(zmm7, zmm5, zmm4, OrBAndAC);
            zmm15 = Avx512F.AlignRight32(zmm31, zmm13, 8);
            zmm11 = Avx512F.TernaryLogic(zmm11, zmm9, zmm8, OrBAndAC);
            zmm15 = Avx512F.TernaryLogic(zmm15, zmm13, zmm12, OrBAndAC);
            return (new(zmm3.AsUInt16()), new(zmm7.AsUInt16()), new(zmm11.AsUInt16()), new(zmm15.AsUInt16()));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static (PartialBitBoard512 upper, PartialBitBoard512 right, PartialBitBoard512 lower, PartialBitBoard512 left) FillDropReachable4Sets(PartialBitBoard512 upperBoard, PartialBitBoard512 rightBoard, PartialBitBoard512 lowerBoard, PartialBitBoard512 leftBoard, PartialBitBoard512 upperReached, PartialBitBoard512 rightReached, PartialBitBoard512 lowerReached, PartialBitBoard512 leftReached)
        {
            var zmm31 = Vector128<uint>.Zero.ToVector256Unsafe().ToVector512Unsafe();
            var zmm30 = Vector128.CreateScalar(ushort.MaxValue).ToVector256Unsafe().ToVector512Unsafe().AsUInt32();
            const int OrBAndAC = 0xEC;
            var zmm1 = upperReached.storage.AsUInt32();
            var zmm5 = rightReached.storage.AsUInt32();
            var zmm9 = lowerReached.storage.AsUInt32();
            var zmm13 = leftReached.storage.AsUInt32();
            var zmm0 = upperBoard.storage.AsUInt32();
            var zmm4 = rightBoard.storage.AsUInt32();
            var zmm8 = lowerBoard.storage.AsUInt32();
            var zmm12 = leftBoard.storage.AsUInt32();
            var zmm3 = Avx512F.AndNot(zmm30, zmm1);
            zmm3 = Avx512BW.PermuteVar32x16(zmm3.AsUInt16(), Vector512.Create(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, ushort.MinValue)).AsUInt32();
            var zmm7 = Avx512F.AndNot(zmm30, zmm5);
            zmm7 = Avx512BW.PermuteVar32x16(zmm7.AsUInt16(), Vector512.Create(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, ushort.MinValue)).AsUInt32();
            var zmm11 = Avx512F.AndNot(zmm30, zmm9);
            zmm11 = Avx512BW.PermuteVar32x16(zmm11.AsUInt16(), Vector512.Create(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, ushort.MinValue)).AsUInt32();
            var zmm15 = Avx512F.AndNot(zmm30, zmm13);
            zmm15 = Avx512BW.PermuteVar32x16(zmm15.AsUInt16(), Vector512.Create(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, ushort.MinValue)).AsUInt32();
            var zmm2 = Avx512F.AndNot(zmm30, zmm0);
            zmm2 = Avx512BW.PermuteVar32x16(zmm2.AsUInt16(), Vector512.Create(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, ushort.MinValue)).AsUInt32();
            var zmm6 = Avx512F.AndNot(zmm30, zmm4);
            zmm6 = Avx512BW.PermuteVar32x16(zmm6.AsUInt16(), Vector512.Create(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, ushort.MinValue)).AsUInt32();
            var zmm10 = Avx512F.AndNot(zmm30, zmm8);
            zmm10 = Avx512BW.PermuteVar32x16(zmm10.AsUInt16(), Vector512.Create(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, ushort.MinValue)).AsUInt32();
            var zmm14 = Avx512F.AndNot(zmm30, zmm12);
            zmm14 = Avx512BW.PermuteVar32x16(zmm14.AsUInt16(), Vector512.Create(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, ushort.MinValue)).AsUInt32();
            zmm3 = Avx512F.TernaryLogic(zmm3, zmm1, zmm0, OrBAndAC);
            zmm7 = Avx512F.TernaryLogic(zmm7, zmm5, zmm4, OrBAndAC);
            zmm11 = Avx512F.TernaryLogic(zmm11, zmm9, zmm8, OrBAndAC);
            zmm15 = Avx512F.TernaryLogic(zmm15, zmm13, zmm12, OrBAndAC);
            zmm2 &= zmm0;
            zmm6 &= zmm4;
            zmm10 &= zmm8;
            zmm14 &= zmm12;
            zmm1 = Avx512F.AlignRight32(zmm31, zmm3, 1);
            zmm5 = Avx512F.AlignRight32(zmm31, zmm7, 1);
            zmm9 = Avx512F.AlignRight32(zmm31, zmm11, 1);
            zmm13 = Avx512F.AlignRight32(zmm31, zmm15, 1);
            zmm0 = Avx512F.AlignRight32(zmm31, zmm2, 1);
            zmm1 = Avx512F.TernaryLogic(zmm1, zmm3, zmm2, OrBAndAC);
            zmm4 = Avx512F.AlignRight32(zmm31, zmm6, 1);
            zmm5 = Avx512F.TernaryLogic(zmm5, zmm7, zmm6, OrBAndAC);
            zmm8 = Avx512F.AlignRight32(zmm31, zmm10, 1);
            zmm9 = Avx512F.TernaryLogic(zmm9, zmm11, zmm10, OrBAndAC);
            zmm12 = Avx512F.AlignRight32(zmm31, zmm14, 1);
            zmm13 = Avx512F.TernaryLogic(zmm13, zmm15, zmm14, OrBAndAC);
            zmm0 &= zmm2;
            zmm3 = Avx512F.AlignRight32(zmm31, zmm1, 2);
            zmm4 &= zmm6;
            zmm7 = Avx512F.AlignRight32(zmm31, zmm5, 2);
            zmm8 &= zmm10;
            zmm11 = Avx512F.AlignRight32(zmm31, zmm9, 2);
            zmm12 &= zmm14;
            zmm15 = Avx512F.AlignRight32(zmm31, zmm13, 2);
            zmm2 = Avx512F.AlignRight32(zmm31, zmm0, 2);
            zmm3 = Avx512F.TernaryLogic(zmm3, zmm1, zmm0, OrBAndAC);
            zmm6 = Avx512F.AlignRight32(zmm31, zmm4, 2);
            zmm7 = Avx512F.TernaryLogic(zmm7, zmm5, zmm4, OrBAndAC);
            zmm10 = Avx512F.AlignRight32(zmm31, zmm8, 2);
            zmm11 = Avx512F.TernaryLogic(zmm11, zmm9, zmm8, OrBAndAC);
            zmm14 = Avx512F.AlignRight32(zmm31, zmm12, 2);
            zmm15 = Avx512F.TernaryLogic(zmm15, zmm13, zmm12, OrBAndAC);
            zmm2 &= zmm0;
            zmm1 = Avx512F.AlignRight32(zmm31, zmm3, 4);
            zmm6 &= zmm4;
            zmm5 = Avx512F.AlignRight32(zmm31, zmm7, 4);
            zmm10 &= zmm8;
            zmm9 = Avx512F.AlignRight32(zmm31, zmm11, 4);
            zmm14 &= zmm12;
            zmm13 = Avx512F.AlignRight32(zmm31, zmm15, 4);
            zmm0 = Avx512F.AlignRight32(zmm31, zmm2, 4);
            zmm1 = Avx512F.TernaryLogic(zmm1, zmm3, zmm2, OrBAndAC);
            zmm4 = Avx512F.AlignRight32(zmm31, zmm6, 4);
            zmm5 = Avx512F.TernaryLogic(zmm5, zmm7, zmm6, OrBAndAC);
            zmm8 = Avx512F.AlignRight32(zmm31, zmm10, 4);
            zmm9 = Avx512F.TernaryLogic(zmm9, zmm11, zmm10, OrBAndAC);
            zmm12 = Avx512F.AlignRight32(zmm31, zmm14, 4);
            zmm13 = Avx512F.TernaryLogic(zmm13, zmm15, zmm14, OrBAndAC);
            zmm0 &= zmm2;
            zmm3 = Avx512F.AlignRight32(zmm31, zmm1, 8);
            zmm4 &= zmm6;
            zmm7 = Avx512F.AlignRight32(zmm31, zmm5, 8);
            zmm8 &= zmm10;
            zmm11 = Avx512F.AlignRight32(zmm31, zmm9, 8);
            zmm3 = Avx512F.TernaryLogic(zmm3, zmm1, zmm0, OrBAndAC);
            zmm12 &= zmm14;
            zmm7 = Avx512F.TernaryLogic(zmm7, zmm5, zmm4, OrBAndAC);
            zmm15 = Avx512F.AlignRight32(zmm31, zmm13, 8);
            zmm11 = Avx512F.TernaryLogic(zmm11, zmm9, zmm8, OrBAndAC);
            zmm15 = Avx512F.TernaryLogic(zmm15, zmm13, zmm12, OrBAndAC);
            return (new(zmm3.AsUInt16()), new(zmm7.AsUInt16()), new(zmm11.AsUInt16()), new(zmm15.AsUInt16()));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 FillHorizontalReachable(PartialBitBoard512 board, PartialBitBoard512 reached) => FillHorizontalReachableInternal(board, reached);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        internal static PartialBitBoard512 FillHorizontalReachableGfni(PartialBitBoard512 board, PartialBitBoard512 reached)
        {
            var zmm31 = Vector512.Create(
                            (byte)1, 0, 3, 2, 5, 4, 7, 6, 9, 8, 11, 10, 13, 12, 15, 14,
                            1, 0, 3, 2, 5, 4, 7, 6, 9, 8, 11, 10, 13, 12, 15, 14,
                            1, 0, 3, 2, 5, 4, 7, 6, 9, 8, 11, 10, 13, 12, 15, 14,
                            1, 0, 3, 2, 5, 4, 7, 6, 9, 8, 11, 10, 13, 12, 15, 14);
            var zmm30 = Vector512.Create((byte)0x0f).AsUInt16();
            var zmm29 = Vector512.Create(
                0, 128, 64, 192, 32, 160, 96, 224, 16, 144, 80, 208, 48, 176, 112, 240,
                0, 128, 64, 192, 32, 160, 96, 224, 16, 144, 80, 208, 48, 176, 112, 240,
                0, 128, 64, 192, 32, 160, 96, 224, 16, 144, 80, 208, 48, 176, 112, 240,
                0, 128, 64, 192, 32, 160, 96, 224, 16, 144, 80, 208, 48, 176, 112, 240);
            var zmm28 = Vector512.Create(
                (byte)0, 8, 4, 12, 2, 10, 6, 14, 1, 9, 5, 13, 3, 11, 7, 15,
                0, 8, 4, 12, 2, 10, 6, 14, 1, 9, 5, 13, 3, 11, 7, 15,
                0, 8, 4, 12, 2, 10, 6, 14, 1, 9, 5, 13, 3, 11, 7, 15,
                0, 8, 4, 12, 2, 10, 6, 14, 1, 9, 5, 13, 3, 11, 7, 15);
            var zmm0 = board.storage;
            var zmm1 = reached.storage;
            // TODO: AVX512-GFNI Bit Reversal
            var zmm2 = Avx512BW.Shuffle(zmm0.AsByte(), zmm31).AsUInt16();
            var zmm3 = Avx512BW.Shuffle(zmm1.AsByte(), zmm31).AsUInt16();
            var zmm4 = zmm2 >> 4;
            var zmm5 = zmm3 >> 4;
            zmm2 &= zmm30;
            zmm3 &= zmm30;
            zmm4 &= zmm30;
            zmm5 &= zmm30;
            zmm2 = Avx512BW.Shuffle(zmm29, zmm2.AsByte()).AsUInt16();
            zmm3 = Avx512BW.Shuffle(zmm29, zmm3.AsByte()).AsUInt16();
            zmm4 = Avx512BW.Shuffle(zmm28, zmm4.AsByte()).AsUInt16();
            zmm5 = Avx512BW.Shuffle(zmm28, zmm5.AsByte()).AsUInt16();
            zmm2 |= zmm4;   // flipped zmm0
            zmm3 |= zmm5;   // flipped zmm1
            zmm5 = zmm2 + zmm3;
            zmm2 = Avx512F.TernaryLogic(zmm2, zmm3, zmm5, 0xD0);
            zmm2 = Avx512BW.Shuffle(zmm2.AsByte(), zmm31).AsUInt16();
            zmm4 = zmm0 + zmm1;
            zmm0 = Avx512F.TernaryLogic(zmm0, zmm1, zmm4, 0xD0);
            zmm4 = zmm2 >> 4;
            zmm2 &= zmm30;
            zmm4 &= zmm30;
            zmm2 = Avx512BW.Shuffle(zmm29, zmm2.AsByte()).AsUInt16();
            zmm4 = Avx512BW.Shuffle(zmm28, zmm4.AsByte()).AsUInt16();
            zmm2 |= zmm4;
            zmm0 |= zmm2;
            return new(zmm0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        internal static PartialBitBoard512 FillHorizontalReachableInternal(PartialBitBoard512 board, PartialBitBoard512 reached)
        {
            var zmm0 = board.storage;
            var zmm1 = reached.storage;
            var zmm5 = Vector512.ShiftRightLogical(zmm1, 1);
            var zmm4 = Vector512.ShiftRightLogical(zmm0, 1);
            zmm5 = Avx512F.TernaryLogic(zmm5, zmm1, zmm0, 0xEC);
            var zmm2 = zmm0 + zmm1;
            zmm4 &= zmm0;
            zmm2 = Avx512F.TernaryLogic(zmm0, zmm1, zmm2, 0xD0);
            zmm1 = Vector512.ShiftRightLogical(zmm5, 2);
            zmm0 = Vector512.ShiftRightLogical(zmm4, 2);
            zmm1 = Avx512F.TernaryLogic(zmm1, zmm5, zmm4, 0xEC);
            zmm0 &= zmm4;
            zmm5 = Vector512.ShiftRightLogical(zmm1, 4);
            zmm4 = Vector512.ShiftRightLogical(zmm0, 4);
            zmm5 = Avx512F.TernaryLogic(zmm5, zmm1, zmm0, 0xEC);
            zmm4 &= zmm0;
            zmm1 = Vector512.ShiftRightLogical(zmm5, 8);
            zmm1 = Avx512F.TernaryLogic(zmm1, zmm5, zmm4, 0xEC);
            zmm1 |= zmm2;
            return new(zmm1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        internal static PartialBitBoard512 FillHorizontalReachableInternalOld(PartialBitBoard512 board, PartialBitBoard512 reached)
        {
            var zmm0 = board.storage;
            var zmm1 = reached.storage;
            var zmm5 = Vector512.ShiftRightLogical(zmm1, 1);
            var zmm7 = Vector512.ShiftLeft(zmm1, 1);
            var zmm4 = Vector512.ShiftRightLogical(zmm0, 1);
            var zmm6 = Vector512.ShiftLeft(zmm0, 1);
            zmm5 = Avx512F.TernaryLogic(zmm5, zmm1, zmm0, 0xEC);
            zmm7 = Avx512F.TernaryLogic(zmm7, zmm1, zmm0, 0xEC);
            zmm5 |= zmm7;
            zmm4 &= zmm0;
            zmm6 &= zmm0;
            zmm1 = Vector512.ShiftRightLogical(zmm5, 2);
            var zmm3 = Vector512.ShiftLeft(zmm5, 2);
            zmm0 = Vector512.ShiftRightLogical(zmm4, 2);
            var zmm2 = Vector512.ShiftLeft(zmm6, 2);
            zmm1 = Avx512F.TernaryLogic(zmm1, zmm5, zmm4, 0xEC);
            zmm3 = Avx512F.TernaryLogic(zmm3, zmm5, zmm6, 0xEC);
            zmm1 |= zmm3;
            zmm0 &= zmm4;
            zmm2 &= zmm6;
            zmm5 = Vector512.ShiftRightLogical(zmm1, 4);
            zmm7 = Vector512.ShiftLeft(zmm1, 4);
            zmm4 = Vector512.ShiftRightLogical(zmm0, 4);
            zmm6 = Vector512.ShiftLeft(zmm2, 4);
            zmm5 = Avx512F.TernaryLogic(zmm5, zmm1, zmm0, 0xEC);
            zmm7 = Avx512F.TernaryLogic(zmm7, zmm1, zmm2, 0xEC);
            zmm5 |= zmm7;
            zmm4 &= zmm0;
            zmm6 &= zmm2;
            zmm1 = Vector512.ShiftRightLogical(zmm5, 8);
            zmm3 = Vector512.ShiftLeft(zmm5, 8);
            zmm1 = Avx512F.TernaryLogic(zmm1, zmm5, zmm4, 0xEC);
            zmm3 = Avx512F.TernaryLogic(zmm3, zmm5, zmm6, 0xEC);
            zmm1 |= zmm3;
            return new(zmm1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static (PartialBitBoard512 upper, PartialBitBoard512 right, PartialBitBoard512 lower, PartialBitBoard512 left) FillHorizontalReachable4Sets((PartialBitBoard512 upper, PartialBitBoard512 right, PartialBitBoard512 lower, PartialBitBoard512 left) board, (PartialBitBoard512 upper, PartialBitBoard512 right, PartialBitBoard512 lower, PartialBitBoard512 left) reached)
        {
            (var zmm0, var zmm8, var zmm16, var zmm24) = (board.upper.storage, board.right.storage, board.lower.storage, board.left.storage);
            (var zmm1, var zmm9, var zmm17, var zmm25) = (reached.upper.storage, reached.right.storage, reached.lower.storage, reached.left.storage);
            var zmm5 = Vector512.ShiftRightLogical(zmm1, 1);
            var zmm13 = Vector512.ShiftRightLogical(zmm9, 1);
            var zmm21 = Vector512.ShiftRightLogical(zmm17, 1);
            var zmm29 = Vector512.ShiftRightLogical(zmm25, 1);
            var zmm4 = Vector512.ShiftRightLogical(zmm0, 1);
            var zmm12 = Vector512.ShiftRightLogical(zmm8, 1);
            var zmm20 = Vector512.ShiftRightLogical(zmm16, 1);
            var zmm28 = Vector512.ShiftRightLogical(zmm24, 1);
            var zmm2 = zmm0 + zmm1;
            var zmm10 = zmm8 + zmm9;
            var zmm18 = zmm16 + zmm17;
            var zmm26 = zmm24 + zmm25;
            zmm5 = Avx512F.TernaryLogic(zmm5, zmm1, zmm0, 0xEC);
            zmm13 = Avx512F.TernaryLogic(zmm13, zmm9, zmm8, 0xEC);
            zmm21 = Avx512F.TernaryLogic(zmm21, zmm17, zmm16, 0xEC);
            zmm29 = Avx512F.TernaryLogic(zmm29, zmm25, zmm24, 0xEC);
            zmm4 &= zmm0;
            zmm12 &= zmm8;
            zmm20 &= zmm16;
            zmm28 &= zmm24;
            zmm2 = Avx512F.TernaryLogic(zmm2, zmm1, zmm0, 0x8A);
            zmm10 = Avx512F.TernaryLogic(zmm10, zmm9, zmm8, 0x8A);
            zmm18 = Avx512F.TernaryLogic(zmm18, zmm17, zmm16, 0x8A);
            zmm26 = Avx512F.TernaryLogic(zmm26, zmm25, zmm24, 0x8A);
            zmm1 = Vector512.ShiftRightLogical(zmm5, 2);
            zmm9 = Vector512.ShiftRightLogical(zmm13, 2);
            zmm17 = Vector512.ShiftRightLogical(zmm21, 2);
            zmm25 = Vector512.ShiftRightLogical(zmm29, 2);
            zmm0 = Vector512.ShiftRightLogical(zmm4, 2);
            zmm8 = Vector512.ShiftRightLogical(zmm12, 2);
            zmm16 = Vector512.ShiftRightLogical(zmm20, 2);
            zmm24 = Vector512.ShiftRightLogical(zmm28, 2);
            zmm1 = Avx512F.TernaryLogic(zmm1, zmm5, zmm4, 0xEC);
            zmm9 = Avx512F.TernaryLogic(zmm9, zmm13, zmm12, 0xEC);
            zmm17 = Avx512F.TernaryLogic(zmm17, zmm21, zmm20, 0xEC);
            zmm25 = Avx512F.TernaryLogic(zmm25, zmm29, zmm28, 0xEC);
            zmm0 &= zmm4;
            zmm8 &= zmm12;
            zmm16 &= zmm20;
            zmm24 &= zmm28;
            zmm5 = Vector512.ShiftRightLogical(zmm1, 4);
            zmm13 = Vector512.ShiftRightLogical(zmm9, 4);
            zmm21 = Vector512.ShiftRightLogical(zmm17, 4);
            zmm29 = Vector512.ShiftRightLogical(zmm25, 4);
            zmm4 = Vector512.ShiftRightLogical(zmm0, 4);
            zmm12 = Vector512.ShiftRightLogical(zmm8, 4);
            zmm20 = Vector512.ShiftRightLogical(zmm16, 4);
            zmm28 = Vector512.ShiftRightLogical(zmm24, 4);
            zmm5 = Avx512F.TernaryLogic(zmm5, zmm1, zmm0, 0xEC);
            zmm13 = Avx512F.TernaryLogic(zmm13, zmm9, zmm8, 0xEC);
            zmm21 = Avx512F.TernaryLogic(zmm21, zmm17, zmm16, 0xEC);
            zmm29 = Avx512F.TernaryLogic(zmm29, zmm25, zmm24, 0xEC);
            zmm4 &= zmm0;
            zmm12 &= zmm8;
            zmm20 &= zmm16;
            zmm28 &= zmm24;
            zmm1 = Vector512.ShiftRightLogical(zmm5, 8);
            zmm9 = Vector512.ShiftRightLogical(zmm13, 8);
            zmm17 = Vector512.ShiftRightLogical(zmm21, 8);
            zmm25 = Vector512.ShiftRightLogical(zmm29, 8);
            zmm1 = Avx512F.TernaryLogic(zmm1, zmm5, zmm4, 0xEC);
            zmm9 = Avx512F.TernaryLogic(zmm9, zmm13, zmm12, 0xEC);
            zmm17 = Avx512F.TernaryLogic(zmm17, zmm21, zmm20, 0xEC);
            zmm25 = Avx512F.TernaryLogic(zmm25, zmm29, zmm28, 0xEC);
            zmm1 |= zmm2;
            zmm9 |= zmm10;
            zmm17 |= zmm18;
            zmm25 |= zmm26;
            return (new(zmm1), new(zmm9), new(zmm17), new(zmm25));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static (PartialBitBoard512 upper, PartialBitBoard512 right, PartialBitBoard512 lower, PartialBitBoard512 left) FillHorizontalReachable4Sets(PartialBitBoard512 upperBoard, PartialBitBoard512 rightBoard, PartialBitBoard512 lowerBoard, PartialBitBoard512 leftBoard, PartialBitBoard512 upperReached, PartialBitBoard512 rightReached, PartialBitBoard512 lowerReached, PartialBitBoard512 leftReached)
        {
            (var zmm0, var zmm8, var zmm16, var zmm24) = (upperBoard.storage, rightBoard.storage, lowerBoard.storage, leftBoard.storage);
            (var zmm1, var zmm9, var zmm17, var zmm25) = (upperReached.storage, rightReached.storage, lowerReached.storage, leftReached.storage);
            var zmm5 = Vector512.ShiftRightLogical(zmm1, 1);
            var zmm13 = Vector512.ShiftRightLogical(zmm9, 1);
            var zmm21 = Vector512.ShiftRightLogical(zmm17, 1);
            var zmm29 = Vector512.ShiftRightLogical(zmm25, 1);
            var zmm4 = Vector512.ShiftRightLogical(zmm0, 1);
            var zmm12 = Vector512.ShiftRightLogical(zmm8, 1);
            var zmm20 = Vector512.ShiftRightLogical(zmm16, 1);
            var zmm28 = Vector512.ShiftRightLogical(zmm24, 1);
            var zmm2 = zmm0 + zmm1;
            var zmm10 = zmm8 + zmm9;
            var zmm18 = zmm16 + zmm17;
            var zmm26 = zmm24 + zmm25;
            zmm5 = Avx512F.TernaryLogic(zmm5, zmm1, zmm0, 0xEC);
            zmm13 = Avx512F.TernaryLogic(zmm13, zmm9, zmm8, 0xEC);
            zmm21 = Avx512F.TernaryLogic(zmm21, zmm17, zmm16, 0xEC);
            zmm29 = Avx512F.TernaryLogic(zmm29, zmm25, zmm24, 0xEC);
            zmm4 &= zmm0;
            zmm12 &= zmm8;
            zmm20 &= zmm16;
            zmm28 &= zmm24;
            zmm2 = Avx512F.TernaryLogic(zmm2, zmm1, zmm0, 0x8A);
            zmm10 = Avx512F.TernaryLogic(zmm10, zmm9, zmm8, 0x8A);
            zmm18 = Avx512F.TernaryLogic(zmm18, zmm17, zmm16, 0x8A);
            zmm26 = Avx512F.TernaryLogic(zmm26, zmm25, zmm24, 0x8A);
            zmm1 = Vector512.ShiftRightLogical(zmm5, 2);
            zmm9 = Vector512.ShiftRightLogical(zmm13, 2);
            zmm17 = Vector512.ShiftRightLogical(zmm21, 2);
            zmm25 = Vector512.ShiftRightLogical(zmm29, 2);
            zmm0 = Vector512.ShiftRightLogical(zmm4, 2);
            zmm8 = Vector512.ShiftRightLogical(zmm12, 2);
            zmm16 = Vector512.ShiftRightLogical(zmm20, 2);
            zmm24 = Vector512.ShiftRightLogical(zmm28, 2);
            zmm1 = Avx512F.TernaryLogic(zmm1, zmm5, zmm4, 0xEC);
            zmm9 = Avx512F.TernaryLogic(zmm9, zmm13, zmm12, 0xEC);
            zmm17 = Avx512F.TernaryLogic(zmm17, zmm21, zmm20, 0xEC);
            zmm25 = Avx512F.TernaryLogic(zmm25, zmm29, zmm28, 0xEC);
            zmm0 &= zmm4;
            zmm8 &= zmm12;
            zmm16 &= zmm20;
            zmm24 &= zmm28;
            zmm5 = Vector512.ShiftRightLogical(zmm1, 4);
            zmm13 = Vector512.ShiftRightLogical(zmm9, 4);
            zmm21 = Vector512.ShiftRightLogical(zmm17, 4);
            zmm29 = Vector512.ShiftRightLogical(zmm25, 4);
            zmm4 = Vector512.ShiftRightLogical(zmm0, 4);
            zmm12 = Vector512.ShiftRightLogical(zmm8, 4);
            zmm20 = Vector512.ShiftRightLogical(zmm16, 4);
            zmm28 = Vector512.ShiftRightLogical(zmm24, 4);
            zmm5 = Avx512F.TernaryLogic(zmm5, zmm1, zmm0, 0xEC);
            zmm13 = Avx512F.TernaryLogic(zmm13, zmm9, zmm8, 0xEC);
            zmm21 = Avx512F.TernaryLogic(zmm21, zmm17, zmm16, 0xEC);
            zmm29 = Avx512F.TernaryLogic(zmm29, zmm25, zmm24, 0xEC);
            zmm4 &= zmm0;
            zmm12 &= zmm8;
            zmm20 &= zmm16;
            zmm28 &= zmm24;
            zmm1 = Vector512.ShiftRightLogical(zmm5, 8);
            zmm9 = Vector512.ShiftRightLogical(zmm13, 8);
            zmm17 = Vector512.ShiftRightLogical(zmm21, 8);
            zmm25 = Vector512.ShiftRightLogical(zmm29, 8);
            zmm1 = Avx512F.TernaryLogic(zmm1, zmm5, zmm4, 0xEC);
            zmm9 = Avx512F.TernaryLogic(zmm9, zmm13, zmm12, 0xEC);
            zmm17 = Avx512F.TernaryLogic(zmm17, zmm21, zmm20, 0xEC);
            zmm25 = Avx512F.TernaryLogic(zmm25, zmm29, zmm28, 0xEC);
            zmm1 |= zmm2;
            zmm9 |= zmm10;
            zmm17 |= zmm18;
            zmm25 |= zmm26;
            return (new(zmm1), new(zmm9), new(zmm17), new(zmm25));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        internal static (PartialBitBoard512 upper, PartialBitBoard512 right, PartialBitBoard512 lower, PartialBitBoard512 left) FillHorizontalReachable4SetsOld((PartialBitBoard512 upper, PartialBitBoard512 right, PartialBitBoard512 lower, PartialBitBoard512 left) board, (PartialBitBoard512 upper, PartialBitBoard512 right, PartialBitBoard512 lower, PartialBitBoard512 left) reached)
        {
            (var zmm0, var zmm8, var zmm16, var zmm24) = (board.upper.storage, board.right.storage, board.lower.storage, board.left.storage);
            (var zmm1, var zmm9, var zmm17, var zmm25) = (reached.upper.storage, reached.right.storage, reached.lower.storage, reached.left.storage);
            var zmm5 = Vector512.ShiftRightLogical(zmm1, 1);
            var zmm13 = Vector512.ShiftRightLogical(zmm9, 1);
            var zmm21 = Vector512.ShiftRightLogical(zmm17, 1);
            var zmm29 = Vector512.ShiftRightLogical(zmm25, 1);
            var zmm7 = Vector512.ShiftLeft(zmm1, 1);
            var zmm15 = Vector512.ShiftLeft(zmm9, 1);
            var zmm23 = Vector512.ShiftLeft(zmm17, 1);
            var zmm31 = Vector512.ShiftLeft(zmm25, 1);
            var zmm4 = Vector512.ShiftRightLogical(zmm0, 1);
            var zmm12 = Vector512.ShiftRightLogical(zmm8, 1);
            var zmm20 = Vector512.ShiftRightLogical(zmm16, 1);
            var zmm28 = Vector512.ShiftRightLogical(zmm24, 1);
            var zmm6 = Vector512.ShiftLeft(zmm0, 1);
            var zmm14 = Vector512.ShiftLeft(zmm8, 1);
            var zmm22 = Vector512.ShiftLeft(zmm16, 1);
            var zmm30 = Vector512.ShiftLeft(zmm24, 1);
            zmm5 = Avx512F.TernaryLogic(zmm5, zmm1, zmm0, 0xEC);
            zmm13 = Avx512F.TernaryLogic(zmm13, zmm9, zmm8, 0xEC);
            zmm21 = Avx512F.TernaryLogic(zmm21, zmm17, zmm16, 0xEC);
            zmm29 = Avx512F.TernaryLogic(zmm29, zmm25, zmm24, 0xEC);
            zmm7 = Avx512F.TernaryLogic(zmm7, zmm1, zmm0, 0xEC);
            zmm15 = Avx512F.TernaryLogic(zmm15, zmm9, zmm8, 0xEC);
            zmm23 = Avx512F.TernaryLogic(zmm23, zmm17, zmm16, 0xEC);
            zmm31 = Avx512F.TernaryLogic(zmm31, zmm25, zmm24, 0xEC);
            zmm5 |= zmm7;
            zmm13 |= zmm15;
            zmm21 |= zmm23;
            zmm29 |= zmm31;
            zmm4 &= zmm0;
            zmm12 &= zmm8;
            zmm20 &= zmm16;
            zmm28 &= zmm24;
            zmm6 &= zmm0;
            zmm14 &= zmm8;
            zmm22 &= zmm16;
            zmm30 &= zmm24;
            zmm1 = Vector512.ShiftRightLogical(zmm5, 2);
            zmm9 = Vector512.ShiftRightLogical(zmm13, 2);
            zmm17 = Vector512.ShiftRightLogical(zmm21, 2);
            zmm25 = Vector512.ShiftRightLogical(zmm29, 2);
            var zmm3 = Vector512.ShiftLeft(zmm5, 2);
            var zmm11 = Vector512.ShiftLeft(zmm13, 2);
            var zmm19 = Vector512.ShiftLeft(zmm21, 2);
            var zmm27 = Vector512.ShiftLeft(zmm29, 2);
            zmm0 = Vector512.ShiftRightLogical(zmm4, 2);
            zmm8 = Vector512.ShiftRightLogical(zmm12, 2);
            zmm16 = Vector512.ShiftRightLogical(zmm20, 2);
            zmm24 = Vector512.ShiftRightLogical(zmm28, 2);
            var zmm2 = Vector512.ShiftLeft(zmm6, 2);
            var zmm10 = Vector512.ShiftLeft(zmm14, 2);
            var zmm18 = Vector512.ShiftLeft(zmm22, 2);
            var zmm26 = Vector512.ShiftLeft(zmm30, 2);
            zmm1 = Avx512F.TernaryLogic(zmm1, zmm5, zmm4, 0xEC);
            zmm9 = Avx512F.TernaryLogic(zmm9, zmm13, zmm12, 0xEC);
            zmm17 = Avx512F.TernaryLogic(zmm17, zmm21, zmm20, 0xEC);
            zmm25 = Avx512F.TernaryLogic(zmm25, zmm29, zmm28, 0xEC);
            zmm3 = Avx512F.TernaryLogic(zmm3, zmm5, zmm6, 0xEC);
            zmm11 = Avx512F.TernaryLogic(zmm11, zmm13, zmm14, 0xEC);
            zmm19 = Avx512F.TernaryLogic(zmm19, zmm21, zmm22, 0xEC);
            zmm27 = Avx512F.TernaryLogic(zmm27, zmm29, zmm30, 0xEC);
            zmm1 |= zmm3;
            zmm9 |= zmm11;
            zmm17 |= zmm19;
            zmm25 |= zmm27;
            zmm0 &= zmm4;
            zmm8 &= zmm12;
            zmm16 &= zmm20;
            zmm24 &= zmm28;
            zmm2 &= zmm6;
            zmm10 &= zmm14;
            zmm18 &= zmm22;
            zmm26 &= zmm30;
            zmm5 = Vector512.ShiftRightLogical(zmm1, 4);
            zmm13 = Vector512.ShiftRightLogical(zmm9, 4);
            zmm21 = Vector512.ShiftRightLogical(zmm17, 4);
            zmm29 = Vector512.ShiftRightLogical(zmm25, 4);
            zmm7 = Vector512.ShiftLeft(zmm1, 4);
            zmm15 = Vector512.ShiftLeft(zmm9, 4);
            zmm23 = Vector512.ShiftLeft(zmm17, 4);
            zmm31 = Vector512.ShiftLeft(zmm25, 4);
            zmm4 = Vector512.ShiftRightLogical(zmm0, 4);
            zmm12 = Vector512.ShiftRightLogical(zmm8, 4);
            zmm20 = Vector512.ShiftRightLogical(zmm16, 4);
            zmm28 = Vector512.ShiftRightLogical(zmm24, 4);
            zmm6 = Vector512.ShiftLeft(zmm2, 4);
            zmm14 = Vector512.ShiftLeft(zmm10, 4);
            zmm22 = Vector512.ShiftLeft(zmm18, 4);
            zmm30 = Vector512.ShiftLeft(zmm26, 4);
            zmm5 = Avx512F.TernaryLogic(zmm5, zmm1, zmm0, 0xEC);
            zmm13 = Avx512F.TernaryLogic(zmm13, zmm9, zmm8, 0xEC);
            zmm21 = Avx512F.TernaryLogic(zmm21, zmm17, zmm16, 0xEC);
            zmm29 = Avx512F.TernaryLogic(zmm29, zmm25, zmm24, 0xEC);
            zmm7 = Avx512F.TernaryLogic(zmm7, zmm1, zmm2, 0xEC);
            zmm15 = Avx512F.TernaryLogic(zmm15, zmm9, zmm10, 0xEC);
            zmm23 = Avx512F.TernaryLogic(zmm23, zmm17, zmm18, 0xEC);
            zmm31 = Avx512F.TernaryLogic(zmm31, zmm25, zmm26, 0xEC);
            zmm5 |= zmm7;
            zmm13 |= zmm15;
            zmm21 |= zmm23;
            zmm29 |= zmm31;
            zmm4 &= zmm0;
            zmm12 &= zmm8;
            zmm20 &= zmm16;
            zmm28 &= zmm24;
            zmm6 &= zmm2;
            zmm14 &= zmm10;
            zmm22 &= zmm18;
            zmm30 &= zmm26;
            zmm1 = Vector512.ShiftRightLogical(zmm5, 8);
            zmm9 = Vector512.ShiftRightLogical(zmm13, 8);
            zmm17 = Vector512.ShiftRightLogical(zmm21, 8);
            zmm25 = Vector512.ShiftRightLogical(zmm29, 8);
            zmm3 = Vector512.ShiftLeft(zmm5, 8);
            zmm11 = Vector512.ShiftLeft(zmm13, 8);
            zmm19 = Vector512.ShiftLeft(zmm21, 8);
            zmm27 = Vector512.ShiftLeft(zmm29, 8);
            zmm1 = Avx512F.TernaryLogic(zmm1, zmm5, zmm4, 0xEC);
            zmm9 = Avx512F.TernaryLogic(zmm9, zmm13, zmm12, 0xEC);
            zmm17 = Avx512F.TernaryLogic(zmm17, zmm21, zmm20, 0xEC);
            zmm25 = Avx512F.TernaryLogic(zmm25, zmm29, zmm28, 0xEC);
            zmm3 = Avx512F.TernaryLogic(zmm3, zmm5, zmm6, 0xEC);
            zmm11 = Avx512F.TernaryLogic(zmm11, zmm13, zmm14, 0xEC);
            zmm19 = Avx512F.TernaryLogic(zmm19, zmm21, zmm22, 0xEC);
            zmm27 = Avx512F.TernaryLogic(zmm27, zmm29, zmm30, 0xEC);
            zmm1 |= zmm3;
            zmm9 |= zmm11;
            zmm17 |= zmm19;
            zmm25 |= zmm27;
            return (new(zmm1), new(zmm9), new(zmm17), new(zmm25));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        internal static (PartialBitBoard512 upper, PartialBitBoard512 right, PartialBitBoard512 lower, PartialBitBoard512 left) FillHorizontalReachable4SetsOld(PartialBitBoard512 upperBoard, PartialBitBoard512 rightBoard, PartialBitBoard512 lowerBoard, PartialBitBoard512 leftBoard, PartialBitBoard512 upperReached, PartialBitBoard512 rightReached, PartialBitBoard512 lowerReached, PartialBitBoard512 leftReached)
        {
            (var zmm0, var zmm8, var zmm16, var zmm24) = (upperBoard.storage, rightBoard.storage, lowerBoard.storage, leftBoard.storage);
            (var zmm1, var zmm9, var zmm17, var zmm25) = (upperReached.storage, rightReached.storage, lowerReached.storage, leftReached.storage);
            var zmm5 = Vector512.ShiftRightLogical(zmm1, 1);
            var zmm13 = Vector512.ShiftRightLogical(zmm9, 1);
            var zmm21 = Vector512.ShiftRightLogical(zmm17, 1);
            var zmm29 = Vector512.ShiftRightLogical(zmm25, 1);
            var zmm7 = Vector512.ShiftLeft(zmm1, 1);
            var zmm15 = Vector512.ShiftLeft(zmm9, 1);
            var zmm23 = Vector512.ShiftLeft(zmm17, 1);
            var zmm31 = Vector512.ShiftLeft(zmm25, 1);
            var zmm4 = Vector512.ShiftRightLogical(zmm0, 1);
            var zmm12 = Vector512.ShiftRightLogical(zmm8, 1);
            var zmm20 = Vector512.ShiftRightLogical(zmm16, 1);
            var zmm28 = Vector512.ShiftRightLogical(zmm24, 1);
            var zmm6 = Vector512.ShiftLeft(zmm0, 1);
            var zmm14 = Vector512.ShiftLeft(zmm8, 1);
            var zmm22 = Vector512.ShiftLeft(zmm16, 1);
            var zmm30 = Vector512.ShiftLeft(zmm24, 1);
            zmm5 = Avx512F.TernaryLogic(zmm5, zmm1, zmm0, 0xEC);
            zmm13 = Avx512F.TernaryLogic(zmm13, zmm9, zmm8, 0xEC);
            zmm21 = Avx512F.TernaryLogic(zmm21, zmm17, zmm16, 0xEC);
            zmm29 = Avx512F.TernaryLogic(zmm29, zmm25, zmm24, 0xEC);
            zmm7 = Avx512F.TernaryLogic(zmm7, zmm1, zmm0, 0xEC);
            zmm15 = Avx512F.TernaryLogic(zmm15, zmm9, zmm8, 0xEC);
            zmm23 = Avx512F.TernaryLogic(zmm23, zmm17, zmm16, 0xEC);
            zmm31 = Avx512F.TernaryLogic(zmm31, zmm25, zmm24, 0xEC);
            zmm5 |= zmm7;
            zmm13 |= zmm15;
            zmm21 |= zmm23;
            zmm29 |= zmm31;
            zmm4 &= zmm0;
            zmm12 &= zmm8;
            zmm20 &= zmm16;
            zmm28 &= zmm24;
            zmm6 &= zmm0;
            zmm14 &= zmm8;
            zmm22 &= zmm16;
            zmm30 &= zmm24;
            zmm1 = Vector512.ShiftRightLogical(zmm5, 2);
            zmm9 = Vector512.ShiftRightLogical(zmm13, 2);
            zmm17 = Vector512.ShiftRightLogical(zmm21, 2);
            zmm25 = Vector512.ShiftRightLogical(zmm29, 2);
            var zmm3 = Vector512.ShiftLeft(zmm5, 2);
            var zmm11 = Vector512.ShiftLeft(zmm13, 2);
            var zmm19 = Vector512.ShiftLeft(zmm21, 2);
            var zmm27 = Vector512.ShiftLeft(zmm29, 2);
            zmm0 = Vector512.ShiftRightLogical(zmm4, 2);
            zmm8 = Vector512.ShiftRightLogical(zmm12, 2);
            zmm16 = Vector512.ShiftRightLogical(zmm20, 2);
            zmm24 = Vector512.ShiftRightLogical(zmm28, 2);
            var zmm2 = Vector512.ShiftLeft(zmm6, 2);
            var zmm10 = Vector512.ShiftLeft(zmm14, 2);
            var zmm18 = Vector512.ShiftLeft(zmm22, 2);
            var zmm26 = Vector512.ShiftLeft(zmm30, 2);
            zmm1 = Avx512F.TernaryLogic(zmm1, zmm5, zmm4, 0xEC);
            zmm9 = Avx512F.TernaryLogic(zmm9, zmm13, zmm12, 0xEC);
            zmm17 = Avx512F.TernaryLogic(zmm17, zmm21, zmm20, 0xEC);
            zmm25 = Avx512F.TernaryLogic(zmm25, zmm29, zmm28, 0xEC);
            zmm3 = Avx512F.TernaryLogic(zmm3, zmm5, zmm6, 0xEC);
            zmm11 = Avx512F.TernaryLogic(zmm11, zmm13, zmm14, 0xEC);
            zmm19 = Avx512F.TernaryLogic(zmm19, zmm21, zmm22, 0xEC);
            zmm27 = Avx512F.TernaryLogic(zmm27, zmm29, zmm30, 0xEC);
            zmm1 |= zmm3;
            zmm9 |= zmm11;
            zmm17 |= zmm19;
            zmm25 |= zmm27;
            zmm0 &= zmm4;
            zmm8 &= zmm12;
            zmm16 &= zmm20;
            zmm24 &= zmm28;
            zmm2 &= zmm6;
            zmm10 &= zmm14;
            zmm18 &= zmm22;
            zmm26 &= zmm30;
            zmm5 = Vector512.ShiftRightLogical(zmm1, 4);
            zmm13 = Vector512.ShiftRightLogical(zmm9, 4);
            zmm21 = Vector512.ShiftRightLogical(zmm17, 4);
            zmm29 = Vector512.ShiftRightLogical(zmm25, 4);
            zmm7 = Vector512.ShiftLeft(zmm1, 4);
            zmm15 = Vector512.ShiftLeft(zmm9, 4);
            zmm23 = Vector512.ShiftLeft(zmm17, 4);
            zmm31 = Vector512.ShiftLeft(zmm25, 4);
            zmm4 = Vector512.ShiftRightLogical(zmm0, 4);
            zmm12 = Vector512.ShiftRightLogical(zmm8, 4);
            zmm20 = Vector512.ShiftRightLogical(zmm16, 4);
            zmm28 = Vector512.ShiftRightLogical(zmm24, 4);
            zmm6 = Vector512.ShiftLeft(zmm2, 4);
            zmm14 = Vector512.ShiftLeft(zmm10, 4);
            zmm22 = Vector512.ShiftLeft(zmm18, 4);
            zmm30 = Vector512.ShiftLeft(zmm26, 4);
            zmm5 = Avx512F.TernaryLogic(zmm5, zmm1, zmm0, 0xEC);
            zmm13 = Avx512F.TernaryLogic(zmm13, zmm9, zmm8, 0xEC);
            zmm21 = Avx512F.TernaryLogic(zmm21, zmm17, zmm16, 0xEC);
            zmm29 = Avx512F.TernaryLogic(zmm29, zmm25, zmm24, 0xEC);
            zmm7 = Avx512F.TernaryLogic(zmm7, zmm1, zmm2, 0xEC);
            zmm15 = Avx512F.TernaryLogic(zmm15, zmm9, zmm10, 0xEC);
            zmm23 = Avx512F.TernaryLogic(zmm23, zmm17, zmm18, 0xEC);
            zmm31 = Avx512F.TernaryLogic(zmm31, zmm25, zmm26, 0xEC);
            zmm5 |= zmm7;
            zmm13 |= zmm15;
            zmm21 |= zmm23;
            zmm29 |= zmm31;
            zmm4 &= zmm0;
            zmm12 &= zmm8;
            zmm20 &= zmm16;
            zmm28 &= zmm24;
            zmm6 &= zmm2;
            zmm14 &= zmm10;
            zmm22 &= zmm18;
            zmm30 &= zmm26;
            zmm1 = Vector512.ShiftRightLogical(zmm5, 8);
            zmm9 = Vector512.ShiftRightLogical(zmm13, 8);
            zmm17 = Vector512.ShiftRightLogical(zmm21, 8);
            zmm25 = Vector512.ShiftRightLogical(zmm29, 8);
            zmm3 = Vector512.ShiftLeft(zmm5, 8);
            zmm11 = Vector512.ShiftLeft(zmm13, 8);
            zmm19 = Vector512.ShiftLeft(zmm21, 8);
            zmm27 = Vector512.ShiftLeft(zmm29, 8);
            zmm1 = Avx512F.TernaryLogic(zmm1, zmm5, zmm4, 0xEC);
            zmm9 = Avx512F.TernaryLogic(zmm9, zmm13, zmm12, 0xEC);
            zmm17 = Avx512F.TernaryLogic(zmm17, zmm21, zmm20, 0xEC);
            zmm25 = Avx512F.TernaryLogic(zmm25, zmm29, zmm28, 0xEC);
            zmm3 = Avx512F.TernaryLogic(zmm3, zmm5, zmm6, 0xEC);
            zmm11 = Avx512F.TernaryLogic(zmm11, zmm13, zmm14, 0xEC);
            zmm19 = Avx512F.TernaryLogic(zmm19, zmm21, zmm22, 0xEC);
            zmm27 = Avx512F.TernaryLogic(zmm27, zmm29, zmm30, 0xEC);
            zmm1 |= zmm3;
            zmm9 |= zmm11;
            zmm17 |= zmm19;
            zmm25 |= zmm27;
            return (new(zmm1), new(zmm9), new(zmm17), new(zmm25));
        }

        #endregion

        #region Mask Operators
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector512<ushort> MaskUnaryNegation(Vector512<ushort> mask) => (~mask.AsUInt32()).AsUInt16();
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector512<ushort> MaskAnd(Vector512<ushort> left, Vector512<ushort> right) => left & right;
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector512<ushort> MaskOr(Vector512<ushort> left, Vector512<ushort> right) => left | right;
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector512<ushort> MaskXor(Vector512<ushort> left, Vector512<ushort> right) => left ^ right;
        #endregion

        #region Mask Utilities
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int TrailingZeroCount(uint mask) => BitOperations.TrailingZeroCount(mask);
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int PopCount(uint mask) => BitOperations.PopCount(mask);
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int LineHeight(uint mask) => sizeof(uint) * 8 - BitOperations.LeadingZeroCount(mask);
        #endregion

        #region Per-Line Operations
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector512<ushort> CompareEqualPerLineVector(PartialBitBoard512 left, PartialBitBoard512 right)
            => Vector512.Equals(left.storage, right.storage);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static uint CompareEqualPerLineCompact(PartialBitBoard512 left, PartialBitBoard512 right)
            => (uint)Vector512.Equals(left.storage, right.storage).ExtractMostSignificantBits();

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector512<ushort> CompareNotEqualPerLineVector(PartialBitBoard512 left, PartialBitBoard512 right)
            => Avx512BW.CompareNotEqual(left.storage, right.storage);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static uint CompareNotEqualPerLineCompact(PartialBitBoard512 left, PartialBitBoard512 right)
            => (uint)Avx512BW.CompareNotEqual(left.storage, right.storage).ExtractMostSignificantBits();

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 LineSelect(Vector512<ushort> mask, PartialBitBoard512 left, PartialBitBoard512 right)
            => new(Avx512BW.BlendVariable(left.storage, right.storage, mask));

        #endregion

        #region ClearLines
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector512<ushort> GetClearableLinesVector(PartialBitBoard512 board) => Vector512.Equals(board.storage, Vector512<ushort>.AllBitsSet);
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 ClearClearableLines(PartialBitBoard512 board, ushort fill, out Vector512<ushort> clearedLines)
        {
            var lines = GetClearableLinesVector(board);
            clearedLines = lines;
            return ClearLines(board, fill, lines);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 ClearLines(PartialBitBoard512 board, ushort fill, Vector512<ushort> lines)
            => /*Vector512.EqualsAll(lines, default) ? board : */ClearLinesAvx512BW(board, fill, lines);

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        [SkipLocalsInit]
        internal static PartialBitBoard512 ClearLinesFallback(PartialBitBoard512 board, ushort fill, Vector512<ushort> lines)
        {
            var zmm0 = lines;
            // Using PackSignedSaturate because all values from either ymm0 or ymm1 are all one of either 0 or -1.
            var mask = ~(nint)zmm0.ExtractMostSignificantBits();
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
        internal static PartialBitBoard512 ClearLinesBmi2(PartialBitBoard512 board, ushort fill, Vector512<ushort> lines)
        {
            var zmm0 = board.storage;
            var zmm2 = lines;
            if (Vector512.EqualsAll(zmm2, default))
            {
                return new(zmm0);
            }
            if (Bmi2.X64.IsSupported)
            {
                zmm2 = (~zmm2.AsUInt32()).AsUInt16();
                var ymm0 = zmm0.GetLower().AsUInt64();
                var ymm1 = zmm0.GetUpper().AsUInt64();
                var ymm2 = zmm2.GetLower().AsUInt64();
                var ymm3 = zmm2.GetUpper().AsUInt64();
                var zmm4 = Vector512.Create(fill);
                var fHeight = new FixedArray32<ushort>();
                Span<ushort> res = fHeight;
                ref var head = ref MemoryMarshal.GetReference(res);
                zmm4.StoreUnsafe(ref head);
                nint j = 0, k;
                var maskPart0 = ymm2.GetElement(0);
                var boardPart0 = ymm0.GetElement(0);
                boardPart0 = Bmi2.X64.ParallelBitExtract(boardPart0, maskPart0);
                k = (nint)Popcnt.X64.PopCount(maskPart0) >> 4;
                Unsafe.As<ushort, ulong>(ref Unsafe.Add(ref head, j)) = boardPart0;
                j += k;
                var maskPart1 = ymm2.GetElement(1);
                var xmm2 = ymm2.GetUpper();
                var boardPart1 = ymm0.GetElement(1);
                var xmm0 = ymm0.GetUpper();
                boardPart1 = Bmi2.X64.ParallelBitExtract(boardPart1, maskPart1);
                k = (nint)Popcnt.X64.PopCount(maskPart1) >> 4;
                Unsafe.As<ushort, ulong>(ref Unsafe.Add(ref head, j)) = boardPart1;
                j += k;
                var maskPart2 = xmm2.GetElement(0);
                var boardPart2 = xmm0.GetElement(0);
                boardPart2 = Bmi2.X64.ParallelBitExtract(boardPart2, maskPart2);
                k = (nint)Popcnt.X64.PopCount(maskPart2) >> 4;
                Unsafe.As<ushort, ulong>(ref Unsafe.Add(ref head, j)) = boardPart2;
                j += k;
                var maskPart3 = xmm2.GetElement(1);
                var boardPart3 = xmm0.GetElement(1);
                boardPart3 = Bmi2.X64.ParallelBitExtract(boardPart3, maskPart3);
                k = (nint)Popcnt.X64.PopCount(maskPart3) >> 4;
                Unsafe.As<ushort, ulong>(ref Unsafe.Add(ref head, j)) = boardPart3;
                j += k;
                var maskPart4 = ymm3.GetElement(0);
                var boardPart4 = ymm1.GetElement(0);
                boardPart4 = Bmi2.X64.ParallelBitExtract(boardPart4, maskPart4);
                k = (nint)Popcnt.X64.PopCount(maskPart4) >> 4;
                Unsafe.As<ushort, ulong>(ref Unsafe.Add(ref head, j)) = boardPart4;
                j += k;
                var maskPart5 = ymm3.GetElement(1);
                xmm2 = ymm3.GetUpper();
                var boardPart5 = ymm1.GetElement(1);
                xmm0 = ymm1.GetUpper();
                boardPart5 = Bmi2.X64.ParallelBitExtract(boardPart5, maskPart5);
                k = (nint)Popcnt.X64.PopCount(maskPart5) >> 4;
                Unsafe.As<ushort, ulong>(ref Unsafe.Add(ref head, j)) = boardPart5;
                j += k;
                var maskPart6 = xmm2.GetElement(0);
                var boardPart6 = xmm0.GetElement(0);
                boardPart6 = Bmi2.X64.ParallelBitExtract(boardPart6, maskPart6);
                k = (nint)Popcnt.X64.PopCount(maskPart6) >> 4;
                Unsafe.As<ushort, ulong>(ref Unsafe.Add(ref head, j)) = boardPart6;
                j += k;
                var maskPart7 = xmm2.GetElement(1);
                var boardPart7 = xmm0.GetElement(1);
                boardPart7 = Bmi2.X64.ParallelBitExtract(boardPart7, maskPart7);
                k = (nint)Popcnt.X64.PopCount(maskPart7) >> 4;
                Unsafe.As<ushort, ulong>(ref Unsafe.Add(ref head, j)) = boardPart7;
                j += k;
                switch (res.Length - j)
                {
                    case >= 4:
                        Unsafe.As<ushort, double>(ref Unsafe.Add(ref head, j)) = zmm4.AsDouble().GetElement(0);
                        break;
                    case 3:
                        Unsafe.Add(ref head, j + 2) = fill;
                        goto case 2;
                    case 2:
                        Unsafe.As<ushort, float>(ref Unsafe.Add(ref head, j)) = zmm4.AsSingle().GetElement(0);
                        break;
                    case 1:
                        Unsafe.Add(ref head, j) = fill;
                        break;
                    default:
                        break;
                }
                zmm0 = Vector512.LoadUnsafe(ref head);
                return new(zmm0);
            }
            return ClearLinesFallback(board, fill, lines);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        [SkipLocalsInit]
        internal static PartialBitBoard512 ClearLinesAvx512BW(PartialBitBoard512 board, ushort fill, Vector512<ushort> lines)
        {
            if (Avx512BW.IsSupported)
            {
                var zmm0 = board.storage;
                var zmm1 = lines;
                var zmm2 = zmm1 & Vector512.Create((ushort)16);
                zmm0 = Avx512F.AndNot(zmm1, zmm0);
                var zmm3 = Avx512F.ShiftRightLogical(zmm2.AsUInt32(), 16).AsUInt16();
                var zmm31 = zmm2 & Vector512.Create(0x0000_ffffu).AsUInt16();
                zmm0 = Avx512F.ShiftRightLogicalVariable(zmm0.AsUInt32(), zmm31.AsUInt32()).AsUInt16();
                zmm2 = zmm31 + zmm3;
                zmm3 = Avx512F.ShiftRightLogical(zmm2.AsUInt64(), 32).AsUInt16();
                var zmm30 = zmm2 & Vector512.Create(0x0000_0000_ffff_fffful).AsUInt16();
                zmm2 = zmm30 + zmm3;
                zmm2 = Avx512BW.ShiftRightLogical(zmm2, 3);
                var zmm16 = Avx512F.ShiftRightLogicalVariable(zmm0.AsUInt64(), zmm30.AsUInt64()).AsUInt16();
                zmm30 = Avx512BW.Shuffle(zmm2.AsByte(), Vector512<byte>.Zero).AsUInt16();
                zmm0 |= zmm16;
                zmm31 = zmm2 & Vector512.Create(-1, 0, -1, 0, -1, 0, -1, 0).AsUInt16();
                zmm3 = Avx512F.Permute4x32(zmm2.AsSingle(), 0b11_11_11_10).AsUInt16();
                var zmm29 = (zmm30.AsByte() + Vector512.Create(byte.MinValue, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15)).AsUInt16();
                var k7 = Avx512BW.CompareGreaterThan(zmm30.AsSByte(), Vector512.Create(6, 6, 4, 4, 2, 2, 0, 0, -1, -1, -1, -1, -1, -1, -1, -1, 6, 6, 4, 4, 2, 2, 0, 0, -1, -1, -1, -1, -1, -1, -1, -1, 6, 6, 4, 4, 2, 2, 0, 0, -1, -1, -1, -1, -1, -1, -1, -1, 6, 6, 4, 4, 2, 2, 0, 0, -1, -1, -1, -1, -1, -1, -1, -1));
                zmm2 = zmm31 + zmm3;
                zmm29 = Avx512BW.Shuffle(zmm0.AsSByte(), zmm29.AsSByte()).AsUInt16();
                zmm2 = Avx512BW.ShiftRightLogical(zmm2, 1);
                zmm30 = Avx512BW.PermuteVar32x16(zmm2, Vector512.Create(ushort.MinValue, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16));
                zmm31 = zmm2 & Vector512.Create(-1, -1, 0, 0, -1, -1, 0, 0).AsUInt16();
                zmm3 = Avx512F.Permute4x64(zmm2.AsUInt64(), 0b11_11_11_10).AsUInt16();
                zmm0 = Vector512.ConditionalSelect(k7, zmm29.AsSByte(), zmm0.AsSByte()).AsUInt16();
                var zmm28 = zmm30 + Vector512.Create(ushort.MinValue, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31);
                zmm28 = Avx512BW.PermuteVar32x16(zmm0, zmm28);
                zmm2 = zmm31 + zmm3;
                var k6 = Avx512BW.CompareGreaterThan(zmm30.AsInt16(), Vector512.Create(7, 6, 5, 4, 3, 2, 1, 0, -1, -1, -1, -1, -1, -1, -1, -1, 7, 6, 5, 4, 3, 2, 1, 0, -1, -1, -1, -1, -1, -1, -1, -1));
                var xmm2 = zmm2.GetLower().GetLower();
                zmm31 = Avx512BW.BroadcastScalarToVector512(xmm2);
                zmm29 = zmm31 + Vector512.Create(ushort.MinValue, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31);
                zmm0 = Vector512.ConditionalSelect(k6, zmm28.AsInt16(), zmm0.AsInt16()).AsUInt16();
                zmm29 = Avx512BW.PermuteVar32x16(zmm0, zmm29);
                var xmm3 = zmm2.GetUpper().GetLower();
                xmm2 += xmm3;
                zmm28 = Vector512.Create(fill);
                zmm3 = Avx512BW.BroadcastScalarToVector512(xmm2);
                var k5 = Avx512BW.CompareGreaterThan(zmm3.AsInt16(), Vector512.Create(31, 30, 29, 28, 27, 26, 25, 24, 23, 22, 21, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0));
                zmm28 = Avx512BW.BlendVariable(zmm29.AsInt16(), zmm28.AsInt16(), k5).AsUInt16();
                k6 = Avx512BW.CompareGreaterThan(zmm31.AsInt16(), Vector512.Create(15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1));
                zmm0 = Avx512BW.BlendVariable(zmm0.AsInt16(), zmm28.AsInt16(), k6).AsUInt16();
                return new(zmm0);
            }
            return ClearLinesBmi2(board, fill, lines);
        }

        public static PartialBitBoard512 ClearClearableLines(PartialBitBoard512 board, ushort fill) => ClearClearableLines(board, fill, out _);
        #endregion

        #region Board Classification
        public static bool IsBoardZero(PartialBitBoard512 board)
            => Vector512.EqualsAll(board.storage, Vector512<ushort>.Zero);
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool GetBlockAt(ushort line, int x) => PartialBitBoard256X2.GetBlockAt(line, x);
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool GetBlockAtFullRange(ushort line, int x) => PartialBitBoard256X2.GetBlockAtFullRange(line, x);
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int LocateAllBlocks(PartialBitBoard512 board, IBufferWriter<CompressedPointList> writer)
        {
            if (Vector512.EqualsAll(board.Value, default)) return 0;
            var count = TotalBlocks(board);
            var compressedCount = (count + 2) / 3;
            var buffer = writer.GetSpan(compressedCount);
            uint compressing = 0;
            var modShift = 0;
            ref var bHead = ref Unsafe.As<PartialBitBoard512, ushort>(ref board);
            nint j = 0;
            var i = 0;
            uint yCoord = 0;
            for (; j < Height; j++, yCoord += 1 << 4)
            {
                var line = (uint)Unsafe.Add(ref bHead, j) << 16;
                while (true)
                {
                    var pos = (uint)BitOperations.LeadingZeroCount(line);
                    if (pos >= 16) break;
                    line = MathI.ZeroHighBitsFromHigh((int)pos + 1, line);
                    var coord = yCoord | pos;
                    compressing |= coord << modShift;
                    modShift += 10;
                    if (modShift < 30) continue;
                    modShift = 0;
                    buffer[i] = new(compressing | 0xc000_0000u);
                    compressing = 0;
                    i++;
                }
            }
            if ((uint)(modShift - 1) < 29)  // x - 1 brings 0 to uint.MaxValue
            {
                var mcount = (uint)modShift / 10u;
                buffer[i] = new(compressing | (mcount << 30));
                i++;
            }
            writer.Advance(i);
            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int TotalBlocks(PartialBitBoard512 board) => TotalBlocks(board.storage);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int TotalBlocks(Vector512<ushort> board)
        {
            var zmm0 = board.AsByte();
            var zmm1 = Vector512.Create(~0x0f0f0f0fu).AsByte();
            var zmm2 = zmm0 & zmm1;
            var zmm3 = Vector512.Create(byte.MinValue, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4, 0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4, 0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4, 0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4);
            var zmm31 = Vector128.Create((byte)0, 8, 16, 24, 32, 40, 48, 56, 0, 0, 0, 0, 0, 0, 0, 0).ToVector256Unsafe().ToVector512Unsafe();
            zmm2 = Vector512.ShiftRightLogical(zmm2.AsUInt16(), 4).AsByte();
            zmm0 = Avx512F.AndNot(zmm1, zmm0);
            var xmm4 = Vector128<byte>.Zero;
            var zmm4 = xmm4.ToVector256Unsafe().ToVector512Unsafe();
            zmm0 = Avx512BW.Shuffle(zmm3, zmm0);
            zmm2 = Avx512BW.Shuffle(zmm3, zmm2);
            zmm0 += zmm2;
            zmm0 = Avx512BW.SumAbsoluteDifferences(zmm0, zmm4).AsByte();
            var xmm0 = Avx512Vbmi.IsSupported ? Avx512Vbmi.PermuteVar64x8(zmm0, zmm31).GetLower().GetLower() : Avx512F.ConvertToVector128Byte(zmm0.AsUInt64());
            xmm0 = Sse2.SumAbsoluteDifferences(xmm0, xmm4).AsByte();
            return xmm0.AsInt32().GetElement(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int GetBlockHeight(PartialBitBoard512 board) => GetBlockHeight(board.storage);

        internal static int GetBlockHeight(Vector512<ushort> storage)
        {
            var zmm0 = storage;
            var zmm31 = Vector512.Create(FullBitBoard.InvertedEmptyRow);
            zmm0 &= zmm31;
            var k1 = Avx512BW.CompareNotEqual(zmm0, Vector512<ushort>.Zero);
            return BitOperations.LeadingZeroCount(0u) - BitOperations.LeadingZeroCount((uint)k1.ExtractMostSignificantBits());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard512 BlocksPerLine(PartialBitBoard512 board)
        {
            var zmm0 = board.storage.AsByte();
            var zmm1 = Vector512.Create(~0x0f0f0f0fu).AsByte();
            var zmm2 = zmm0 & zmm1;
            var zmm3 = Vector512.Create(byte.MinValue, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4, 0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4, 0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4, 0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4);
            zmm2 = Vector512.ShiftRightLogical(zmm2.AsUInt16(), 4).AsByte();
            zmm0 = Avx512F.AndNot(zmm1, zmm0);
            zmm0 = Avx512BW.Shuffle(zmm3, zmm0);
            zmm2 = Avx512BW.Shuffle(zmm3, zmm2);
            zmm0 += zmm2;
            zmm2 = Vector512.ShiftLeft(zmm0.AsUInt16(), 8).AsByte();
            zmm0 += zmm2;
            return new(Vector512.ShiftRightLogical(zmm0.AsUInt16(), 8));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public bool Equals(PartialBitBoard512 other) => this == other;
        public override bool Equals(object? obj) => obj is PartialBitBoard512 board && Equals(board);

        public override int GetHashCode() => HashCode.Combine(CalculateHash(this));

        private string GetDebuggerDisplay() => ((PartialBitBoard256X2)this).GetDebuggerDisplay();

        public override string ToString() => GetDebuggerDisplay();
        public static bool IsSetAt(Vector512<ushort> mask, byte index)
        {
            var m = mask.ExtractMostSignificantBits();
            return ((m >> index) & 1) > 0;
        }

        #endregion

        #region Hash Operations
        public static ulong CalculateHash(PartialBitBoard512 board, ulong key = default)
        {
            var zmm0 = board.storage;
            var zmm31 = ~Vector512<ushort>.Indices;
            var zmm30 = Vector512.Create((byte)0x0f).AsUInt16();
            var zmm29 = Vector512.Create(
                168, 162, 130, 34, 8, 170, 32, 40, 42, 0, 138, 10, 2, 128, 160, 136,
                160, 32, 136, 130, 170, 40, 10, 2, 0, 42, 8, 168, 162, 138, 34, 128,
                8, 130, 40, 34, 0, 160, 138, 170, 168, 10, 32, 2, 162, 42, 128, 136,
                160, 42, 130, 34, 0, 138, 8, 168, 170, 136, 10, 2, 162, 128, 40, 32);
            var zmm28 = Vector512.Create((byte)
                68, 80, 64, 65, 21, 1, 4, 5, 69, 20, 17, 85, 81, 0, 16, 84,
                65, 81, 68, 20, 80, 85, 0, 69, 5, 1, 84, 16, 21, 4, 17, 64,
                5, 64, 20, 4, 84, 80, 81, 0, 1, 17, 16, 85, 68, 21, 65, 69,
                17, 4, 69, 84, 20, 64, 5, 85, 81, 21, 80, 16, 68, 65, 0, 1);
            var zmm26 = Vector512.Create(50754, 48049, 64569, 1817, 27326, 6051, 37990, 33887, 16956, 13030, 41406, 11722, 2980, 38674, 34301, 478, 14287, 22547, 34803, 23018, 64232, 62172, 25075, 50580, 50423, 3753, 46921, 50138, 43248, 11191, 21398, 65253);
            var zmm27 = Vector512.Create(
                        8, 2, 7, 1, 6, 10, 5, 0, 4, 11, 14, 3, 9, 15, 13, 12,
                        6, 4, 0, 1, 8, 7, 11, 9, 13, 3, 14, 15, 5, 2, 12, 10,
                        3, 13, 10, 6, 5, 2, 14, 4, 11, 0, 8, 12, 9, 15, 1, 7,
                        15, 12, 6, 8, 11, 0, 9, 3, 14, 4, 2, 7, 10, 5, 13, (byte)1);
            var ymm15 = Vector256.Create(11231, 43655, 58474, 2655, 62987, 28428, 50195, 12224, 14485, 35104, 8666, 12975, 22706, 52105, 12257, 40543);
            var xmm14 = Vector128.Create(30215, 55752, 31273, 50575, 33383, 9724, 18163, 46263);
            var height = (uint)GetBlockHeight(zmm0);
            var zmm3 = Vector512.Create(key).AsUInt16() ^ zmm26;
            zmm0 ^= zmm3;
            var zmm1 = Vector512.Create((ushort)height);
            zmm31 += zmm1;
            var zmm2 = zmm0 >> 4;
            zmm0 &= zmm30;
            zmm2 &= zmm30;
            zmm2 = Avx512BW.Shuffle(zmm28, zmm2.AsByte()).AsUInt16();
            zmm0 = Avx512BW.Shuffle(zmm29, zmm0.AsByte()).AsUInt16();
            zmm0 |= zmm2;
            zmm1 = Avx512BW.PermuteVar32x16(zmm0, zmm31);
            zmm1 += zmm3;
            var zmm4 = zmm3;
            zmm0 = zmm1;
            for (var i = 0; i < 8; i++)
            {
                zmm1 = Avx512F.AlignRight32(zmm0.AsUInt32(), zmm0.AsUInt32(), 4).AsUInt16();
                zmm2 = Avx512BW.Shuffle(zmm0.AsByte(), zmm27).AsUInt16();
                zmm4 += zmm3;
                zmm2 >>= 4;
                zmm1 &= zmm30;
                zmm2 &= zmm30;
                zmm2 = Avx512BW.Shuffle(zmm28, zmm2.AsByte()).AsUInt16();
                zmm1 = Avx512BW.Shuffle(zmm29, zmm1.AsByte()).AsUInt16();
                zmm4 ^= zmm2 | zmm1;
                zmm1 = Avx512F.AlignRight32(zmm0.AsUInt32(), zmm0.AsUInt32(), 7).AsUInt16();
                zmm2 = Avx512BW.Shuffle(zmm0.AsByte(), zmm27).AsUInt16();
                zmm0 += zmm3;
                zmm2 >>= 4;
                zmm1 &= zmm30;
                zmm2 &= zmm30;
                zmm2 = Avx512BW.Shuffle(zmm29, zmm2.AsByte()).AsUInt16();
                zmm1 = Avx512BW.Shuffle(zmm28, zmm1.AsByte()).AsUInt16();
                zmm0 ^= zmm2 | zmm1;
            }
            var ymm1 = ymm15 + zmm0.GetLower();
            var ymm2 = ymm1 >> 4;
            ymm2 &= zmm30.GetLower();
            ymm1 &= zmm30.GetLower();
            ymm2 = Avx2.Shuffle(zmm28.GetLower(), ymm2.AsByte()).AsUInt16();
            ymm1 = Avx2.Shuffle(zmm29.GetLower(), ymm1.AsByte()).AsUInt16();
            var ymm0 = zmm0.GetLower() + (ymm1 | ymm2);
            var xmm1 = ymm0.GetUpper();
            var xmm2 = Pclmulqdq.CarrylessMultiply(xmm1.AsUInt64(), xmm14.AsUInt64(), 0x00).AsUInt16();
            xmm1 = Pclmulqdq.CarrylessMultiply(xmm1.AsUInt64(), xmm14.AsUInt64(), 0x11).AsUInt16();
            xmm1 = Sse.Shuffle(xmm1.AsSingle(), xmm2.AsSingle(), 0b01_00_01_00).AsUInt16();
            var xmm0 = ymm0.GetLower() + xmm1;
            var rcx = xmm0.AsUInt64().GetElement(1);
            var rax = xmm0.AsUInt64().GetElement(0);
            rcx *= 9452341668337194139ul;
            rax += rcx;
            return rax;
            //return zmm0.AsUInt64().GetElement(0);
        }
        #endregion
    }

#endif
}
