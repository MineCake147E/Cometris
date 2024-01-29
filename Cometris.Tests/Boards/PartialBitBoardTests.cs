using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

using Cometris.Boards;

namespace Cometris.Tests.Boards
{
    [TestFixture(typeof(PartialBitBoard256X2), typeof(PartialBitBoard256X2))]
    [TestFixture(typeof(PartialBitBoard512), typeof(Vector512<ushort>))]
    public partial class PartialBitBoardTests<TBitBoard, TLineMask>
        where TBitBoard : unmanaged, IMaskableBitBoard<TBitBoard, ushort, TLineMask, uint>
        where TLineMask : unmanaged, IEquatable<TLineMask>
    {
        private static IEnumerable<TestCaseData> FillDropReachableTestCaseSource()
        {
            yield return new TestCaseData(TBitBoard.CreateFilled((ushort)0x0ff0u), TBitBoard.CreateSingleLine((ushort)0x0ff0u, 31), TBitBoard.CreateFilled((ushort)0x0ff0u));
            for (var i = 30; i >= 2; i--)
            {
                yield return new TestCaseData(TBitBoard.CreateFilled((ushort)0x0ff0u).WithLine(0, i), TBitBoard.CreateTwoLines(i - 2, 31, (ushort)0x0ff0u, (ushort)0x0ff0u), TBitBoard.CreateFilled((ushort)0x0ff0u).WithLine(0, i).WithLine(0, i - 1));
            }
        }

        private static TBitBoard FillHorizontalReachable(TBitBoard board, TBitBoard reached)
        {
            for (var k = 0; k < TBitBoard.StorableWidth - 3; k += 4)
            {
                var u = reached >> 1;
                u |= reached << 1;
                u &= board;
                reached |= u;
                u = reached >> 1;
                u |= reached << 1;
                u &= board;
                reached |= u;
                u = reached >> 1;
                u |= reached << 1;
                u &= board;
                reached |= u;
                u = reached >> 1;
                u |= reached << 1;
                u &= board;
                u |= reached;
                if (u == reached) return u;
                reached = u;
            }
            return reached;
        }
        [SkipLocalsInit]
        private static IEnumerable<TestCaseData> FillHorizontalReachableTestCaseSource()
        {
            var f32 = new FixedArray32<ushort>();
            yield return new TestCaseData(TBitBoard.CreateFilled((ushort)0x0ff0u), TBitBoard.CreateFilled((ushort)0x0100u), TBitBoard.CreateFilled((ushort)0x0ff0u));
            Span<ushort> t = f32;
            var i = 0;
            _ = t[15];
            for (; i < 16; i++)
            {
                t[i] = (ushort)BitOperations.RotateLeft(~0x0001_0001u, i);
            }
            for (; i < t.Length; i++)
            {
                t[i] = (ushort)BitOperations.RotateRight(~0x8000_8000u, i);
            }
            var board = TBitBoard.FromBoard(t, 0);
            for (i = 0; i < 16; i++)
            {
                var start = TBitBoard.CreateFilled((ushort)(1u << i)) & board;
                var expected = FillHorizontalReachable(board, start);
                yield return new TestCaseData(board, start, expected);
            }
        }

        private static IEnumerable<TBitBoard> Boards()
        {
            yield return TBitBoard.AllBitsSet;
            yield return IndexBoard();
            yield return TBitBoard.FromBoard([0b1111101010000111, 0b1111001000000111, 0b1110001000110111, 0b1110000001000111, 0b1110111101001111, 0b1110000001011111, 0b1111001001000111, 0b1111101000100111, 0b1110001011110111, 0b1110011000000111, 0b1110111001001111, 0b1110000001011111, 0b1111001101000111, 0b1111100001100111, 0b1110001001110111, 0b1110011000000111, 0b1110111011001111, 0b1110000001011111, 0b1111000001000111, 0b1111111111100111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111], TBitBoard.ZeroLine);
        }

        private static TBitBoard IndexBoard()
        {
            if (TBitBoard.Height <= 32)
            {
                return TBitBoard.FromBoard([0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31], TBitBoard.ZeroLine);
            }
            else
            {
                Span<ushort> lines = stackalloc ushort[TBitBoard.Height];
                for (var i = 0; i < lines.Length; i++)
                {
                    lines[i] = (ushort)i;
                }
                return TBitBoard.FromBoard(lines, (ushort)TBitBoard.Height);
            }
        }

        private static IEnumerable<int> LinesRange(int lowerSpace = 0, int upperSpace = 0) => Enumerable.Range(lowerSpace, TBitBoard.Height - upperSpace - lowerSpace);
        private static IEnumerable<int> ColumnsRange() => Enumerable.Range(0, TBitBoard.StorableWidth);

        private static IEnumerable<ushort> LineSamples(bool includeZero = false)
        {
            if (includeZero) yield return ushort.MinValue;
            yield return ushort.MaxValue;
            foreach (var item in ColumnsRange().Select(x => (ushort)(0x8000u >> x)))
            {
                yield return item;
            }
        }

        private static IEnumerable<TestCaseData> BoardOnlyTestCaseSource() => Boards().Select(a => new TestCaseData(a));

        private static IEnumerable<TestCaseData> ShiftVerticalVariableTestCaseSource() => LinesRange().Select(a => new TestCaseData(a));
        private static IEnumerable<TestCaseData> ClearClearableLinesTestCaseSource()
        {
            yield return new TestCaseData(TBitBoard.Empty, TBitBoard.Empty, TBitBoard.ZeroMask);
            yield return new TestCaseData(TBitBoard.Empty.WithLine(ushort.MaxValue, 2), TBitBoard.Empty, TBitBoard.CreateMaskFromBoard(TBitBoard.Zero.WithLine(ushort.MaxValue, 2)));
            yield return new TestCaseData(TBitBoard.FromBoard([0xe007, 0xffff, 0xffff], 0xe007), TBitBoard.Empty, TBitBoard.CreateMaskFromBoard(TBitBoard.FromBoard([0, 0xffff, 0xffff], 0)));
            yield return new TestCaseData(TBitBoard.FromBoard([0xe007, 0xffff, 0xe007, 0xffff], 0xe007), TBitBoard.Empty, TBitBoard.CreateMaskFromBoard(TBitBoard.FromBoard([0, 0xffff, 0, 0xffff], 0)));
        }

        [SkipLocalsInit]
        private static IEnumerable<TestCaseData> ClearLinesTestCaseSource()
            => LineMasks().Concat(LineMasks().Select(a => ~a)).Select(item => new TestCaseData(ExpandMask(item)));

        private static TLineMask ExpandMask(uint item)
        {
            var q = new FixedArray32<ushort>();
            Span<ushort> a = q;
            var y = item;
            for (var i = 0; i < a.Length; i++)
            {
                a[i] = (y & 1) > 0 ? ushort.MaxValue : ushort.MinValue;
                y >>= 1;
            }
            var mask = TBitBoard.CreateMaskFromBoard(TBitBoard.FromBoard(a, 0));
            return mask;
        }

        private static IEnumerable<uint> LineMasks() => [0xaaaa_aaaau, 0xcccc_ccccu, 0xf0f0_f0f0u, 0xff00_ff00u];

        [SkipLocalsInit]
        private static IEnumerable<TestCaseData> ExpandMaskTestCaseSource()
            => LineMasks().Concat(LineMasks().Select(a => ~a)).Select(item => new TestCaseData(item));

        #region Board Construction

        [TestCaseSource(nameof(WithLineTestCaseSource))]
        public static void FromBoardWorksCorrectly(int length, ushort fill)
        {
            var board = IndexBoard();
            Span<ushort> span = stackalloc ushort[TBitBoard.Height];
            TBitBoard.Store(board, span);
            span.Slice(length).Fill(fill);
            var expected = TBitBoard.FromBoard(span, fill);
            var actual = TBitBoard.FromBoard(span.Slice(0, length), fill);
            Assert.That(actual, Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> WithLineTestCaseSource()
            => LineSamples(true).SelectMany(line => LinesRange().Select(y => new TestCaseData(y, line)));

        [TestCaseSource(nameof(WithLineTestCaseSource))]
        [SkipLocalsInit]
        public void WithLineWorksCorrectly(int y, ushort line)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)y, (uint)TBitBoard.Height);
            var board = IndexBoard();
            Span<ushort> span = stackalloc ushort[TBitBoard.Height];
            TBitBoard.Store(board, span);
            span[y] = line;
            var expected = TBitBoard.FromBoard(span, 0);
            var actual = board.WithLine(line, y);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(WithLineTestCaseSource))]
        [SkipLocalsInit]
        public void CreateSingleLineWorksCorrectly(int y, ushort line)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)y, (uint)TBitBoard.Height);
            Span<ushort> span = stackalloc ushort[TBitBoard.Height];
            span.Clear();
            span[y] = line;
            var expected = TBitBoard.FromBoard(span, 0);
            var actual = TBitBoard.CreateSingleLine(line, y);
            Assert.That(actual, Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> CreateFilledTestCaseSource()
            => LineSamples().Select(a => new TestCaseData(a));

        [TestCaseSource(nameof(CreateFilledTestCaseSource))]
        public void CreateFilledWorksCorrectly(ushort fill)
        {
            Span<ushort> span = stackalloc ushort[TBitBoard.Height];
            span.Fill(fill);
            var expected = TBitBoard.FromBoard(span, fill);
            var actual = TBitBoard.CreateFilled(fill);
            Assert.That(actual, Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> CreateSingleBlockTestCaseSource()
            => ColumnsRange().SelectMany(x => LinesRange().Select(y => new TestCaseData(x, y)));

        [TestCaseSource(nameof(CreateSingleBlockTestCaseSource))]
        public void CreateSingleBlockWorksCorrectly(int x, int y)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)y, (uint)TBitBoard.Height);
            Span<ushort> span = stackalloc ushort[TBitBoard.Height];
            span.Clear();
            var line = TBitBoard.CreateSingleBlockLine(x);
            span[y] = line;
            var expected = TBitBoard.FromBoard(span, 0);
            var actual = TBitBoard.CreateSingleBlock(x, y);
            Assert.That(actual, Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> CreateTwoLinesTestCaseSource()
            => LinesRange().SelectMany(y0 => LinesRange().Where(y1 => y0 != y1).Select(
                y1 => new TestCaseData(y0, y1, ushort.MaxValue, ushort.MaxValue)));

        [TestCaseSource(nameof(CreateTwoLinesTestCaseSource))]
        public void CreateTwoLinesWorksCorrectly(int y0, int y1, ushort line0, ushort line1)
        {
            var expected = TBitBoard.CreateSingleLine(line0, y0).WithLine(line1, y1);
            var actual = TBitBoard.CreateTwoLines(y0, y1, line0, line1);
            Assert.That(actual, Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> CreateTwoAdjacentLinesUpTestCaseSource()
            => LinesRange(0, 1).Select(y => new TestCaseData(y, (ushort)y, ushort.MaxValue));

        [TestCaseSource(nameof(CreateTwoAdjacentLinesUpTestCaseSource))]
        public void CreateTwoAdjacentLinesUpWorksCorrectly(int y, ushort lineMiddle, ushort lineUpper)
        {
            var expected = TBitBoard.CreateThreeAdjacentLines(y, 0, lineMiddle, lineUpper);
            var actual = TBitBoard.CreateTwoAdjacentLinesUp(y, lineMiddle, lineUpper);
            Assert.That(actual, Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> CreateTwoAdjacentLinesDownTestCaseSource()
            => LinesRange(1, 0).Select(y => new TestCaseData(y, ushort.MaxValue, (ushort)y));

        [TestCaseSource(nameof(CreateTwoAdjacentLinesDownTestCaseSource))]
        public void CreateTwoAdjacentLinesDownWorksCorrectly(int y, ushort lineLower, ushort lineMiddle)
        {
            var expected = TBitBoard.CreateThreeAdjacentLines(y, lineLower, lineMiddle, 0);
            var actual = TBitBoard.CreateTwoAdjacentLinesDown(y, lineLower, lineMiddle);
            Assert.That(actual, Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> CreateThreeAdjacentLinesTestCaseSource()
            => LinesRange(1, 1).SelectMany(y => LineSamples().Select(line => new TestCaseData(y, line, (ushort)y, line)));

        [TestCaseSource(nameof(CreateThreeAdjacentLinesTestCaseSource))]
        public void CreateThreeAdjacentLinesWorksCorrectly(int y, ushort lineLower, ushort lineMiddle, ushort lineUpper)
        {
            var expected = TBitBoard.CreateTwoLines(y - 1, y, lineLower, lineMiddle).WithLine(lineUpper, y + 1);
            var actual = TBitBoard.CreateThreeAdjacentLines(y, lineLower, lineMiddle, lineUpper);
            Assert.That(actual, Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> CreateVerticalI4PieceTestCaseSource()
            => ColumnsRange().SelectMany(x => LinesRange(2, 1).Select(y => new TestCaseData(x, y)));

        [TestCaseSource(nameof(CreateVerticalI4PieceTestCaseSource))]
        public void CreateVerticalI4PieceWorksCorrectly(int x, int y)
        {
            var line = TBitBoard.CreateSingleBlockLine(x);
            var expected = TBitBoard.CreateTwoAdjacentLinesUp(y, line, line);
            expected |= TBitBoard.ShiftDownTwoLines(expected, 0);
            var actual = TBitBoard.CreateVerticalI4Piece(x, y);
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion

        [TestCaseSource(nameof(FillDropReachableTestCaseSource))]
        public void FillDropReachableFillsCorrectly(TBitBoard mobility, TBitBoard reached, TBitBoard expected)
        {
            var actual = TBitBoard.FillDropReachable(mobility, reached);
            Console.WriteLine(actual.ToString());
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(FillDropReachableTestCaseSource))]
        public void FillDropReachable4SetsFillsCorrectly(TBitBoard mobility, TBitBoard reached, TBitBoard expected)
        {
            var actual = TBitBoard.FillDropReachable4Sets((mobility, mobility, mobility, mobility), (reached, reached, reached, reached));
            Console.WriteLine(actual.ToString());
            Assert.That(actual, Is.EqualTo((expected, expected, expected, expected)));
        }

        [TestCaseSource(nameof(FillDropReachableTestCaseSource))]
        public void FillDropReachable4SetsNonTupleFillsCorrectly(TBitBoard mobility, TBitBoard reached, TBitBoard expected)
        {
            var actual = TBitBoard.FillDropReachable4Sets(mobility, mobility, mobility, mobility, reached, reached, reached, reached);
            Console.WriteLine(actual.ToString());
            Assert.That(actual, Is.EqualTo((expected, expected, expected, expected)));
        }

        [TestCaseSource(nameof(FillHorizontalReachableTestCaseSource))]
        public void FillHorizontalReachableFillsCorrectly(TBitBoard mobility, TBitBoard reached, TBitBoard expected)
        {
            var actual = TBitBoard.FillHorizontalReachable(mobility, reached);
            Console.WriteLine(actual.ToString());
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(FillHorizontalReachableTestCaseSource))]
        public void FillHorizontalReachable4SetsFillsCorrectly(TBitBoard mobility, TBitBoard reached, TBitBoard expected)
        {
            var actual = TBitBoard.FillHorizontalReachable4Sets((mobility, mobility, mobility, mobility), (reached, reached, reached, reached));
            Console.WriteLine(actual.ToString());
            Assert.That(actual, Is.EqualTo((expected, expected, expected, expected)));
        }

        [TestCaseSource(nameof(ClearClearableLinesTestCaseSource))]
        public void ClearClearableLinesClearsCorrectly(TBitBoard board, TBitBoard expectedBoard, TLineMask expectedCleared)
        {
            var actual = TBitBoard.ClearClearableLines(board, FullBitBoard.EmptyRow, out var clearedLines);
            Console.WriteLine(actual.ToString());
            Assert.That(actual, Is.EqualTo(expectedBoard));
            Console.WriteLine(clearedLines.ToString());
            Assert.That(clearedLines, Is.EqualTo(expectedCleared));
        }

        [TestCaseSource(nameof(ClearLinesTestCaseSource))]
        public void ClearLinesClearsCorrectly(TLineMask lines)
        {
            var board = IndexBoard();
            Span<ushort> res = stackalloc ushort[TBitBoard.Height];
            ref var dst = ref MemoryMarshal.GetReference(res);
            TBitBoard.StoreUnsafe(board, ref dst);
            nint j = 0, k;
            for (nint i = 0; i < TBitBoard.Height; i++)
            {
                var v = Unsafe.Add(ref dst, i);
                k = j;
                var f = TBitBoard.IsSetAt(lines, (byte)i) ? 0 : 1;
                j += f;
                Unsafe.Add(ref dst, k) = v;
            }
            res[(int)j..].Fill(FullBitBoard.EmptyRow);
            var expectedBoard = TBitBoard.LoadUnsafe(ref dst);
            var actual = TBitBoard.ClearLines(board, FullBitBoard.EmptyRow, lines);
            Console.WriteLine(actual.ToString());
            Assert.That(actual, Is.EqualTo(expectedBoard));
        }

        #region Shift Down
        [TestCaseSource(nameof(BoardOnlyTestCaseSource))]
        public void ShiftDownOneLineShiftsCorrectly(TBitBoard board)
        {
            var f32 = new FixedArray32<ushort>();
            Span<ushort> a = f32;
            TBitBoard.StoreUnsafe(board, ref MemoryMarshal.GetReference(a), 0);
            var expected = TBitBoard.FromBoard(a[1..], FullBitBoard.EmptyRow);
            var actual = TBitBoard.ShiftDownOneLine(board, FullBitBoard.EmptyRow);
            Console.WriteLine(actual.ToString());
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(BoardOnlyTestCaseSource))]
        public void ShiftDownOneLinePreFilledShiftsCorrectly(TBitBoard board)
        {
            var filled = TBitBoard.CreateFilled(0xaaaa);
            var f32 = new FixedArray32<ushort>();
            Span<ushort> a = f32;
            TBitBoard.StoreUnsafe(board, ref MemoryMarshal.GetReference(a), 0);
            var expected = TBitBoard.FromBoard(a[1..], 0xaaaa);
            var actual = TBitBoard.ShiftDownOneLine(board, filled);
            Console.WriteLine(actual.ToString());
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(BoardOnlyTestCaseSource))]
        public void ShiftDownTwoLinesShiftsCorrectly(TBitBoard board)
        {
            var f32 = new FixedArray32<ushort>();
            Span<ushort> a = f32;
            TBitBoard.StoreUnsafe(board, ref MemoryMarshal.GetReference(a), 0);
            var expected = TBitBoard.FromBoard(a[2..], FullBitBoard.EmptyRow);
            var actual = TBitBoard.ShiftDownTwoLines(board, FullBitBoard.EmptyRow);
            Console.WriteLine(actual.ToString());
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(BoardOnlyTestCaseSource))]
        public void ShiftDownTwoLinesPreFilledShiftsCorrectly(TBitBoard board)
        {
            var filled = TBitBoard.CreateFilled(0xaaaa);
            var f32 = new FixedArray32<ushort>();
            Span<ushort> a = f32;
            TBitBoard.StoreUnsafe(board, ref MemoryMarshal.GetReference(a), 0);
            var expected = TBitBoard.FromBoard(a[2..], 0xaaaa);
            var actual = TBitBoard.ShiftDownTwoLines(board, filled);
            Console.WriteLine(actual.ToString());
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(BoardOnlyTestCaseSource))]
        public void ShiftDownFourLinesShiftsCorrectly(TBitBoard board)
        {
            var f32 = new FixedArray32<ushort>();
            Span<ushort> a = f32;
            TBitBoard.StoreUnsafe(board, ref MemoryMarshal.GetReference(a), 0);
            var expected = TBitBoard.FromBoard(a[4..], FullBitBoard.EmptyRow);
            var actual = TBitBoard.ShiftDownFourLines(board, FullBitBoard.EmptyRow);
            Console.WriteLine(actual.ToString());
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(BoardOnlyTestCaseSource))]
        public void ShiftDownEightLinesShiftsCorrectly(TBitBoard board)
        {
            var f32 = new FixedArray32<ushort>();
            Span<ushort> a = f32;
            TBitBoard.StoreUnsafe(board, ref MemoryMarshal.GetReference(a), 0);
            var expected = TBitBoard.FromBoard(a[8..], FullBitBoard.EmptyRow);
            var actual = TBitBoard.ShiftDownEightLines(board, FullBitBoard.EmptyRow);
            Console.WriteLine(actual.ToString());
            Assert.That(actual, Is.EqualTo(expected));
        }
        [TestCaseSource(nameof(BoardOnlyTestCaseSource))]
        public void ShiftDownSixteenLinesShiftsCorrectly(TBitBoard board)
        {
            var f32 = new FixedArray32<ushort>();
            Span<ushort> a = f32;
            TBitBoard.StoreUnsafe(board, ref MemoryMarshal.GetReference(a), 0);
            var expected = TBitBoard.FromBoard(a[16..], FullBitBoard.EmptyRow);
            var actual = TBitBoard.ShiftDownSixteenLines(board, FullBitBoard.EmptyRow);
            Console.WriteLine(actual.ToString());
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(ShiftVerticalVariableTestCaseSource))]
        public void ShiftDownVariableLinesShiftsCorrectly(int count)
        {
            ReadOnlySpan<ushort> a = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31];
            var board = TBitBoard.LoadUnsafe(ref MemoryMarshal.GetReference(a));
            var expected = TBitBoard.FromBoard(a[count..], FullBitBoard.EmptyRow);
            var actual = TBitBoard.ShiftDownVariableLines(board, count, FullBitBoard.EmptyRow);
            Console.WriteLine(actual.ToString());
            Assert.That(actual, Is.EqualTo(expected));
        }
        #endregion

        #region Shift Up
        [TestCaseSource(nameof(BoardOnlyTestCaseSource))]
        public void ShiftUpOneLineShiftsCorrectly(TBitBoard board)
        {
            var f33 = new FixedArray33<ushort>();
            Span<ushort> a = f33;
            TBitBoard.StoreUnsafe(board, ref Unsafe.Add(ref MemoryMarshal.GetReference(a), 1), 0);
            a[0] = FullBitBoard.FullRow;
            var expected = TBitBoard.FromBoard(a, FullBitBoard.EmptyRow);
            var actual = TBitBoard.ShiftUpOneLine(board, FullBitBoard.FullRow);
            Console.WriteLine(actual.ToString());
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(BoardOnlyTestCaseSource))]
        public void ShiftUpOneLinePreFilledShiftsCorrectly(TBitBoard board)
        {
            var fill = TBitBoard.CreateFilled(0xaaaa);
            var f33 = new FixedArray33<ushort>();
            Span<ushort> a = f33;
            TBitBoard.StoreUnsafe(board, ref Unsafe.Add(ref MemoryMarshal.GetReference(a), 1), 0);
            a[0] = 0xaaaa;
            var expected = TBitBoard.FromBoard(a, 0xaaaa);
            var actual = TBitBoard.ShiftUpOneLine(board, fill);
            Console.WriteLine(actual.ToString());
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(BoardOnlyTestCaseSource))]
        public void ShiftUpTwoLinesShiftsCorrectly(TBitBoard board)
        {
            var expected = TBitBoard.ShiftUpOneLine(TBitBoard.ShiftUpOneLine(board, FullBitBoard.FullRow), FullBitBoard.FullRow);
            var actual = TBitBoard.ShiftUpTwoLines(board, FullBitBoard.FullRow);
            Console.WriteLine(actual.ToString());
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(BoardOnlyTestCaseSource))]
        public void ShiftUpTwoLinesPreFilledShiftsCorrectly(TBitBoard board)
        {
            var fill = TBitBoard.CreateFilled(0xaaaa);
            var expected = TBitBoard.ShiftUpOneLine(TBitBoard.ShiftUpOneLine(board, 0xaaaa), 0xaaaa);
            var actual = TBitBoard.ShiftUpTwoLines(board, fill);
            Console.WriteLine(actual.ToString());
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(BoardOnlyTestCaseSource))]
        public void ShiftUpFourLinesShiftsCorrectly(TBitBoard board)
        {
            var expected = TBitBoard.ShiftUpTwoLines(TBitBoard.ShiftUpTwoLines(board, FullBitBoard.FullRow), FullBitBoard.FullRow);
            var actual = TBitBoard.ShiftUpFourLines(board, FullBitBoard.FullRow);
            Console.WriteLine(actual.ToString());
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(BoardOnlyTestCaseSource))]
        public void ShiftUpEightLinesShiftsCorrectly(TBitBoard board)
        {
            var expected = TBitBoard.ShiftUpFourLines(TBitBoard.ShiftUpFourLines(board, FullBitBoard.FullRow), FullBitBoard.FullRow);
            var actual = TBitBoard.ShiftUpEightLines(board, FullBitBoard.FullRow);
            Console.WriteLine(actual.ToString());
            Assert.That(actual, Is.EqualTo(expected));
        }
        [TestCaseSource(nameof(BoardOnlyTestCaseSource))]
        public void ShiftUpSixteenLinesShiftsCorrectly(TBitBoard board)
        {
            var expected = TBitBoard.ShiftUpEightLines(TBitBoard.ShiftUpEightLines(board, FullBitBoard.FullRow), FullBitBoard.FullRow);
            var actual = TBitBoard.ShiftUpSixteenLines(board, FullBitBoard.FullRow);
            Console.WriteLine(actual.ToString());
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(ShiftVerticalVariableTestCaseSource))]
        public void ShiftUpVariableLinesShiftsCorrectly(int count)
        {
            var board = TBitBoard.FromBoard([0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31], TBitBoard.ZeroLine);
            var f64 = new FixedArray64<ushort>();
            Span<ushort> a = f64;
            a.Fill(FullBitBoard.FullRow);
            TBitBoard.StoreUnsafe(board, ref MemoryMarshal.GetReference(a), count);
            var expected = TBitBoard.FromBoard(a, FullBitBoard.FullRow);
            var actual = TBitBoard.ShiftUpVariableLines(board, count, FullBitBoard.FullRow);
            Console.WriteLine(actual.ToString());
            Assert.That(actual, Is.EqualTo(expected));
        }
        #endregion

        #region TotalBlocks
        [TestCaseSource(nameof(BoardOnlyTestCaseSource))]
        public void TotalBlocksCountsCorrectly(TBitBoard board)
        {
            var f32 = new FixedArray32<ushort>();
            Span<ushort> a = f32;
            TBitBoard.StoreUnsafe(board, ref MemoryMarshal.GetReference(a));
            var expected = 0;
            for (var i = 0; i < a.Length; i++)
            {
                expected += BitOperations.PopCount(a[i]);
            }
            var actual = TBitBoard.TotalBlocks(board);
            Console.WriteLine($"Total Blocks: {actual}");
            Assert.That(actual, Is.EqualTo(expected));
        }
        #endregion

        [TestCaseSource(nameof(BoardOnlyTestCaseSource))]
        public void LocateAllBlockLocatesCorrectly(TBitBoard board)
            => Assert.Multiple(() =>
            {
                var remaining = board;
                ArrayBufferWriter<CompressedPositionsTuple> writer = new();
                var count = TBitBoard.LocateAllBlocks(board, writer);
                Assert.That(count, Is.EqualTo(TBitBoard.TotalBlocks(board)));
                foreach (var item in writer.WrittenSpan.ToArray().SelectMany(a => a))
                {
                    remaining ^= TBitBoard.CreateSingleBlock(item.X, item.Y);
                }
                Assert.That(remaining, Is.EqualTo(TBitBoard.Zero));
            });

        [TestCaseSource(nameof(ExpandMaskTestCaseSource))]
        public void ExpandMaskExpandsCorrectly(uint mask)
        {
            var expanded = TBitBoard.ExpandMask(mask);
            Assert.That(expanded, Is.EqualTo(ExpandMask(mask)));
        }

        [TestCaseSource(nameof(ExpandMaskTestCaseSource))]
        public void CompressMaskExpandsCorrectly(uint mask)
        {
            var expanded = ExpandMask(mask);
            var compressed = TBitBoard.CompressMask(expanded);
            Assert.That(compressed, Is.EqualTo(mask));
        }
    }
}
