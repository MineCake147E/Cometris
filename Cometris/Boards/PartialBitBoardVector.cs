using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Versioning;

using Cometris.Utils;

#pragma warning disable SYSLIB5003 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
namespace Cometris.Boards
{
    /// <summary>
    /// Bit board that records only the upper nonzero lines as much as <see cref="Vector{T}"/> can hold, out of 40 lines.<br/>
    /// This structure is for mainstream hardware-accelerated board operations, mainly for Arm64 target with SVE available.
    /// </summary>
    [RequiresPreviewFeatures("Sve is in preview")]
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct PartialBitBoardVector : IMaskableBitBoard<PartialBitBoardVector, ushort, Vector<ushort>>
    {
        private readonly Vector<ushort> storage;
        public PartialBitBoardVector()
        {
        }

        public PartialBitBoardVector(ushort fill)
        {
            storage = new(fill);
        }

        public PartialBitBoardVector(Vector<ushort> vector)
        {
            storage = vector;
        }

        public ushort this[int y] => storage[y];

        public static Vector<ushort> ZeroMask => Sve.CreateFalseMaskUInt16();
        public static Vector<ushort> AllBitsSetMask => Sve.CreateTrueMaskUInt16(SveMaskPattern.All);
        public static bool IsBitwiseOperationHardwareAccelerated => Sve.IsSupported;
        public static bool IsHorizontalConstantShiftHardwareAccelerated => Sve.IsSupported;
        public static bool IsHorizontalVariableShiftSupported => Sve.IsSupported;
        public static bool IsSupported => Vector<ushort>.Count == 32;
        public static bool IsVerticalShiftSupported => Sve.IsSupported;
        public static int MaxEnregisteredLocals => 32;
        public static ushort EmptyLine { get; }
        public static PartialBitBoardVector Empty => new(FullBitBoard.EmptyRow);
        static int IBitBoard<PartialBitBoardVector, ushort>.BitPositionXLeftmost => (16 - PartialBitBoard256X2.EffectiveWidth) / 2 + PartialBitBoard256X2.EffectiveWidth;
        public static int Height => Vector<ushort>.Count;
        public static int RightmostPaddingWidth => 3;
        public static int StorableWidth => 8 * sizeof(ushort);

        public static int EffectiveWidth => PartialBitBoard256X2.EffectiveWidth;
        public static byte HeightFieldSize => (byte)(int.LeadingZeroCount(0) - int.LeadingZeroCount(Vector<ushort>.Count));

        public static PartialBitBoardVector BlocksPerLine(PartialBitBoardVector board) => new(Sve.PopCount(board.storage));
        public static PartialBitBoardVector ClearClearableLines(PartialBitBoardVector board, ushort fill, out Vector<ushort> clearedLines) => throw new NotImplementedException();
        public static PartialBitBoardVector ClearClearableLines(PartialBitBoardVector board, ushort fill) => throw new NotImplementedException();
        public static PartialBitBoardVector ClearLines(PartialBitBoardVector board, ushort fill, Vector<ushort> lines) => throw new NotImplementedException();
        public static Vector<ushort> CompareEqualPerLineVector(PartialBitBoardVector left, PartialBitBoardVector right) => throw new NotImplementedException();
        public static Vector<ushort> CreateMaskFromBoard(PartialBitBoardVector board) => throw new NotImplementedException();
        public static PartialBitBoardVector CreateSingleBlock(int x, int y) => throw new NotImplementedException();
        public static ushort CreateSingleBlockLine(int x) => throw new NotImplementedException();
        public static PartialBitBoardVector CreateSingleLine(ushort line, int y) => throw new NotImplementedException();
        public static PartialBitBoardVector CreateThreeAdjacentLines(int y, ushort lineLower, ushort lineMiddle, ushort lineUpper) => throw new NotImplementedException();
        public static PartialBitBoardVector CreateTwoLines(int y0, int y1, ushort line0, ushort line1) => throw new NotImplementedException();
        public static PartialBitBoardVector CreateVerticalI4Piece(int x, int y) => throw new NotImplementedException();
        public static PartialBitBoardVector FillDropReachable(PartialBitBoardVector board, PartialBitBoardVector reached) => throw new NotImplementedException();
        public static PartialBitBoardVector FillHorizontalReachable(PartialBitBoardVector board, PartialBitBoardVector reached) => throw new NotImplementedException();
        public static PartialBitBoardVector FromBoard(ReadOnlySpan<ushort> board, ushort fill) => throw new NotImplementedException();
        public static bool GetBlockAt(ushort line, int x) => throw new NotImplementedException();
        public static bool GetBlockAtFullRange(ushort line, int x) => throw new NotImplementedException();
        public static Vector<ushort> GetClearableLinesVector(PartialBitBoardVector board) => throw new NotImplementedException();
        public static bool IsSetAt(Vector<ushort> mask, byte index) => throw new NotImplementedException();
        public static PartialBitBoardVector LineSelect(Vector<ushort> mask, PartialBitBoardVector left, PartialBitBoardVector right) => throw new NotImplementedException();
        public static PartialBitBoardVector LoadUnsafe(ref ushort source, nint elementOffset) => throw new NotImplementedException();
        public static PartialBitBoardVector LoadUnsafe(ref ushort source, nuint elementOffset = 0U) => throw new NotImplementedException();
        public static int LocateAllBlocks(PartialBitBoardVector board, IBufferWriter<CompressedPositionsTuple> writer) => throw new NotImplementedException();
        public static Vector<ushort> MaskAnd(Vector<ushort> left, Vector<ushort> right) => throw new NotImplementedException();
        public static Vector<ushort> MaskOr(Vector<ushort> left, Vector<ushort> right) => throw new NotImplementedException();
        public static Vector<ushort> MaskUnaryNegation(Vector<ushort> mask) => throw new NotImplementedException();
        public static Vector<ushort> MaskXor(Vector<ushort> left, Vector<ushort> right) => throw new NotImplementedException();
        public static PartialBitBoardVector ShiftDownOneLine(PartialBitBoardVector board, ushort upperFeedValue) => throw new NotImplementedException();
        public static PartialBitBoardVector ShiftDownOneLine(PartialBitBoardVector board, PartialBitBoardVector upperFeedBoard) => throw new NotImplementedException();
        public static PartialBitBoardVector ShiftUpOneLine(PartialBitBoardVector board, ushort lowerFeedValue) => throw new NotImplementedException();
        public static PartialBitBoardVector ShiftUpOneLine(PartialBitBoardVector board, PartialBitBoardVector lowerFeedBoard) => throw new NotImplementedException();
        public static PartialBitBoardVector ShiftUpVariableLines(PartialBitBoardVector board, int count, PartialBitBoardVector lowerFeedBoard) => throw new NotImplementedException();
        public static void StoreUnsafe(PartialBitBoardVector board, ref ushort destination, nint elementOffset) => throw new NotImplementedException();
        public static void StoreUnsafe(PartialBitBoardVector board, ref ushort destination, nuint elementOffset = 0U) => throw new NotImplementedException();
        public static int TotalBlocks(PartialBitBoardVector board) => throw new NotImplementedException();
        public bool Equals(PartialBitBoardVector other) => throw new NotImplementedException();
        public PartialBitBoardVector WithLine(ushort line, int y) => throw new NotImplementedException();
        public static ulong CalculateHash(PartialBitBoardVector board) => throw new NotImplementedException();
        public static int GetBlockHeight(PartialBitBoardVector board) => throw new NotImplementedException();

        public static PartialBitBoardVector operator ~(PartialBitBoardVector value) => throw new NotImplementedException();
        public static PartialBitBoardVector operator &(PartialBitBoardVector left, PartialBitBoardVector right) => throw new NotImplementedException();
        public static PartialBitBoardVector operator |(PartialBitBoardVector left, PartialBitBoardVector right) => throw new NotImplementedException();
        public static PartialBitBoardVector operator ^(PartialBitBoardVector left, PartialBitBoardVector right) => throw new NotImplementedException();
        public static PartialBitBoardVector operator <<(PartialBitBoardVector left, [ConstantExpected] int right) => throw new NotImplementedException();
        public static PartialBitBoardVector operator >>(PartialBitBoardVector left, [ConstantExpected] int right) => throw new NotImplementedException();
        public static bool operator ==(PartialBitBoardVector left, PartialBitBoardVector right) => throw new NotImplementedException();
        public static bool operator !=(PartialBitBoardVector left, PartialBitBoardVector right) => throw new NotImplementedException();
    }
}

#pragma warning restore SYSLIB5003 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.