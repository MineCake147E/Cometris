using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cometris.Boards
{
    public partial interface IBitBoard<TSelf, TLineElement> : IEquatable<TSelf>, IEqualityOperators<TSelf, TSelf, bool>
        where TSelf : unmanaged, IBitBoard<TSelf, TLineElement>
        where TLineElement : unmanaged, IBinaryNumber<TLineElement>, IUnsignedNumber<TLineElement>
    {
        #region Static Properties
        #region Hardware Capability
        static abstract bool IsBitwiseOperationHardwareAccelerated { get; }

        static abstract bool IsHorizontalConstantShiftHardwareAccelerated { get; }

        static abstract bool IsHorizontalVariableShiftSupported { get; }

        static abstract bool IsSupported { get; }
        static abstract bool IsVerticalShiftSupported { get; }

        static abstract int MaxEnregisteredLocals { get; }

        #endregion
        #region Line Constants
        static abstract TLineElement EmptyLine { get; }

        static virtual TLineElement FullLine => TLineElement.AllBitsSet;

        static virtual TLineElement InvertedEmptyLine => ~TSelf.EmptyLine;

        static virtual TLineElement ZeroLine => TLineElement.Zero;

        #endregion
        #region Board Constants
        static virtual TSelf AllBitsSet => ~TSelf.Zero;
        static abstract TSelf Empty { get; }

        static virtual TSelf InvertedEmpty => ~TSelf.Empty;

        static virtual TSelf Zero => default;

        #endregion
        /// <summary>
        /// The shift value that <typeparamref name="TLineElement"/> &gt;&gt; (<see cref="BitPositionXLeftmost"/> - 1) &amp; 1 represents the value of the leftmost column.
        /// </summary>
        static abstract int BitPositionXLeftmost { get; }

        static virtual int BitPositionXRightmost => TSelf.BitPositionXLeftmost - TSelf.EffectiveWidth;

        static virtual int EffectiveWidth => 10;

        static abstract int Height { get; }

        static virtual int LeftmostPaddingWidth => TSelf.RightmostPaddingWidth;

        static abstract int RightmostPaddingWidth { get; }

        static abstract int StorableWidth { get; }
        #endregion
        TLineElement this[int y] { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract TSelf ClearClearableLines(TSelf board, TLineElement fill);

        #region Board Construction
        TSelf WithLine(TLineElement line, int y);
        static abstract TSelf FromBoard(ReadOnlySpan<TLineElement> board, TLineElement fill);

        static virtual TSelf CreateFilled(TLineElement fill) => TSelf.FromBoard(default, fill);

        static abstract TSelf CreateSingleBlock(int x, int y);

        static abstract TSelf CreateSingleLine(TLineElement line, int y);

        static abstract TSelf CreateTwoLines(int y0, int y1, TLineElement line0, TLineElement line1);

        static virtual TSelf CreateTwoAdjacentLinesUp(int y, TLineElement lineMiddle, TLineElement lineUpper) => TSelf.CreateThreeAdjacentLines(y, TSelf.ZeroLine, lineMiddle, lineUpper);

        static virtual TSelf CreateTwoAdjacentLinesDown(int y, TLineElement lineLower, TLineElement lineMiddle) => TSelf.CreateThreeAdjacentLines(y, lineLower, lineMiddle, TSelf.ZeroLine);

        /// <summary>
        /// Create a new board with 3 specified consecutive lines.
        /// Suitable for creation of T, J, L, S, and Z pieces.
        /// </summary>
        /// <param name="y">The y coordinate of <paramref name="lineMiddle"/>.</param>
        /// <param name="lineLower">The line to put at the y coordinate <paramref name="y"/> - 1.</param>
        /// <param name="lineMiddle">The line to put at the y coordinate <paramref name="y"/>.</param>
        /// <param name="lineUpper">The line to put at the y coordinate <paramref name="y"/> + 1.</param>
        /// <returns>A new board with 3 specified consecutive lines.</returns>
        static abstract TSelf CreateThreeAdjacentLines(int y, TLineElement lineLower, TLineElement lineMiddle, TLineElement lineUpper);

        /// <summary>
        /// Create a new board with a vertical I piece facing right(the vertical I3 piece with extra block at the bottom) at specified position.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        static abstract TSelf CreateVerticalI4Piece(int x, int y);

        #endregion

        #region Line Construction

        static abstract TLineElement CreateSingleBlockLine(int x);
        #endregion

        #region Board Load/Store

        #region Load
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract TSelf LoadUnsafe(ref TLineElement source, nint elementOffset);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract TSelf LoadUnsafe(ref TLineElement source, nuint elementOffset = 0);
        #endregion

        #region Store
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual void Store(TSelf board, Span<TLineElement> destination)
        {
            if (!TSelf.TryStore(board, destination)) throw new InvalidOperationException($"The {nameof(destination)} must be longer than {TSelf.Height - 1}!");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual bool TryStore(TSelf board, Span<TLineElement> destination)
        {
            if (destination.Length < TSelf.Height) return false;
            TSelf.StoreUnsafe(board, ref MemoryMarshal.GetReference(destination));
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract void StoreUnsafe(TSelf board, ref TLineElement destination, nint elementOffset);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract void StoreUnsafe(TSelf board, ref TLineElement destination, nuint elementOffset = 0);
        #endregion

        #endregion

        #region ConvertMobility
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual (TSelf upper, TSelf right, TSelf lower, TSelf left) ConvertHorizontalSymmetricToAsymmetricMobility((TSelf upper, TSelf right) boards)
            => (boards.upper, boards.right, boards.upper >> 1, TSelf.ShiftDownOneLine(boards.right, TSelf.ZeroLine));

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual (TSelf upper, TSelf right) MergeAsymmetricToHorizontalSymmetricMobility((TSelf upper, TSelf right, TSelf lower, TSelf left) boards)
        {
            var upper = boards.upper;
            var right = boards.right;
            upper |= boards.lower << 1;
            right |= TSelf.ShiftUpOneLine(boards.left, TSelf.ZeroLine);
            return (upper, right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual (TSelf upper, TSelf right, TSelf lower, TSelf left) ConvertOPieceToAsymmetricMobility(TSelf board)
        {
            var rightBoard = TSelf.ShiftUpOneLine(board, TSelf.ZeroLine);
            return (board, rightBoard, rightBoard >> 1, board >> 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual TSelf MergeAsymmetricToOPieceMobility((TSelf upper, TSelf right, TSelf lower, TSelf left) boards)
        {
            var right = boards.right;
            right |= boards.lower << 1;
            var board = boards.upper;
            right = TSelf.ShiftDownOneLine(right, TSelf.ZeroLine);
            board |= boards.left << 1;
            board |= right;
            return board;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual (TSelf upper, TSelf right, TSelf lower, TSelf left) ConvertVerticalSymmetricToAsymmetricMobility((TSelf upper, TSelf right) boards)
            => (boards.upper, boards.right, TSelf.ShiftUpOneLine(boards.upper, TSelf.ZeroLine), boards.right >> 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual (TSelf upper, TSelf right) MergeAsymmetricToVerticalSymmetricMobility((TSelf upper, TSelf right, TSelf lower, TSelf left) boards)
        {
            var upper = boards.upper;
            var right = boards.right;
            upper |= TSelf.ShiftDownOneLine(boards.lower, TSelf.ZeroLine);
            right |= boards.left << 1;
            return (upper, right);
        }
        #endregion

        #region Reachability
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract TSelf FillDropReachable(TSelf board, TSelf reached);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual (TSelf upper, TSelf right, TSelf lower, TSelf left) FillDropReachable4Sets((TSelf upper, TSelf right, TSelf lower, TSelf left) board, (TSelf upper, TSelf right, TSelf lower, TSelf left) reached)
        {
            (var upperBoard, var rightBoard, var lowerBoard, var leftBoard) = (board.upper, board.right, board.lower, board.left);
            (var upperReached, var rightReached, var lowerReached, var leftReached) = (reached.upper, reached.right, reached.lower, reached.left);
            var upper = TSelf.FillDropReachable(upperBoard, upperReached);
            var right = TSelf.FillDropReachable(rightBoard, rightReached);
            var lower = TSelf.FillDropReachable(lowerBoard, lowerReached);
            var left = TSelf.FillDropReachable(leftBoard, leftReached);
            return (upper, right, lower, left);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual (TSelf upper, TSelf right, TSelf lower, TSelf left) FillDropReachable4Sets(TSelf upperBoard, TSelf rightBoard, TSelf lowerBoard, TSelf leftBoard, TSelf upperReached, TSelf rightReached, TSelf lowerReached, TSelf leftReached)
            => (
            TSelf.FillDropReachable(upperBoard, upperReached),
            TSelf.FillDropReachable(rightBoard, rightReached),
            TSelf.FillDropReachable(lowerBoard, lowerReached),
            TSelf.FillDropReachable(leftBoard, leftReached));

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract TSelf FillHorizontalReachable(TSelf board, TSelf reached);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual (TSelf upper, TSelf right, TSelf lower, TSelf left) FillHorizontalReachable4Sets((TSelf upper, TSelf right, TSelf lower, TSelf left) board, (TSelf upper, TSelf right, TSelf lower, TSelf left) reached)
            => (
            TSelf.FillHorizontalReachable(board.upper, reached.upper),
            TSelf.FillHorizontalReachable(board.right, reached.right),
            TSelf.FillHorizontalReachable(board.lower, reached.lower),
            TSelf.FillHorizontalReachable(board.left, reached.left));

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual (TSelf upper, TSelf right, TSelf lower, TSelf left) FillHorizontalReachable4Sets(TSelf upperBoard, TSelf rightBoard, TSelf lowerBoard, TSelf leftBoard, TSelf upperReached, TSelf rightReached, TSelf lowerReached, TSelf leftReached)
            => (
            TSelf.FillHorizontalReachable(upperBoard, upperReached),
            TSelf.FillHorizontalReachable(rightBoard, rightReached),
            TSelf.FillHorizontalReachable(lowerBoard, lowerReached),
            TSelf.FillHorizontalReachable(leftBoard, leftReached));

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual TSelf FindLockablePositions(TSelf board, TSelf lowerFeedBoard) => TSelf.AndNot(TSelf.ShiftUpOneLine(board, lowerFeedBoard), board);

        static virtual TSelf FindLockablePositions(TSelf board) => TSelf.FindLockablePositions(board, TSelf.Zero);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual (TSelf upper, TSelf right, TSelf lower, TSelf left) FindLockablePositions4Sets(TSelf upper, TSelf right, TSelf lower, TSelf left, TSelf lowerFeedBoard = default)
            => (
            TSelf.FindLockablePositions(upper, lowerFeedBoard),
            TSelf.FindLockablePositions(right, lowerFeedBoard),
            TSelf.FindLockablePositions(lower, lowerFeedBoard),
            TSelf.FindLockablePositions(left, lowerFeedBoard));

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual (TSelf upper, TSelf right, TSelf lower, TSelf left) FindLockablePositions4Sets((TSelf upper, TSelf right, TSelf lower, TSelf left) boards, TSelf lowerFeedBoard = default)
            => (
            TSelf.FindLockablePositions(boards.upper, lowerFeedBoard),
            TSelf.FindLockablePositions(boards.right, lowerFeedBoard),
            TSelf.FindLockablePositions(boards.lower, lowerFeedBoard),
            TSelf.FindLockablePositions(boards.left, lowerFeedBoard));
        #endregion

        static abstract bool GetBlockAt(TLineElement line, int x);

        static abstract bool GetBlockAtFullRange(TLineElement line, int x);

        static abstract int LocateAllBlocks(TSelf board, IBufferWriter<CompressedPositionsTuple> writer);

        #region Operator Overloads
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract TSelf operator &(TSelf left, TSelf right);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract TSelf operator |(TSelf left, TSelf right);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract TSelf operator ^(TSelf left, TSelf right);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract TSelf operator ~(TSelf value);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract TSelf operator <<(TSelf left, [ConstantExpected] int right);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract TSelf operator >>(TSelf left, [ConstantExpected] int right);

        #endregion

        #region Vertical Shift

        #region Shift Down
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract TSelf ShiftDownOneLine(TSelf board, TLineElement upperFeedValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract TSelf ShiftDownOneLine(TSelf board, TSelf upperFeedBoard);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual TSelf ShiftDownTwoLines(TSelf board, TLineElement upperFeedValue) => TSelf.ShiftDownOneLine(TSelf.ShiftDownOneLine(board, upperFeedValue), upperFeedValue);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual TSelf ShiftDownTwoLines(TSelf board, TSelf upperFeedBoard) => TSelf.ShiftDownOneLine(TSelf.ShiftDownOneLine(board, upperFeedBoard), upperFeedBoard);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual TSelf ShiftDownFourLines(TSelf board, TLineElement upperFeedValue) => TSelf.ShiftDownTwoLines(TSelf.ShiftDownTwoLines(board, upperFeedValue), upperFeedValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual TSelf ShiftDownEightLines(TSelf board, TLineElement upperFeedValue) => TSelf.ShiftDownFourLines(TSelf.ShiftDownFourLines(board, upperFeedValue), upperFeedValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual TSelf ShiftDownSixteenLines(TSelf board, TLineElement upperFeedValue) => TSelf.ShiftDownEightLines(TSelf.ShiftDownEightLines(board, upperFeedValue), upperFeedValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual TSelf ShiftDownVariableLines(TSelf board, int count, TLineElement upperFeedValue)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            if (count >= TSelf.Height)
            {
                return TSelf.CreateFilled(upperFeedValue);
            }
            var c = count;
            var d = count;
            var b = board;
            d -= 16;
            while (d >= 0)
            {
                c = d;
                b = TSelf.ShiftDownSixteenLines(b, upperFeedValue);
                d -= 16;
            }
            if ((c & 8) > 0)
            {
                b = TSelf.ShiftDownEightLines(b, upperFeedValue);
            }
            if ((c & 4) > 0)
            {
                b = TSelf.ShiftDownFourLines(b, upperFeedValue);
            }
            if ((c & 2) > 0)
            {
                b = TSelf.ShiftDownTwoLines(b, upperFeedValue);
            }
            if ((c & 1) > 0)
            {
                b = TSelf.ShiftDownOneLine(b, upperFeedValue);
            }
            return b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual TSelf ShiftDownConstantLines(TSelf board, [ConstantExpected] int count, TLineElement upperFeedValue) => TSelf.ShiftDownVariableLines(board, count, upperFeedValue);
        #endregion

        #region Shift Up
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract TSelf ShiftUpOneLine(TSelf board, TLineElement lowerFeedValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract TSelf ShiftUpOneLine(TSelf board, TSelf lowerFeedBoard);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual TSelf ShiftUpTwoLines(TSelf board, TLineElement lowerFeedValue) => TSelf.ShiftUpOneLine(TSelf.ShiftUpOneLine(board, lowerFeedValue), lowerFeedValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual TSelf ShiftUpTwoLines(TSelf board, TSelf lowerFeedBoard) => TSelf.ShiftUpOneLine(TSelf.ShiftUpOneLine(board, lowerFeedBoard), lowerFeedBoard);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual TSelf ShiftUpFourLines(TSelf board, TLineElement lowerFeedValue) => TSelf.ShiftUpTwoLines(TSelf.ShiftUpTwoLines(board, lowerFeedValue), lowerFeedValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual TSelf ShiftUpEightLines(TSelf board, TLineElement lowerFeedValue) => TSelf.ShiftUpFourLines(TSelf.ShiftUpFourLines(board, lowerFeedValue), lowerFeedValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual TSelf ShiftUpSixteenLines(TSelf board, TLineElement lowerFeedValue) => TSelf.ShiftUpEightLines(TSelf.ShiftUpEightLines(board, lowerFeedValue), lowerFeedValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract TSelf ShiftUpVariableLines(TSelf board, int count, TSelf lowerFeedBoard);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual TSelf ShiftUpVariableLines(TSelf board, int count, TLineElement lowerFeedValue)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            if (count >= TSelf.Height)
            {
                return TSelf.CreateFilled(lowerFeedValue);
            }
            var c = count;
            var d = count;
            var b = board;
            d -= 16;
            while (d >= 0)
            {
                c = d;
                b = TSelf.ShiftUpSixteenLines(b, lowerFeedValue);
                d -= 16;
            }
            if ((c & 8) > 0)
            {
                b = TSelf.ShiftUpEightLines(b, lowerFeedValue);
            }
            if ((c & 4) > 0)
            {
                b = TSelf.ShiftUpFourLines(b, lowerFeedValue);
            }
            if ((c & 2) > 0)
            {
                b = TSelf.ShiftUpTwoLines(b, lowerFeedValue);
            }
            if ((c & 1) > 0)
            {
                b = TSelf.ShiftUpOneLine(b, lowerFeedValue);
            }
            return b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual TSelf ShiftUpConstantLines(TSelf board, [ConstantExpected] int count, TLineElement lowerFeedValue) => TSelf.ShiftUpVariableLines(board, count, lowerFeedValue);
        #endregion

        #endregion

        #region Board Classification
        static virtual bool IsBoardZero(TSelf board) => board == TSelf.Zero;
        static virtual bool IsBoardEmpty(TSelf board) => board == TSelf.Empty;
        static virtual bool IsBoardInvertedEmpty(TSelf board) => board == TSelf.InvertedEmpty;
        static virtual bool IsBoardAllBitsSet(TSelf board) => board == TSelf.AllBitsSet;
        #endregion

        #region Line Classification
        static virtual bool IsLineZero(TLineElement line) => line == TSelf.ZeroLine;
        static virtual bool IsLineEmpty(TLineElement line) => line == TSelf.EmptyLine;
        static virtual bool IsLineInvertedEmpty(TLineElement line) => line == TSelf.InvertedEmptyLine;
        static virtual bool IsLineFull(TLineElement line) => line == TSelf.FullLine;
        #endregion

        #region Board Statistics
        static abstract int TotalBlocks(TSelf board);

        static abstract TSelf BlocksPerLine(TSelf board);

        #endregion
    }
}