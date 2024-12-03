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
using Cometris.Utils.Vector;

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

        private readonly QuadVector128<ushort> storage;

        public PartialBitBoard128X4(QuadVector128<ushort> storage)
        {
            this.storage = storage;
        }

        public PartialBitBoard128X4(Vector128<ushort> storage0, Vector128<ushort> storage1, Vector128<ushort> storage2, Vector128<ushort> storage3)
        {
            storage = new(storage0, storage1, storage2, storage3);
        }

        public PartialBitBoard128X4(ushort fill)
        {
            storage = QuadVector128.Create(fill);
        }

        public ushort this[int y]
        {
            get
            {
                if (AdvSimd.IsSupported)
                {
                    var v0_4h = Vector64.Create((byte)(y * 2));
                    return AdvSimd.VectorTableLookup((Storage0.AsByte(), Storage1.AsByte(), Storage2.AsByte(), Storage3.AsByte()), v0_4h).AsUInt16().GetElement(0);
                }
                // Fallback
                var s = (y >> 3) switch
                {
                    0 => Storage0,
                    1 => Storage1,
                    2 => Storage2,
                    _ => Storage3,
                };
                y &= 7;
                return s.GetElementVariable(y);
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

        [SuppressMessage("Major Code Smell", "S1168:Empty arrays and collections should be returned instead of null", Justification = "Poor performance if applied")]
        public static PartialBitBoard128X4 Zero => default;
        public static PartialBitBoard128X4 AllBitsSetMask => new(ushort.MaxValue);

        public static PartialBitBoard128X4 AllBitsSet => new(ushort.MaxValue);

        public Vector128<ushort> Storage0 => storage.V0;

        public Vector128<ushort> Storage1 => storage.V1;

        public Vector128<ushort> Storage2 => storage.V2;

        public Vector128<ushort> Storage3 => storage.V3;

        public void Deconstruct(out Vector128<ushort> storage0, out Vector128<ushort> storage1, out Vector128<ushort> storage2, out Vector128<ushort> storage3)
            => (storage0, storage1, storage2, storage3) = (Storage0, Storage1, Storage2, Storage3);

        public static PartialBitBoard128X4 ClearClearableLines(PartialBitBoard128X4 board, ushort fill) => ClearClearableLines(board, fill, out _);
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
        public static PartialBitBoard128X4 FillDropReachable(PartialBitBoard128X4 board, PartialBitBoard128X4 reached)
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
            r1 = ShiftDown16Lines(r0, 0);
            r1 = Or1And02(r1, r0, b0);
            return r1;
        }
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
        public static int LocateAllBlocks(PartialBitBoard128X4 board, IBufferWriter<CompressedPointList> writer) => throw new NotImplementedException();
        public static PartialBitBoard128X4 ShiftDownOneLine(PartialBitBoard128X4 board, ushort upperFeedValue)
        {
            var (v0_16b, v1_16b, v2_16b, v3_16b) = (board.Storage0.AsByte(), board.Storage1.AsByte(), board.Storage2.AsByte(), board.Storage3.AsByte());
            var v7_16b = Vector128.Create(upperFeedValue).AsByte();
            if (AdvSimd.IsSupported)
            {
                v0_16b = AdvSimd.ExtractVector128(v1_16b, v0_16b, 2);
                v1_16b = AdvSimd.ExtractVector128(v2_16b, v1_16b, 2);
                v2_16b = AdvSimd.ExtractVector128(v3_16b, v2_16b, 2);
                v3_16b = AdvSimd.ExtractVector128(v7_16b, v3_16b, 2);
                return new(v0_16b.AsUInt16(), v1_16b.AsUInt16(), v2_16b.AsUInt16(), v3_16b.AsUInt16());
            }
            else if (Sse41.IsSupported)
            {
                v0_16b = Ssse3.AlignRight(v1_16b, v0_16b, 2);
                v1_16b = Ssse3.AlignRight(v2_16b, v1_16b, 2);
                v2_16b = Ssse3.AlignRight(v3_16b, v2_16b, 2);
                v3_16b = Ssse3.AlignRight(v7_16b, v3_16b, 2);
                return new(v0_16b.AsUInt16(), v1_16b.AsUInt16(), v2_16b.AsUInt16(), v3_16b.AsUInt16());
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
            var (v0_16b, v1_16b, v2_16b, v3_16b) = (board.Storage0.AsByte(), board.Storage1.AsByte(), board.Storage2.AsByte(), board.Storage3.AsByte());
            var v7_16b = upperFeedBoard.Storage0.AsByte();
            if (AdvSimd.IsSupported)
            {
                v0_16b = AdvSimd.ExtractVector128(v1_16b, v0_16b, 2);
                v1_16b = AdvSimd.ExtractVector128(v2_16b, v1_16b, 2);
                v2_16b = AdvSimd.ExtractVector128(v3_16b, v2_16b, 2);
                v3_16b = AdvSimd.ExtractVector128(v7_16b, v3_16b, 2);
                return new(v0_16b.AsUInt16(), v1_16b.AsUInt16(), v2_16b.AsUInt16(), v3_16b.AsUInt16());
            }
            else if (Sse41.IsSupported)
            {
                v0_16b = Ssse3.AlignRight(v1_16b, v0_16b, 2);
                v1_16b = Ssse3.AlignRight(v2_16b, v1_16b, 2);
                v2_16b = Ssse3.AlignRight(v3_16b, v2_16b, 2);
                v3_16b = Ssse3.AlignRight(v7_16b, v3_16b, 2);
                return new(v0_16b.AsUInt16(), v1_16b.AsUInt16(), v2_16b.AsUInt16(), v3_16b.AsUInt16());
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

        public static PartialBitBoard128X4 ShiftDownTwoLines(PartialBitBoard128X4 board, ushort upperFeedValue)
        {
            var (v0_16b, v1_16b, v2_16b, v3_16b) = (board.Storage0.AsByte(), board.Storage1.AsByte(), board.Storage2.AsByte(), board.Storage3.AsByte());
            var v7_16b = Vector128.Create(upperFeedValue).AsByte();
            if (AdvSimd.IsSupported)
            {
                v0_16b = AdvSimd.ExtractVector128(v1_16b, v0_16b, 4);
                v1_16b = AdvSimd.ExtractVector128(v2_16b, v1_16b, 4);
                v2_16b = AdvSimd.ExtractVector128(v3_16b, v2_16b, 4);
                v3_16b = AdvSimd.ExtractVector128(v7_16b, v3_16b, 4);
                return new(v0_16b.AsUInt16(), v1_16b.AsUInt16(), v2_16b.AsUInt16(), v3_16b.AsUInt16());
            }
            else if (Sse41.IsSupported)
            {
                v0_16b = Ssse3.AlignRight(v1_16b, v0_16b, 4);
                v1_16b = Ssse3.AlignRight(v2_16b, v1_16b, 4);
                v2_16b = Ssse3.AlignRight(v3_16b, v2_16b, 4);
                v3_16b = Ssse3.AlignRight(v7_16b, v3_16b, 4);
                return new(v0_16b.AsUInt16(), v1_16b.AsUInt16(), v2_16b.AsUInt16(), v3_16b.AsUInt16());
            }
            else
            {
                var v8_16b = Vector128.Create((byte)4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 0, 1, 2, 3);
                var v12_16b = Vector128.Create(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255);
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

        public static PartialBitBoard128X4 ShiftDownFourLines(PartialBitBoard128X4 board, ushort upperFeedValue)
        {
            var (v0_16b, v1_16b, v2_16b, v3_16b) = (board.Storage0.AsByte(), board.Storage1.AsByte(), board.Storage2.AsByte(), board.Storage3.AsByte());
            var v7_16b = Vector128.Create(upperFeedValue).AsByte();
            const int BytesToShift = 8;
            if (AdvSimd.IsSupported)
            {
                v0_16b = AdvSimd.ExtractVector128(v1_16b, v0_16b, BytesToShift);
                v1_16b = AdvSimd.ExtractVector128(v2_16b, v1_16b, BytesToShift);
                v2_16b = AdvSimd.ExtractVector128(v3_16b, v2_16b, BytesToShift);
                v3_16b = AdvSimd.ExtractVector128(v7_16b, v3_16b, BytesToShift);
                return new(v0_16b.AsUInt16(), v1_16b.AsUInt16(), v2_16b.AsUInt16(), v3_16b.AsUInt16());
            }
            else if (Sse41.IsSupported)
            {
                v0_16b = Ssse3.AlignRight(v1_16b, v0_16b, BytesToShift);
                v1_16b = Ssse3.AlignRight(v2_16b, v1_16b, BytesToShift);
                v2_16b = Ssse3.AlignRight(v3_16b, v2_16b, BytesToShift);
                v3_16b = Ssse3.AlignRight(v7_16b, v3_16b, BytesToShift);
                return new(v0_16b.AsUInt16(), v1_16b.AsUInt16(), v2_16b.AsUInt16(), v3_16b.AsUInt16());
            }
            else
            {
                var v8_16b = Vector128.Create((byte)8, 9, 10, 11, 12, 13, 14, 15, 0, 1, 2, 3, 4, 5, 6, 7);
                var v12_16b = Vector128.Create(0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255);
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

        public static PartialBitBoard128X4 ShiftDownEightLines(PartialBitBoard128X4 board, ushort upperFeedValue)
        {
            var (v0_16b, v1_16b, v2_16b) = (board.Storage1.AsByte(), board.Storage2.AsByte(), board.Storage3.AsByte());
            var v3_16b = Vector128.Create(upperFeedValue).AsByte();
            return new(v0_16b.AsUInt16(), v1_16b.AsUInt16(), v2_16b.AsUInt16(), v3_16b.AsUInt16());
        }

        public static PartialBitBoard128X4 ShiftDown16Lines(PartialBitBoard128X4 board, ushort upperFeedValue)
        {
            var (v0_16b, v1_16b) = (board.Storage2.AsByte(), board.Storage3.AsByte());
            var v3_16b = Vector128.Create(upperFeedValue).AsByte();
            return new(v0_16b.AsUInt16(), v1_16b.AsUInt16(), v3_16b.AsUInt16(), v3_16b.AsUInt16());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static PartialBitBoard128X4 ShiftDownVariableLines(PartialBitBoard128X4 board, int count, ushort upperFeedValue)
        {
            count &= 31;
            var v8_16b = Vector128.Create((byte)(count * 2));
            var (v0_16b, v1_16b, v2_16b, v3_16b) = (board.Storage0.AsByte(), board.Storage1.AsByte(), board.Storage2.AsByte(), board.Storage3.AsByte());
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
            var (v0_16b, v1_16b, v2_16b, v3_16b) = (board.Storage0.AsByte(), board.Storage1.AsByte(), board.Storage2.AsByte(), board.Storage3.AsByte());
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
            var (v0_16b, v1_16b, v2_16b, v3_16b) = (board.Storage0.AsByte(), board.Storage1.AsByte(), board.Storage2.AsByte(), board.Storage3.AsByte());
            var v4_16b = lowerFeedBoard.Storage0.AsByte();
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
            var (v0_16b, v1_16b, v2_16b, v3_16b) = (board.Storage0.AsByte(), board.Storage1.AsByte(), board.Storage2.AsByte(), board.Storage3.AsByte());
            var (v4_16b, v5_16b, v6_16b, v7_16b) = (lowerFeedBoard.Storage0.AsByte(), lowerFeedBoard.Storage1.AsByte(), lowerFeedBoard.Storage2.AsByte(), lowerFeedBoard.Storage3.AsByte());
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
            board.Storage0.StoreUnsafe(ref destination, (nuint)elementOffset + 0 * (nuint)Vector128<ushort>.Count);
            board.Storage1.StoreUnsafe(ref destination, (nuint)elementOffset + 1 * (nuint)Vector128<ushort>.Count);
            board.Storage2.StoreUnsafe(ref destination, (nuint)elementOffset + 2 * (nuint)Vector128<ushort>.Count);
            board.Storage3.StoreUnsafe(ref destination, (nuint)elementOffset + 3 * (nuint)Vector128<ushort>.Count);
        }

        public static void StoreUnsafe(PartialBitBoard128X4 board, ref ushort destination, nuint elementOffset = 0U)
        {
            board.Storage0.StoreUnsafe(ref destination, elementOffset + 0 * (nuint)Vector128<ushort>.Count);
            board.Storage1.StoreUnsafe(ref destination, elementOffset + 1 * (nuint)Vector128<ushort>.Count);
            board.Storage2.StoreUnsafe(ref destination, elementOffset + 2 * (nuint)Vector128<ushort>.Count);
            board.Storage3.StoreUnsafe(ref destination, elementOffset + 3 * (nuint)Vector128<ushort>.Count);
        }

        public static int TotalBlocks(PartialBitBoard128X4 board)
        {
            var v15_16b = Vector128.Create((byte)0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4);
            var v14_16b = Vector128.Create((byte)0x0f).AsUInt16();
            var v13_16b = Vector128.Create((ushort)0xff);
            var v0_8h = board.Storage0;
            var v1_8h = board.Storage1;
            var v2_8h = board.Storage2;
            var v3_8h = board.Storage3;
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
            var v0_8h = Storage0;
            var v1_8h = Storage1;
            var v2_8h = Storage2;
            var v3_8h = Storage3;
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

        public override int GetHashCode() => HashCode.Combine(Storage0, Storage1, Storage2, Storage3);
        public override bool Equals(object? obj) => obj is PartialBitBoard128X4 board && Equals(board);
        public bool Equals(PartialBitBoard128X4 other) => Storage0.Equals(other.Storage0) && Storage1.Equals(other.Storage1) && Storage2.Equals(other.Storage2) && Storage3.Equals(other.Storage3);

        public static int TrailingZeroCount(uint mask) => BitOperations.TrailingZeroCount(mask);
        public static int PopCount(uint mask) => BitOperations.PopCount(mask);
        public static uint CompressMask(PartialBitBoard128X4 mask)
        {
            var (v0_8h, v1_8h, v2_8h, v3_8h) = (mask.Storage0, mask.Storage1, mask.Storage2, mask.Storage3);
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

        public static PartialBitBoard128X4 ClearLines(PartialBitBoard128X4 board, ushort fill, PartialBitBoard128X4 lines)
        {
            //var v0_32h = board.AsQuadVector128();
            //var v1_32h = lines.AsQuadVector128();
            //var v2_32h = v1_32h & Vector128.Create((ushort)2);
            //var v3_32h = QuadVector128.ShiftRightLogical(v2_32h.AsUInt32(), 16).AsUInt16();
            //var v4_64b = QuadVector128.ShuffleBytesPerLane(v2_32h.AsByte(), Vector128.Create((byte)0, 0, 0, 0, 4, 4, 4, 4, 8, 8, 8, 8, 12, 12, 12, 12)) + Vector128<byte>.Indices;
            //var v5_32h = QuadVector128.CompareLessThan(v4_64b.AsSByte(), Vector128.Create(4, 4, 4, 4, 8, 8, 8, 8, 12, 12, 12, 12, 16, 16, 16, 16)).AsUInt16();
            //v2_32h += v3_32h;
            //v0_32h = QuadVector128.ShuffleBytesPerLane(v0_32h.AsByte(), v4_64b).AsUInt16() & v5_32h;
            //v3_32h = QuadVector128.ShiftRightLogical(v2_32h.AsUInt64(), 32).AsUInt16();
            //v4_64b = QuadVector128.ShuffleBytesPerLane(v2_32h.AsByte(), Vector128.Create((byte)0, 0, 0, 0, 0, 0, 0, 0, 8, 8, 8, 8, 8, 8, 8, 8)) + Vector128<byte>.Indices;
            //v5_32h = QuadVector128.CompareLessThan(v4_64b.AsSByte(), Vector128.Create(8, 8, 8, 8, 8, 8, 8, 8, 16, 16, 16, 16, 16, 16, 16, 16)).AsUInt16();
            //v2_32h += v3_32h;
            //v0_32h = QuadVector128.ShuffleBytesPerLane(v0_32h.AsByte(), v4_64b).AsUInt16() & v5_32h;
            throw new NotImplementedException();
        }

        public static bool IsSetAt(PartialBitBoard128X4 mask, byte index) => ((CompressMask(mask) >> index) & 1) > 0;
        public static PartialBitBoard128X4 CreateMaskFromBoard(PartialBitBoard128X4 board) => board;
        public static PartialBitBoard128X4 MaskUnaryNegation(PartialBitBoard128X4 mask) => ~mask;
        public static PartialBitBoard128X4 MaskAnd(PartialBitBoard128X4 left, PartialBitBoard128X4 right) => left & right;
        public static PartialBitBoard128X4 MaskOr(PartialBitBoard128X4 left, PartialBitBoard128X4 right) => left | right;
        public static PartialBitBoard128X4 MaskXor(PartialBitBoard128X4 left, PartialBitBoard128X4 right) => left ^ right;

        #region Operator Supplement
        public static PartialBitBoard128X4 Or1And02(PartialBitBoard128X4 b0, PartialBitBoard128X4 b1, PartialBitBoard128X4 b2) => b1 | (b0 & b2);
        #endregion

        public static ulong CalculateHash(PartialBitBoard128X4 board, ulong key = 0) => throw new NotImplementedException();

        public QuadVector128<ushort> AsQuadVector128() => storage;

        [SkipLocalsInit]
        private string GetDebuggerDisplay()
        {
            Span<ushort> lines = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
            ref var x8 = ref MemoryMarshal.GetReference(lines);
            Storage0.StoreUnsafe(ref x8, 0 * (nuint)Vector128<ushort>.Count);
            Storage1.StoreUnsafe(ref x8, 1 * (nuint)Vector128<ushort>.Count);
            Storage2.StoreUnsafe(ref x8, 2 * (nuint)Vector128<ushort>.Count);
            Storage3.StoreUnsafe(ref x8, 3 * (nuint)Vector128<ushort>.Count);
            var sb = new StringBuilder(Environment.NewLine);
            sb.AppendSquareDisplay(lines);
            return sb.ToString();
        }

        public override string ToString() => GetDebuggerDisplay();
    }
}
