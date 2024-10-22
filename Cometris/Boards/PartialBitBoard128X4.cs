using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.Wasm;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

using Cometris.Utils;

using MikoMino.Boards;

namespace Cometris.Boards
{
    /// <summary>
    /// Bit board that records only the bottom 32 out of 40 lines.<br/>
    /// This structure is for mainstream hardware-accelerated board operations, mainly for Arm64 target with AdvSimd available.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = sizeof(ushort) * Height)]
    public readonly partial struct PartialBitBoard128X4 : ICompactMaskableBitBoard<PartialBitBoard128X4, ushort, PartialBitBoard128X4, uint>
    {
        /// <summary>
        /// 32 = <see cref="Vector512{T}.Count"/> for <see cref="ushort"/>.
        /// </summary>
        public const int Height = PartialBitBoard256X2.Height;

        public const int EffectiveWidth = PartialBitBoard256X2.EffectiveWidth;

        private readonly Vector128<ushort> storage0;
        private readonly Vector128<ushort> storage1;
        private readonly Vector128<ushort> storage2;
        private readonly Vector128<ushort> storage3;

        public PartialBitBoard128X4(Vector128<ushort> storage0, Vector128<ushort> storage1, Vector128<ushort> storage2, Vector128<ushort> storage3)
        {
            this.storage0 = storage0;
            this.storage1 = storage1;
            this.storage2 = storage2;
            this.storage3 = storage3;
        }

        public PartialBitBoard128X4(ushort fill)
        {
            var v0_8h = Vector128.Create(fill);
            storage0 = v0_8h;
            storage1 = v0_8h;
            storage2 = v0_8h;
            storage3 = v0_8h;
        }

        public ushort this[int y]
        {
            get
            {
                if (AdvSimd.IsSupported)
                {
                    var v0_4h = Vector64.Create((byte)(y * 2));
                    return AdvSimd.VectorTableLookup((storage0.AsByte(), storage1.AsByte(), storage2.AsByte(), storage3.AsByte()), v0_4h).AsUInt16().GetElement(0);
                }
                // Fallback
                var storage = (y >> 3) switch
                {
                    0 => storage0,
                    1 => storage1,
                    2 => storage2,
                    _ => storage3,
                };
                y &= 7;
                return storage.GetElementVariable(y);
            }
        }

        public static ushort EmptyLine => FullBitBoard.EmptyRow;
        static int IBitBoard<PartialBitBoard128X4, ushort>.BitPositionXLeftmost => (16 - EffectiveWidth) / 2 + EffectiveWidth;
        static int IBitBoard<PartialBitBoard128X4, ushort>.Height => Height;
        public static int RightmostPaddingWidth => 3;
        public static int StorableWidth => sizeof(ushort) * 8;
        public static bool IsBitwiseOperationHardwareAccelerated => Vector128.IsHardwareAccelerated;
        public static bool IsHorizontalConstantShiftHardwareAccelerated => Vector128.IsHardwareAccelerated;
        public static bool IsHorizontalVariableShiftSupported => Vector128.IsHardwareAccelerated;
        public static bool IsSupported => Vector128.IsHardwareAccelerated;
        public static bool IsVerticalShiftSupported => Vector128.IsHardwareAccelerated;
        public static int MaxEnregisteredLocals => AdvSimd.Arm64.IsSupported ? 8 : 4;
        static int IBitBoard<PartialBitBoard128X4, ushort>.EffectiveWidth => EffectiveWidth;

        public static PartialBitBoard128X4 ZeroMask => new(0);
        public static PartialBitBoard128X4 Zero => default;
        public static PartialBitBoard128X4 AllBitsSetMask => new(ushort.MaxValue);

        public static PartialBitBoard128X4 AllBitsSet => new(ushort.MaxValue);

        public void Deconstruct(out Vector128<ushort> storage0, out Vector128<ushort> storage1, out Vector128<ushort> storage2, out Vector128<ushort> storage3)
            => (storage0, storage1, storage2, storage3) = (this.storage0, this.storage1, this.storage2, this.storage3);

        public static PartialBitBoard128X4 ClearClearableLines(PartialBitBoard128X4 board, ushort fill) => throw new NotImplementedException();
        public static PartialBitBoard128X4 CreateSingleBlock(int x, int y) => CreateSingleLine((ushort)(0x8000u >> x), y);
        public static ushort CreateSingleBlockLine(int x) => PartialBitBoard512.CreateSingleBlockLine(x);
        public static PartialBitBoard128X4 CreateSingleLine(ushort line, int y)
        {
            var v15_8h = Vector128<ushort>.Indices;
            var v14_8h = Vector128.Create((ushort)8, 9, 10, 11, 12, 13, 14, 15);
            var v13_8h = Vector128.Create((ushort)16, 17, 18, 19, 20, 21, 22, 23);
            var v12_8h = Vector128.Create((ushort)24, 25, 26, 27, 28, 29, 30, 31);
            var v8_8h = Vector128.Create(line);
            var v9_8h = Vector128.Create((ushort)y);
            var v0_8h = Vector128.Equals(v15_8h, v9_8h);
            var v1_8h = Vector128.Equals(v14_8h, v9_8h);
            var v2_8h = Vector128.Equals(v13_8h, v9_8h);
            var v3_8h = Vector128.Equals(v12_8h, v9_8h);
            v0_8h &= v8_8h;
            v1_8h &= v8_8h;
            v2_8h &= v8_8h;
            v3_8h &= v8_8h;
            return new(v0_8h, v1_8h, v2_8h, v3_8h);
        }
        public static PartialBitBoard128X4 CreateThreeAdjacentLines(int y, ushort lineLower, ushort lineMiddle, ushort lineUpper)
        {
            var board = Zero;
            board = board.WithLine(lineLower, y - 1);
            board = board.WithLine(lineMiddle, y);
            board = board.WithLine(lineUpper, y + 1);
            return board;
        }
        public static PartialBitBoard128X4 CreateTwoLines(int y0, int y1, ushort line0, ushort line1)
        {
            var board = Zero;
            board = board.WithLine(line0, y0);
            board = board.WithLine(line1, y1);
            return board;
        }
        public static PartialBitBoard128X4 CreateVerticalI4Piece(int x, int y)
        {
            var board = new PartialBitBoard128X4(Vector128.Create(0x8000, 0x8000, 0x8000, 0x8000, 0, 0, 0, 0), Vector128<ushort>.Zero, Vector128<ushort>.Zero, Vector128<ushort>.Zero);
            return ShiftUpVariableLines(board, y - 1, default);
        }
        public static PartialBitBoard128X4 FillDropReachable(PartialBitBoard128X4 board, PartialBitBoard128X4 reached) => throw new NotImplementedException();
        public static PartialBitBoard128X4 FromBoard(ReadOnlySpan<ushort> board, ushort fill)
        {
            if (board.IsEmpty) return new(fill);
            ref var head = ref MemoryMarshal.GetReference(board);
            if (board.Length >= Height)
            {
                return new(Vector128.LoadUnsafe(ref head),
                    Vector128.LoadUnsafe(ref head, 1 * (nuint)Vector128<ushort>.Count),
                    Vector128.LoadUnsafe(ref head, 2 * (nuint)Vector128<ushort>.Count),
                    Vector128.LoadUnsafe(ref head, 3 * (nuint)Vector128<ushort>.Count));
            }
            // Fallback
            return FromBoardFallback(board, fill);
        }

        [SkipLocalsInit]
        private static PartialBitBoard128X4 FromBoardFallback(ReadOnlySpan<ushort> board, ushort fill)
        {
            var filled = Vector128.Create(fill);
            Span<Vector128<ushort>> a = [filled, filled, filled, filled];
            _ = board.TryCopyTo(MemoryMarshal.Cast<Vector128<ushort>, ushort>(a));
            return new(a[0], a[1], a[2], a[3]);
        }
        public static bool GetBlockAt(ushort line, int x) => PartialBitBoard512.GetBlockAt(line, x);
        public static bool GetBlockAtFullRange(ushort line, int x) => PartialBitBoard512.GetBlockAtFullRange(line, x);
        public static int GetBlockHeight(PartialBitBoard128X4 board) => throw new NotImplementedException();
        public static PartialBitBoard128X4 LoadUnsafe(ref ushort source, nint elementOffset)
        {
            var v0_8h = Vector128.LoadUnsafe(ref source, (nuint)elementOffset + 0 * (nuint)Vector128<ushort>.Count);
            var v1_8h = Vector128.LoadUnsafe(ref source, (nuint)elementOffset + 1 * (nuint)Vector128<ushort>.Count);
            var v2_8h = Vector128.LoadUnsafe(ref source, (nuint)elementOffset + 2 * (nuint)Vector128<ushort>.Count);
            var v3_8h = Vector128.LoadUnsafe(ref source, (nuint)elementOffset + 3 * (nuint)Vector128<ushort>.Count);
            return new(v0_8h, v1_8h, v2_8h, v3_8h);
        }
        public static PartialBitBoard128X4 LoadUnsafe(ref ushort source, nuint elementOffset = 0U)
        {
            var v0_8h = Vector128.LoadUnsafe(ref source, elementOffset + 0 * (nuint)Vector128<ushort>.Count);
            var v1_8h = Vector128.LoadUnsafe(ref source, elementOffset + 1 * (nuint)Vector128<ushort>.Count);
            var v2_8h = Vector128.LoadUnsafe(ref source, elementOffset + 2 * (nuint)Vector128<ushort>.Count);
            var v3_8h = Vector128.LoadUnsafe(ref source, elementOffset + 3 * (nuint)Vector128<ushort>.Count);
            return new(v0_8h, v1_8h, v2_8h, v3_8h);
        }
        public static int LocateAllBlocks(PartialBitBoard128X4 board, IBufferWriter<CompressedPositionsTuple> writer) => throw new NotImplementedException();
        public static PartialBitBoard128X4 ShiftDownOneLine(PartialBitBoard128X4 board, ushort upperFeedValue)
        {
            var (v0_16b, v1_16b, v2_16b, v3_16b) = (board.storage0.AsByte(), board.storage1.AsByte(), board.storage2.AsByte(), board.storage3.AsByte());
            var v7_16b = Vector128.Create(upperFeedValue).AsByte();
            if (AdvSimd.Arm64.IsSupported)
            {
                var v8_16b = Vector128.Create((byte)2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17);
                var v4_16b = AdvSimd.Arm64.VectorTableLookup((v0_16b, v1_16b), v8_16b);
                var v5_16b = AdvSimd.Arm64.VectorTableLookup((v1_16b, v2_16b), v8_16b);
                var v6_16b = AdvSimd.Arm64.VectorTableLookup((v2_16b, v3_16b), v8_16b);
                v7_16b = AdvSimd.Arm64.VectorTableLookupExtension(v7_16b, v3_16b, v8_16b);
                return new(v4_16b.AsUInt16(), v5_16b.AsUInt16(), v6_16b.AsUInt16(), v7_16b.AsUInt16());
            }
            else
            {
                var v8_16b = Vector128.Create((byte)2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 0, 1);
                var v12_16b = Vector128.Create(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255);
                v0_16b = VectorUtils.VectorTableLookup(v0_16b, v8_16b);
                v1_16b = VectorUtils.VectorTableLookup(v1_16b, v8_16b);
                v2_16b = VectorUtils.VectorTableLookup(v2_16b, v8_16b);
                v3_16b = VectorUtils.VectorTableLookup(v3_16b, v8_16b);
                v0_16b = VectorUtils.BlendVariable(v0_16b, v1_16b, v12_16b);
                v1_16b = VectorUtils.BlendVariable(v1_16b, v2_16b, v12_16b);
                v2_16b = VectorUtils.BlendVariable(v2_16b, v3_16b, v12_16b);
                v3_16b = VectorUtils.BlendVariable(v3_16b, v7_16b, v12_16b);
                return new(v0_16b.AsUInt16(), v1_16b.AsUInt16(), v2_16b.AsUInt16(), v3_16b.AsUInt16());
            }
        }
        public static PartialBitBoard128X4 ShiftDownOneLine(PartialBitBoard128X4 board, PartialBitBoard128X4 upperFeedBoard)
        {
            var (v0_16b, v1_16b, v2_16b, v3_16b) = (board.storage0.AsByte(), board.storage1.AsByte(), board.storage2.AsByte(), board.storage3.AsByte());
            var v7_16b = upperFeedBoard.storage0.AsByte();
            if (AdvSimd.Arm64.IsSupported)
            {
                var v8_16b = Vector128.Create((byte)2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17);
                var v4_16b = AdvSimd.Arm64.VectorTableLookup((v0_16b, v1_16b), v8_16b);
                var v5_16b = AdvSimd.Arm64.VectorTableLookup((v1_16b, v2_16b), v8_16b);
                var v6_16b = AdvSimd.Arm64.VectorTableLookup((v2_16b, v3_16b), v8_16b);
                v7_16b = AdvSimd.Arm64.VectorTableLookupExtension(v7_16b, v3_16b, v8_16b);
                return new(v4_16b.AsUInt16(), v5_16b.AsUInt16(), v6_16b.AsUInt16(), v7_16b.AsUInt16());
            }
            else
            {
                var v8_16b = Vector128.Create((byte)2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 0, 1);
                var v12_16b = Vector128.Create(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255);
                v0_16b = VectorUtils.VectorTableLookup(v0_16b, v8_16b);
                v1_16b = VectorUtils.VectorTableLookup(v1_16b, v8_16b);
                v2_16b = VectorUtils.VectorTableLookup(v2_16b, v8_16b);
                v3_16b = VectorUtils.VectorTableLookup(v3_16b, v8_16b);
                v0_16b = VectorUtils.BlendVariable(v0_16b, v1_16b, v12_16b);
                v1_16b = VectorUtils.BlendVariable(v1_16b, v2_16b, v12_16b);
                v2_16b = VectorUtils.BlendVariable(v2_16b, v3_16b, v12_16b);
                v3_16b = VectorUtils.BlendVariable(v3_16b, v7_16b, v12_16b);
                return new(v0_16b.AsUInt16(), v1_16b.AsUInt16(), v2_16b.AsUInt16(), v3_16b.AsUInt16());
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard128X4 ShiftDownVariableLines(PartialBitBoard128X4 board, int count, ushort upperFeedValue)
        {
            count &= 31;
            var v8_16b = Vector128.Create((byte)(count * 2));
            var (v0_16b, v1_16b, v2_16b, v3_16b) = (board.storage0.AsByte(), board.storage1.AsByte(), board.storage2.AsByte(), board.storage3.AsByte());
            var v7_16b = Vector128.Create(upperFeedValue).AsByte();
            if (AdvSimd.Arm64.IsSupported)
            {
                var v9_16b = Vector128.Create((byte)16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31) + v8_16b;
                var v10_16b = Vector128.Create((byte)32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47) + v8_16b;
                var v11_16b = Vector128.Create((byte)48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63) + v8_16b;
                v8_16b = Vector128<byte>.Indices + v8_16b;
                var v4_16b = AdvSimd.Arm64.VectorTableLookupExtension(v7_16b, (v0_16b, v1_16b, v2_16b, v3_16b), v8_16b);
                var v5_16b = AdvSimd.Arm64.VectorTableLookupExtension(v7_16b, (v0_16b, v1_16b, v2_16b, v3_16b), v9_16b);
                var v6_16b = AdvSimd.Arm64.VectorTableLookupExtension(v7_16b, (v0_16b, v1_16b, v2_16b, v3_16b), v10_16b);
                v7_16b = AdvSimd.Arm64.VectorTableLookupExtension(v7_16b, (v0_16b, v1_16b, v2_16b, v3_16b), v11_16b);
                return new(v4_16b.AsUInt16(), v5_16b.AsUInt16(), v6_16b.AsUInt16(), v7_16b.AsUInt16());
            }
            else
            {
                var v12_16b = Vector128.GreaterThan((v8_16b & Vector128.Create((byte)0b10_0000)).AsSByte(), Vector128<sbyte>.Zero).AsByte();
                var v13_16b = Vector128.GreaterThan((v8_16b & Vector128.Create((byte)0b01_0000)).AsSByte(), Vector128<sbyte>.Zero).AsByte();
                v8_16b &= Vector128.Create((byte)0x0e);
                var v14_16b = Vector128<byte>.Indices + v8_16b;
                v0_16b = VectorUtils.BlendVariable(v0_16b, v2_16b, v12_16b);
                v1_16b = VectorUtils.BlendVariable(v1_16b, v3_16b, v12_16b);
                v2_16b = VectorUtils.BlendVariable(v2_16b, v7_16b, v12_16b);
                v3_16b = VectorUtils.BlendVariable(v3_16b, v7_16b, v12_16b);
                v12_16b = Vector128.GreaterThan(v14_16b.AsSByte(), Vector128.Create((sbyte)15)).AsByte();
                v0_16b = VectorUtils.BlendVariable(v0_16b, v1_16b, v13_16b);
                v0_16b = VectorUtils.VectorTableLookup(v0_16b, v14_16b);
                v1_16b = VectorUtils.BlendVariable(v1_16b, v2_16b, v13_16b);
                v1_16b = VectorUtils.VectorTableLookup(v1_16b, v14_16b);
                if (!Ssse3.IsSupported) v14_16b &= Vector128.Create((byte)0x0f);
                v2_16b = VectorUtils.BlendVariable(v2_16b, v3_16b, v13_16b);
                v2_16b = VectorUtils.VectorTableLookup(v2_16b, v14_16b);
                v3_16b = VectorUtils.BlendVariable(v3_16b, v7_16b, v13_16b);
                v3_16b = VectorUtils.VectorTableLookup(v3_16b, v14_16b);
                v0_16b = VectorUtils.BlendVariable(v0_16b, v1_16b, v12_16b);
                v1_16b = VectorUtils.BlendVariable(v1_16b, v2_16b, v12_16b);
                v2_16b = VectorUtils.BlendVariable(v2_16b, v3_16b, v12_16b);
                v3_16b = VectorUtils.BlendVariable(v3_16b, v7_16b, v12_16b);
                return new(v0_16b.AsUInt16(), v1_16b.AsUInt16(), v2_16b.AsUInt16(), v3_16b.AsUInt16());
            }
        }
        public static PartialBitBoard128X4 ShiftUpOneLine(PartialBitBoard128X4 board, ushort lowerFeedValue)
        {
            var (v0_16b, v1_16b, v2_16b, v3_16b) = (board.storage0.AsByte(), board.storage1.AsByte(), board.storage2.AsByte(), board.storage3.AsByte());
            var v4_16b = Vector128.Create(lowerFeedValue).AsByte();
            if (AdvSimd.Arm64.IsSupported)
            {
                var v8_16b = Vector128.Create((byte)14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29);
                v4_16b = AdvSimd.Arm64.VectorTableLookup((v4_16b, v0_16b), v8_16b);
                var v5_16b = AdvSimd.Arm64.VectorTableLookup((v0_16b, v1_16b), v8_16b);
                var v6_16b = AdvSimd.Arm64.VectorTableLookup((v1_16b, v2_16b), v8_16b);
                var v7_16b = AdvSimd.Arm64.VectorTableLookup((v2_16b, v3_16b), v8_16b);
                return new(v4_16b.AsUInt16(), v5_16b.AsUInt16(), v6_16b.AsUInt16(), v7_16b.AsUInt16());
            }
            else
            {
                var v8_16b = Vector128.Create((byte)14, 15, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13);
                var v12_16b = Vector128.Create(255, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                v3_16b = VectorUtils.VectorTableLookup(v3_16b, v8_16b);
                v2_16b = VectorUtils.VectorTableLookup(v2_16b, v8_16b);
                v1_16b = VectorUtils.VectorTableLookup(v1_16b, v8_16b);
                v0_16b = VectorUtils.VectorTableLookup(v0_16b, v8_16b);
                v3_16b = VectorUtils.BlendVariable(v3_16b, v2_16b, v12_16b);
                v2_16b = VectorUtils.BlendVariable(v2_16b, v1_16b, v12_16b);
                v1_16b = VectorUtils.BlendVariable(v1_16b, v0_16b, v12_16b);
                v0_16b = VectorUtils.BlendVariable(v0_16b, v4_16b, v12_16b);
                return new(v0_16b.AsUInt16(), v1_16b.AsUInt16(), v2_16b.AsUInt16(), v3_16b.AsUInt16());
            }
        }
        public static PartialBitBoard128X4 ShiftUpOneLine(PartialBitBoard128X4 board, PartialBitBoard128X4 lowerFeedBoard)
        {
            var (v0_16b, v1_16b, v2_16b, v3_16b) = (board.storage0.AsByte(), board.storage1.AsByte(), board.storage2.AsByte(), board.storage3.AsByte());
            var v4_16b = lowerFeedBoard.storage0.AsByte();
            if (AdvSimd.Arm64.IsSupported)
            {
                var v8_16b = Vector128.Create((byte)14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29);
                v4_16b = AdvSimd.Arm64.VectorTableLookup((v4_16b, v0_16b), v8_16b);
                var v5_16b = AdvSimd.Arm64.VectorTableLookup((v0_16b, v1_16b), v8_16b);
                var v6_16b = AdvSimd.Arm64.VectorTableLookup((v1_16b, v2_16b), v8_16b);
                var v7_16b = AdvSimd.Arm64.VectorTableLookup((v2_16b, v3_16b), v8_16b);
                return new(v4_16b.AsUInt16(), v5_16b.AsUInt16(), v6_16b.AsUInt16(), v7_16b.AsUInt16());
            }
            else
            {
                var v8_16b = Vector128.Create((byte)14, 15, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13);
                var v12_16b = Vector128.Create(255, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                v3_16b = VectorUtils.VectorTableLookup(v3_16b, v8_16b);
                v2_16b = VectorUtils.VectorTableLookup(v2_16b, v8_16b);
                v1_16b = VectorUtils.VectorTableLookup(v1_16b, v8_16b);
                v0_16b = VectorUtils.VectorTableLookup(v0_16b, v8_16b);
                v3_16b = VectorUtils.BlendVariable(v3_16b, v2_16b, v12_16b);
                v2_16b = VectorUtils.BlendVariable(v2_16b, v1_16b, v12_16b);
                v1_16b = VectorUtils.BlendVariable(v1_16b, v0_16b, v12_16b);
                v0_16b = VectorUtils.BlendVariable(v0_16b, v4_16b, v12_16b);
                return new(v0_16b.AsUInt16(), v1_16b.AsUInt16(), v2_16b.AsUInt16(), v3_16b.AsUInt16());
            }
        }

        public static PartialBitBoard128X4 ShiftUpVariableLines(PartialBitBoard128X4 board, int count, PartialBitBoard128X4 lowerFeedBoard)
        {
            count &= 31;
            var v8_16b = Vector128.Create((byte)(count * 2));
            var (v0_16b, v1_16b, v2_16b, v3_16b) = (board.storage0.AsByte(), board.storage1.AsByte(), board.storage2.AsByte(), board.storage3.AsByte());
            var (v4_16b, v5_16b, v6_16b, v7_16b) = (lowerFeedBoard.storage0.AsByte(), lowerFeedBoard.storage1.AsByte(), lowerFeedBoard.storage2.AsByte(), lowerFeedBoard.storage3.AsByte());
            if (AdvSimd.Arm64.IsSupported)
            {
                var v9_16b = Vector128.Create((byte)16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31) - v8_16b;
                var v10_16b = Vector128.Create((byte)32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47) - v8_16b;
                var v11_16b = Vector128.Create((byte)48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63) - v8_16b;
                v8_16b = Vector128<byte>.Indices - v8_16b;
                v4_16b = AdvSimd.Arm64.VectorTableLookupExtension(v4_16b, (v0_16b, v1_16b, v2_16b, v3_16b), v8_16b);
                v5_16b = AdvSimd.Arm64.VectorTableLookupExtension(v5_16b, (v0_16b, v1_16b, v2_16b, v3_16b), v9_16b);
                v6_16b = AdvSimd.Arm64.VectorTableLookupExtension(v6_16b, (v0_16b, v1_16b, v2_16b, v3_16b), v10_16b);
                v7_16b = AdvSimd.Arm64.VectorTableLookupExtension(v7_16b, (v0_16b, v1_16b, v2_16b, v3_16b), v11_16b);
                return new(v4_16b.AsUInt16(), v5_16b.AsUInt16(), v6_16b.AsUInt16(), v7_16b.AsUInt16());
            }
            else
            {
                var v12_16b = Vector128.GreaterThan((v8_16b & Vector128.Create((byte)0b10_0000)).AsSByte(), Vector128<sbyte>.Zero).AsByte();
                var v13_16b = Vector128.GreaterThan((v8_16b & Vector128.Create((byte)0b01_0000)).AsSByte(), Vector128<sbyte>.Zero).AsByte();
                var v14_16b = Vector128<byte>.Indices - v8_16b;
                v3_16b = VectorUtils.BlendVariable(v3_16b, v1_16b, v12_16b);
                v2_16b = VectorUtils.BlendVariable(v2_16b, v0_16b, v12_16b);
                v12_16b = Vector128.LessThan(v14_16b.AsSByte(), Vector128<sbyte>.Zero).AsByte();
                v1_16b = VectorUtils.BlendVariable(v1_16b, v5_16b, v12_16b);
                v14_16b &= Vector128.Create((byte)0x0f);
                v0_16b = VectorUtils.BlendVariable(v0_16b, v4_16b, v12_16b);
                v3_16b = VectorUtils.BlendVariable(v3_16b, v2_16b, v13_16b);
                v3_16b = VectorUtils.VectorTableLookup(v3_16b, v14_16b);
                v2_16b = VectorUtils.BlendVariable(v2_16b, v1_16b, v13_16b);
                v2_16b = VectorUtils.VectorTableLookup(v2_16b, v14_16b);
                v1_16b = VectorUtils.BlendVariable(v1_16b, v0_16b, v13_16b);
                v1_16b = VectorUtils.VectorTableLookup(v1_16b, v14_16b);
                v0_16b = VectorUtils.BlendVariable(v0_16b, v5_16b, v13_16b);
                v0_16b = VectorUtils.VectorTableLookup(v0_16b, v14_16b);
                v3_16b = VectorUtils.BlendVariable(v3_16b, v2_16b, v12_16b);
                v2_16b = VectorUtils.BlendVariable(v2_16b, v1_16b, v12_16b);
                v1_16b = VectorUtils.BlendVariable(v1_16b, v0_16b, v12_16b);
                v0_16b = VectorUtils.BlendVariable(v0_16b, v5_16b, v12_16b);
                return new(v0_16b.AsUInt16(), v1_16b.AsUInt16(), v2_16b.AsUInt16(), v3_16b.AsUInt16());
            }
        }
        public static void StoreUnsafe(PartialBitBoard128X4 board, ref ushort destination, nint elementOffset)
        {
            board.storage0.StoreUnsafe(ref destination, (nuint)elementOffset + 0 * (nuint)Vector128<ushort>.Count);
            board.storage1.StoreUnsafe(ref destination, (nuint)elementOffset + 1 * (nuint)Vector128<ushort>.Count);
            board.storage2.StoreUnsafe(ref destination, (nuint)elementOffset + 2 * (nuint)Vector128<ushort>.Count);
            board.storage3.StoreUnsafe(ref destination, (nuint)elementOffset + 3 * (nuint)Vector128<ushort>.Count);
        }

        public static void StoreUnsafe(PartialBitBoard128X4 board, ref ushort destination, nuint elementOffset = 0U)
        {
            board.storage0.StoreUnsafe(ref destination, elementOffset + 0 * (nuint)Vector128<ushort>.Count);
            board.storage1.StoreUnsafe(ref destination, elementOffset + 1 * (nuint)Vector128<ushort>.Count);
            board.storage2.StoreUnsafe(ref destination, elementOffset + 2 * (nuint)Vector128<ushort>.Count);
            board.storage3.StoreUnsafe(ref destination, elementOffset + 3 * (nuint)Vector128<ushort>.Count);
        }

        public static int TotalBlocks(PartialBitBoard128X4 board)
        {
            var v15_16b = Vector128.Create((byte)0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4);
            var v14_16b = Vector128.Create((byte)0x0f).AsUInt16();
            var v13_16b = Vector128.Create((ushort)0xff);
            var v0_8h = board.storage0;
            var v1_8h = board.storage1;
            var v2_8h = board.storage2;
            var v3_8h = board.storage3;
            var v4_8h = v0_8h >> 4;
            var v5_8h = v1_8h >> 4;
            var v6_8h = v2_8h >> 4;
            var v7_8h = v3_8h >> 4;
            v0_8h &= v14_16b;
            v1_8h &= v14_16b;
            v2_8h &= v14_16b;
            v3_8h &= v14_16b;
            v4_8h &= v14_16b;
            v5_8h &= v14_16b;
            v6_8h &= v14_16b;
            v7_8h &= v14_16b;
            v0_8h = VectorUtils.VectorTableLookup(v15_16b, v0_8h.AsByte()).AsUInt16();
            v1_8h = VectorUtils.VectorTableLookup(v15_16b, v1_8h.AsByte()).AsUInt16();
            v2_8h = VectorUtils.VectorTableLookup(v15_16b, v2_8h.AsByte()).AsUInt16();
            v3_8h = VectorUtils.VectorTableLookup(v15_16b, v3_8h.AsByte()).AsUInt16();
            v4_8h = VectorUtils.VectorTableLookup(v15_16b, v4_8h.AsByte()).AsUInt16();
            v5_8h = VectorUtils.VectorTableLookup(v15_16b, v5_8h.AsByte()).AsUInt16();
            v6_8h = VectorUtils.VectorTableLookup(v15_16b, v6_8h.AsByte()).AsUInt16();
            v7_8h = VectorUtils.VectorTableLookup(v15_16b, v7_8h.AsByte()).AsUInt16();
            v0_8h = (v0_8h.AsByte() + v4_8h.AsByte()).AsUInt16();
            v1_8h = (v1_8h.AsByte() + v5_8h.AsByte()).AsUInt16();
            v2_8h = (v2_8h.AsByte() + v6_8h.AsByte()).AsUInt16();
            v3_8h = (v3_8h.AsByte() + v7_8h.AsByte()).AsUInt16();
            v0_8h = (v0_8h.AsByte() + v2_8h.AsByte()).AsUInt16();
            v1_8h = (v1_8h.AsByte() + v3_8h.AsByte()).AsUInt16();
            v0_8h = (v0_8h.AsByte() + v1_8h.AsByte()).AsUInt16();
            if (AdvSimd.Arm64.IsSupported)
            {
                return AdvSimd.Arm64.AddAcrossWidening(v0_8h.AsByte()).GetElement(0);
            }
            if (AdvSimd.IsSupported)
            {
                v0_8h = AdvSimd.AddPairwiseWidening(v0_8h.AsByte());
                return Vector128.Sum(v0_8h);
            }
            if (Sse2.IsSupported)
            {
                var xmm9 = Sse2.SumAbsoluteDifferences(v0_8h.AsByte(), Vector128<byte>.Zero);
                var xmm10 = Sse2.Shuffle(xmm9.AsUInt32(), 0b11_11_01_00).AsUInt16();
                xmm9 += xmm10;
                return xmm9.GetElement(0);
            }
            // Fallback
            v1_8h = v0_8h >> 8;
            v0_8h &= v13_16b;
            v0_8h += v1_8h;
            return Vector128.Sum(v0_8h);
        }
        public PartialBitBoard128X4 WithLine(ushort line, int y)
        {
            var v15_8h = Vector128<ushort>.Indices;
            var v14_8h = Vector128.Create((ushort)8, 9, 10, 11, 12, 13, 14, 15);
            var v13_8h = Vector128.Create((ushort)16, 17, 18, 19, 20, 21, 22, 23);
            var v12_8h = Vector128.Create((ushort)24, 25, 26, 27, 28, 29, 30, 31);
            var v8_8h = Vector128.Create(line);
            var v9_8h = Vector128.Create((ushort)y);
            var v0_8h = storage0;
            var v1_8h = storage1;
            var v2_8h = storage2;
            var v3_8h = storage3;
            var v4_8h = Vector128.Equals(v15_8h, v9_8h);
            var v5_8h = Vector128.Equals(v14_8h, v9_8h);
            var v6_8h = Vector128.Equals(v13_8h, v9_8h);
            var v7_8h = Vector128.Equals(v12_8h, v9_8h);
            v0_8h = VectorUtils.BlendVariable(v0_8h, v8_8h, v4_8h);
            v1_8h = VectorUtils.BlendVariable(v1_8h, v8_8h, v5_8h);
            v2_8h = VectorUtils.BlendVariable(v2_8h, v8_8h, v6_8h);
            v3_8h = VectorUtils.BlendVariable(v3_8h, v8_8h, v7_8h);
            return new(v0_8h, v1_8h, v2_8h, v3_8h);
        }

        public static bool operator ==(PartialBitBoard128X4 left, PartialBitBoard128X4 right) => left.Equals(right);
        public static bool operator !=(PartialBitBoard128X4 left, PartialBitBoard128X4 right) => !(left == right);

        public static PartialBitBoard128X4 FillHorizontalReachable(PartialBitBoard128X4 board, PartialBitBoard128X4 reached)
        {
            var b0 = board;
            var r0 = reached;
            var r2 = r0 >> 1;
            var b2 = b0 >> 1;
            var r1 = b0 + r0;
            r2 = (r2 & b0) | r0;
            r1 = ~r1;
            r1 |= r0;
            b2 &= b0;
            r1 &= b0;
            r0 = r2 >> 2;
            b0 = b2 >> 2;
            r0 = (r0 & b2) | r2;
            b0 &= b2;
            r2 = r0 >> 4;
            b2 = b0 >> 4;
            r2 = (r2 & b0) | r0;
            b2 &= b0;
            r0 = r2 >> 8;
            r0 = (r0 & b2) | r2;
            r0 |= r1;
            return r0;
        }

        public override int GetHashCode() => HashCode.Combine(storage0, storage1, storage2, storage3);
        public override bool Equals(object? obj) => obj is PartialBitBoard128X4 board && Equals(board);
        public bool Equals(PartialBitBoard128X4 other) => storage0.Equals(other.storage0) && storage1.Equals(other.storage1) && storage2.Equals(other.storage2) && storage3.Equals(other.storage3);

        public static int TrailingZeroCount(uint mask) => BitOperations.TrailingZeroCount(mask);
        public static int PopCount(uint mask) => BitOperations.PopCount(mask);
        public static uint CompressMask(PartialBitBoard128X4 mask)
        {
            var (v0_8h, v1_8h, v2_8h, v3_8h) = (mask.storage0, mask.storage1, mask.storage2, mask.storage3);
            var m3 = v3_8h.ExtractMostSignificantBits() << 24;
            var m2 = v2_8h.ExtractMostSignificantBits() << 16;
            var m1 = v1_8h.ExtractMostSignificantBits() << 8;
            var m0 = v0_8h.ExtractMostSignificantBits();
            m2 |= m3;
            m0 |= m1;
            m0 |= m2;
            return m0;
        }
        public static PartialBitBoard128X4 ExpandMask(uint compactLineMask) => throw new NotImplementedException();
        public static PartialBitBoard128X4 GetClearableLinesVector(PartialBitBoard128X4 board) => CompareEqualPerLineVector(board, AllBitsSet);
        public static PartialBitBoard128X4 ClearClearableLines(PartialBitBoard128X4 board, ushort fill, out PartialBitBoard128X4 clearedLines)
        {
            var l = GetClearableLinesVector(board);
            clearedLines = l;
            return ClearLines(board, fill, l);
        }
        public static PartialBitBoard128X4 ClearLines(PartialBitBoard128X4 board, ushort fill, PartialBitBoard128X4 lines) => throw new NotImplementedException();
        public static bool IsSetAt(PartialBitBoard128X4 mask, byte index) => ((CompressMask(mask) >> index) & 1) > 0;
        public static PartialBitBoard128X4 CreateMaskFromBoard(PartialBitBoard128X4 board) => board;
        public static PartialBitBoard128X4 MaskUnaryNegation(PartialBitBoard128X4 mask) => ~mask;
        public static PartialBitBoard128X4 MaskAnd(PartialBitBoard128X4 left, PartialBitBoard128X4 right) => left & right;
        public static PartialBitBoard128X4 MaskOr(PartialBitBoard128X4 left, PartialBitBoard128X4 right) => left | right;
        public static PartialBitBoard128X4 MaskXor(PartialBitBoard128X4 left, PartialBitBoard128X4 right) => left ^ right;
        public static ulong CalculateHash(PartialBitBoard128X4 board, ulong key = 0) => throw new NotImplementedException();

        [SkipLocalsInit]
        private string GetDebuggerDisplay()
        {
            Span<ushort> lines = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
            ref var x8 = ref MemoryMarshal.GetReference(lines);
            storage0.StoreUnsafe(ref x8, 0 * (nuint)Vector128<ushort>.Count);
            storage1.StoreUnsafe(ref x8, 1 * (nuint)Vector128<ushort>.Count);
            storage2.StoreUnsafe(ref x8, 2 * (nuint)Vector128<ushort>.Count);
            storage3.StoreUnsafe(ref x8, 3 * (nuint)Vector128<ushort>.Count);
            var sb = new StringBuilder(Environment.NewLine);
            sb.AppendSquareDisplay(lines);
            return sb.ToString();
        }

        public override string ToString() => GetDebuggerDisplay();
    }
}
