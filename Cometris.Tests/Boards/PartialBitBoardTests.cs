using System.Buffers;
using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

using Cometris.Boards;
using Cometris.Boards.Hashing;
using Cometris.Movements;
using Cometris.Movements.Reachability;
using Cometris.Pieces;
using Cometris.Pieces.Mobility;
using Cometris.Pieces.Placing;
using Cometris.Tests.Movements;

using MikoMino;

namespace Cometris.Tests.Boards
{
    [TestFixture(typeof(PartialBitBoard128X4), typeof(PartialBitBoard128X4))]
    [TestFixture(typeof(PartialBitBoard256X2), typeof(PartialBitBoard256X2))]
    [TestFixture(typeof(PartialBitBoard512), typeof(Vector512<ushort>))]
    public partial class PartialBitBoardTests<TBitBoard, TLineMask>
        where TBitBoard : unmanaged, ICompactMaskableBitBoard<TBitBoard, ushort, TLineMask, uint>
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
            yield return new TestCaseData(TBitBoard.CreateFilled((ushort)0x0ff0u), TBitBoard.CreateFilled((ushort)0x0080u), TBitBoard.CreateFilled((ushort)0x0ff0u));
            yield return new TestCaseData(TBitBoard.CreateFilled((ushort)0x0ff0u), TBitBoard.CreateFilled((ushort)0x0180u), TBitBoard.CreateFilled((ushort)0x0ff0u));
            yield return new TestCaseData(TBitBoard.CreateFilled((ushort)0x0ff0u), TBitBoard.CreateFilled((ushort)0x0810u), TBitBoard.CreateFilled((ushort)0x0ff0u));
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

        private static IEnumerable<TBitBoard> Boards() => [
            TBitBoard.AllBitsSet,
            IndexBoard(),
            TBitBoard.FromBoard([0b1111101010000111, 0b1111001000000111, 0b1110001000110111, 0b1110000001000111, 0b1110111101001111, 0b1110000001011111, 0b1111001001000111, 0b1111101000100111, 0b1110001011110111, 0b1110011000000111, 0b1110111001001111, 0b1110000001011111, 0b1111001101000111, 0b1111100001100111, 0b1110001001110111, 0b1110011000000111, 0b1110111011001111, 0b1110000001011111, 0b1111000001000111, 0b1111111111100111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111], TBitBoard.EmptyLine),
            TBitBoard.Empty
        ];

        private static IEnumerable<TBitBoard> BoardsForHashTests() => [
            TBitBoard.FromBoard([0b1111101010000111, 0b1111001000000111, 0b1110001000110111, 0b1110000001000111, 0b1110111101001111, 0b1110000001011111, 0b1111001001000111, 0b1111101000100111, 0b1110001011110111, 0b1110011000000111, 0b1110111001001111, 0b1110000001011111, 0b1111001101000111, 0b1111100001100111, 0b1110001001110111, 0b1110011000000111, 0b1110111011001111, 0b1110000001011111, 0b1111000001000111, 0b1111111111100111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111], TBitBoard.EmptyLine),
            TBitBoard.FromBoard([0b1111000011100111, 0b1111001011110111, 0b1111001001110111, 0b1110000001000111, 0b1110101001011111, 0b1111101000001111, 0b1110001011000111, 0b1110111001010111, 0b1110010001010111, 0b1110001101000111, 0b1110101000011111, 0b1111101011001111, 0b1110001001000111, 0b1110110001010111, 0b1110011101110111, 0b1110001000000111, 0b1110101011011111, 0b1111101000001111, 0b1110001000001111, 0b1110111111111111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111], TBitBoard.EmptyLine),
            TBitBoard.FromBoard([0b1110011100001111, 0b1110111101001111, 0b1110111001001111, 0b1110001000000111, 0b1111101001010111, 0b1111000001011111, 0b1110001101000111, 0b1110101001110111, 0b1110101000100111, 0b1110001011000111, 0b1111100001010111, 0b1111001101011111, 0b1110001001000111, 0b1110101000110111, 0b1110111011100111, 0b1110000001000111, 0b1111101101010111, 0b1111000001011111, 0b1111000001000111, 0b1111111111110111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111], TBitBoard.EmptyLine),
            TBitBoard.FromBoard([0b1110000011001111, 0b1111000001100111, 0b1110001001001111, 0b1110111010011111, 0b1110000000001111, 0b1111001001000111, 0b1110011010001111, 0b1111000001100111, 0b1110001001001111, 0b1110111010011111, 0b1110000000001111, 0b1111001001000111, 0b1110011010001111, 0b1111000000100111, 0b1110001001001111, 0b1110111010011111, 0b1110000000001111, 0b1111001000000111, 0b1110011111111111, 0b1111001111111111, 0b1110010000000111, 0b1110000000000111, 0b1110000000000111], TBitBoard.EmptyLine),
            TBitBoard.FromBoard([0b1111001100000111, 0b1110011000001111, 0b1111001001000111, 0b1111100101110111, 0b1111000000000111, 0b1110001001001111, 0b1111000101100111, 0b1110011000001111, 0b1111001001000111, 0b1111100101110111, 0b1111000000000111, 0b1110001001001111, 0b1111000101100111, 0b1110010000001111, 0b1111001001000111, 0b1111100101110111, 0b1111000000000111, 0b1110000001001111, 0b1111111111100111, 0b1111111111001111, 0b1110000000100111, 0b1110000000000111, 0b1110000000000111], TBitBoard.EmptyLine),
            TBitBoard.FromBoard([0b1111111100000111, 0b1111111101110111, 0b1110000001110111, 0b1110111000000111, 0b1110111011100111, 0b1110000011100111, 0b1111110000000111, 0b1111110001110111, 0b1110000001110111, 0b1110111000000111, 0b1110111011100111, 0b1110000011100111, 0b1111110000000111, 0b1111110001110111, 0b1110000001110111, 0b1110111000000111, 0b1110111011100111, 0b1110000011100111, 0b1111111000000111, 0b1111111111110111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111], TBitBoard.EmptyLine),
            TBitBoard.FromBoard([0b111_111_0111_111_111], 0b111_111_0000_111_111),
            TBitBoard.FromBoard([0b111_111_0111_111_111, 0b111_111_0001_111_111, 0b111_111_0011_111_111], 0b111_111_0000_111_111),
            TBitBoard.FromBoard([0b1111110001111111,0b1111111001111111,0b1111110001111111,0b1111100001111111], TBitBoard.EmptyLine),
            TBitBoard.Empty
        ];

        private static Piece[] Pieces() => Enum.GetValues<Piece>().Except([Piece.None]).ToArray();

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
            var actual = TBitBoard.ShiftDown16Lines(board, FullBitBoard.EmptyRow);
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
            var actual = TBitBoard.ShiftUp16Lines(board, FullBitBoard.FullRow);
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

        #region Statistics
        [TestCaseSource(nameof(BoardOnlyTestCaseSource))]
        public void TotalBlocksCountsCorrectly(TBitBoard board)
        {
            var expected = 0;
            for (var i = 0; i < TBitBoard.Height; i++)
            {
                expected += BitOperations.PopCount(board[i]);
            }
            var actual = TBitBoard.TotalBlocks(board);
            Console.WriteLine($"Total Blocks: {actual}");
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(BoardOnlyTestCaseSource))]
        public void GetBlockHeightCountsCorrectly(TBitBoard board)
        {
            var expected = 0;
            for (var i = TBitBoard.Height - 1; i >= 0; i--)
            {
                if ((board[i] & TBitBoard.InvertedEmptyLine) != TBitBoard.ZeroLine)
                {
                    expected = i + 1;
                    break;
                }
            }
            var actual = TBitBoard.GetBlockHeight(board);
            Console.WriteLine($"Height: {actual}");
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(BoardOnlyTestCaseSource))]
        public void CalculateHashCalculatesCorrectly(TBitBoard board)
        {
            var actual = TBitBoard.CalculateHash(board);
            Console.WriteLine($"Hash: 0x{actual:x16}");
            Assert.Pass();
        }

        private static IEnumerable<TestCaseData> HashDoesNotCollideFromTestCaseSource()
            => BoardsForHashTests().SelectMany(a => Pieces().Select(b => new TestCaseData(a, b)));

        [TestCaseSource(nameof(HashDoesNotCollideFromTestCaseSource))]
        public async Task HashDoesNotCollideFromAsync(TBitBoard board, Piece piece)
        {
            var hashes = new ConcurrentDictionary<ulong, ConcurrentBag<CompressedPiecePlacement>>();
            var baseCollisionBag = new ConcurrentBag<CompressedPiecePlacement>();
            var b = board;
            var hash = TBitBoard.CalculateHash(b);
            var (upper, right, lower, left) = LocateReachabilityOf(piece, b);
            var count = TBitBoard.TotalBlocks(upper) + TBitBoard.TotalBlocks(right) + TBitBoard.TotalBlocks(lower) + TBitBoard.TotalBlocks(left);
            var m = Enumerable.SelectMany([(upper, Angle.Up), (right, Angle.Right), (lower, Angle.Down), (left, Angle.Left)], a => TBitBoard.LocateAllBlocks(a.Item1).Select(b => (position: b, angle: a.Item2)));
            await Parallel.ForEachAsync(m, (a, ctx) =>
            {
                var bb = b;
                bb |= PartialBitBoardTests<TBitBoard, TLineMask>.PlacePiece(piece, a.position, a.angle);
                var h = TBitBoard.CalculateHash(bb);
                var placement = new CompressedPiecePlacement(a.position, piece, a.angle);
                if (h == hash)
                {
                    baseCollisionBag.Add(placement);
                }
                _ = hashes.AddOrUpdate(h, [placement], (hash, bag) =>
                {
                    bag.Add(placement);
                    return bag;
                });
                return ValueTask.CompletedTask;
            });
            var baseCollisions = baseCollisionBag.ToArray();
            Console.WriteLine($"Collisions with Base: {baseCollisions.Length} / {count}");
            foreach (var item in baseCollisions)
            {
                Console.Write($"{item}:{Environment.NewLine}");
                var nb = b | PlacePiece(piece, item.Position, item.Angle);
                Console.WriteLine(BitBoardUtils.VisualizeDifferences(nb, b));
            }
            var childCollisions = hashes.Where(a => a.Value.Count > 1).ToArray();
            Console.WriteLine($"Collisions between children: {childCollisions.Select(a => a.Value.Count).Sum()} ({childCollisions.Length} groups) / {count}");
            foreach (var group in childCollisions)
            {
                Console.WriteLine($"Hash: {group.Key}: {group.Value.Count} collisions");
                foreach (var item in group.Value)
                {
                    Console.Write($"{item}:{Environment.NewLine}");
                    var nb = b | PlacePiece(piece, item.Position, item.Angle);
                    Console.WriteLine(BitBoardUtils.VisualizeDifferences(nb, b));
                }
                Console.WriteLine();
            }
            Assert.Multiple(() =>
            {
                Assert.That(baseCollisions, Is.Empty);
                Assert.That(childCollisions, Is.Empty);
            });
        }

        private static IEnumerable<TestCaseData> HashDoesNotCollideFromTwoPiecesTestCaseSource()
            => BoardsForHashTests().SelectMany(b => LinqUtils.GenerateAllTwoCombinationsOf(Pieces()).Select(a => new TestCaseData(b, a.Item1, a.Item2)));

        [TestCaseSource(nameof(HashDoesNotCollideFromTwoPiecesTestCaseSource))]
        public async Task HashDoesNotCollideFromOneStepTwoPiecesAsync(TBitBoard board, Piece piece, Piece next)
        {
            var b = board;
            var hash = TBitBoard.CalculateHash(b);
            IEqualityComparer<CompressedPiecePlacement> comparer = new PiecePlacingEqualityComparer<CompressedPiecePlacement, GuidelinePieceRegistry<TBitBoard>, TBitBoard>(b);
            var hashes = new ConcurrentDictionary<ulong, ConcurrentDictionary<CompressedPiecePlacement, byte>>();
            var baseCollisionBag = new ConcurrentDictionary<CompressedPiecePlacement, byte>(comparer);
            var rp0 = LocateReachabilityOf(piece, b);
            var c0 = TBitBoard.TotalBlocks(rp0.upper) + TBitBoard.TotalBlocks(rp0.right) + TBitBoard.TotalBlocks(rp0.lower) + TBitBoard.TotalBlocks(rp0.left);
            var m0 = LocateAllPositionsWithAngles(rp0);
            var rp1 = LocateReachabilityOf(next, b);
            var c1 = TBitBoard.TotalBlocks(rp1.upper) + TBitBoard.TotalBlocks(rp1.right) + TBitBoard.TotalBlocks(rp1.lower) + TBitBoard.TotalBlocks(rp1.left);
            var m1 = LocateAllPositionsWithAngles(rp1);
            var count = c0 + c1;
            var t0 = Parallel.ForEachAsync(m0, (a, ctx) =>
            {
                TestPositionForPiece(piece, a, hashes, baseCollisionBag, b, hash, comparer);
                return ValueTask.CompletedTask;
            });
            var t1 = Parallel.ForEachAsync(m1, (a, ctx) =>
            {
                TestPositionForPiece(next, a, hashes, baseCollisionBag, b, hash, comparer);
                return ValueTask.CompletedTask;
            });
            await Task.WhenAll(t0, t1).ConfigureAwait(false);
            var baseCollisions = baseCollisionBag.ToArray();
            Console.WriteLine($"Collisions with Base: {baseCollisions.Length} / {count}");
            foreach (var item in baseCollisions)
            {
                Console.Write($"{item}:{Environment.NewLine}");
                var nb = b | PlacePiece(item.Key);
                Console.WriteLine(BitBoardUtils.VisualizeDifferences(nb, b));
            }
            var childCollisions = hashes.Where(a => a.Value.Count > 1).ToArray();
            Console.WriteLine($"Collisions between children: {childCollisions.Select(a => a.Value.Count).Sum()} ({childCollisions.Length} groups) / {count}");
            foreach (var group in childCollisions)
            {
                Console.WriteLine($"Hash: {group.Key}: {group.Value.Count} collisions");
                foreach (var item in group.Value)
                {
                    Console.Write($"{item}:{Environment.NewLine}");
                    var nb = b | PlacePiece(item.Key);
                    Console.WriteLine(BitBoardUtils.VisualizeDifferences(nb, b));
                }
                Console.WriteLine();
            }
            Assert.Multiple(() =>
            {
                Assert.That(baseCollisions, Is.Empty);
                Assert.That(childCollisions, Is.Empty);
            });
        }

        private static void TestPositionForPiece(Piece piece, (Point position, Angle angle) a, ConcurrentDictionary<ulong, ConcurrentDictionary<CompressedPiecePlacement, byte>> hashes, ConcurrentDictionary<CompressedPiecePlacement, byte> baseCollisionBag, TBitBoard b, ulong hash, IEqualityComparer<CompressedPiecePlacement> comparer)
        {
            var bb = b;
            bb |= PartialBitBoardTests<TBitBoard, TLineMask>.PlacePiece(piece, a.position, a.angle);
            var h = TBitBoard.CalculateHash(bb);
            var placement = new CompressedPiecePlacement(a.position, piece, a.angle);
            if (h == hash)
            {
                _ = baseCollisionBag.TryAdd(placement, 0);
            }
            _ = hashes.AddOrUpdate(h,
                hash => new ConcurrentDictionary<CompressedPiecePlacement, byte>([KeyValuePair.Create(placement, byte.MinValue)], comparer),
                (hash, bag) =>
                {
                    _ = bag.TryAdd(placement, 0);
                    return bag;
                });
        }

        [TestCaseSource(nameof(HashDoesNotCollideFromTwoPiecesTestCaseSource))]
        public void HashDoesNotCollideFromTwoSteps(TBitBoard board, Piece piece, Piece next)
        {
            var baseBoard = board;
            var hash = TBitBoard.CalculateHash(baseBoard);
            var hashes = new ConcurrentDictionary<ulong, ConcurrentDictionary<TBitBoard, ConcurrentBag<(CompressedPiecePlacement p0, CompressedPiecePlacement p1)>>>();
            var baseCollisionBag = new ConcurrentDictionary<TBitBoard, ConcurrentBag<(CompressedPiecePlacement p0, CompressedPiecePlacement p1)>>();
            var l0p0 = TBitBoard.FindLockablePositions4Sets(LocateReachabilityOf(piece, baseBoard));
            var l0p1 = TBitBoard.FindLockablePositions4Sets(LocateReachabilityOf(next, baseBoard));
            var s0p0 = AttachPiece(piece, LocateAllPositionsWithAngles(l0p0)).ToList();
            var s0p1 = AttachPiece(next, LocateAllPositionsWithAngles(l0p1)).ToList();
            var s1p1 = s0p0.AsParallel().Select(p =>
            {
                var exclusion = l0p1;
                var board = baseBoard | PlacePiece(p);
                board = TBitBoard.ClearClearableLines(board, TBitBoard.EmptyLine, out var clearable);
                if (!TBitBoard.IsMaskZero(clearable)) exclusion = default;
                return (first: p, board, placements: FindAllPlacementsOf(next, board, exclusion));
            });
            var s1p0 = s0p1.AsParallel().Select(p =>
            {
                var exclusion = l0p0;
                var board = baseBoard | PartialBitBoardTests<TBitBoard, TLineMask>.PlacePiece(p);
                board = TBitBoard.ClearClearableLines(board, TBitBoard.EmptyLine, out var clearable);
                if (!TBitBoard.IsMaskZero(clearable)) exclusion = default;
                return (first: p, board, placements: PartialBitBoardTests<TBitBoard, TLineMask>.FindAllPlacementsOf(piece, board, exclusion));
            });
            var all = s1p0.Union(s1p1);
            var count = all.Sum(a => (long)a.placements.Count());
            all.ForAll(a =>
            {
                var board = a.board;
                foreach (var item in a.placements)
                {
                    var nb = board | PlacePiece(item);
                    var pair = (a.first, item);
                    var newHash = TBitBoard.CalculateHash(nb);
                    if (hash == newHash)
                    {
                        _ = baseCollisionBag.AddOrUpdate(nb, [pair], (b, t) =>
                        {
                            t.Add(pair);
                            return t;
                        });
                    }
                    _ = hashes.AddOrUpdate(newHash,
                        hash => new ConcurrentDictionary<TBitBoard, ConcurrentBag<(CompressedPiecePlacement p0, CompressedPiecePlacement p1)>>(
                            [KeyValuePair.Create(nb, new ConcurrentBag<(CompressedPiecePlacement p0, CompressedPiecePlacement p1)>([pair]))]),
                        (h, t) =>
                    {
                        _ = t.AddOrUpdate(nb, [pair], (b, t) =>
                        {
                            t.Add(pair);
                            return t;
                        });
                        return t;
                    });
                }
            });
            var baseCollisions = baseCollisionBag.ToArray();
            Console.WriteLine($"Collisions with Base: {baseCollisions.Select(a => a.Value.Count).Sum()} ({baseCollisions.Length} groups) / {count}");
            foreach (var item in baseCollisions)
            {
                Console.WriteLine($"Placements with same board result: {string.Join(", ", item.Value)}");
                Console.WriteLine(BitBoardUtils.VisualizeDifferences(item.Key, baseBoard));
                Console.WriteLine();
            }
            var childCollisions = hashes.Where(a => a.Value.Count > 1).ToArray();
            Console.WriteLine($"Collisions between children: {childCollisions.Select(a => a.Value.Sum(b => b.Value.Count)).Sum()} ({childCollisions.Length} groups) / {count}");
            foreach (var group in childCollisions)
            {
                Console.WriteLine($"Hash: {group.Key}: {group.Value.Count} collisions");
                foreach (var item in group.Value)
                {
                    Console.WriteLine($"Placements with same board result: {string.Join(", ", item.Value)}");
                    Console.WriteLine(BitBoardUtils.VisualizeDifferences(item.Key, baseBoard));
                    Console.WriteLine();
                }
                Console.WriteLine();
            }
            Assert.Multiple(() =>
            {
                Assert.That(baseCollisions, Is.Empty);
                Assert.That(childCollisions, Is.Empty);
            });
        }

        private static IEnumerable<CompressedPiecePlacement> FindAllPlacementsOf(Piece pieceToPlace, TBitBoard board, (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) exclusion = default)
            => AttachPiece(pieceToPlace, LocateAllPositionsWithAngles(TBitBoard.AndNot4Sets(exclusion, TBitBoard.FindLockablePositions4Sets(LocateReachabilityOf(pieceToPlace, board)))));

        private static IEnumerable<CompressedPiecePlacement> AttachPiece(Piece piece, IEnumerable<(Point position, Angle angle)> enumerable) => enumerable.Select(a => new CompressedPiecePlacement(a.position, piece, a.angle));
        private static IEnumerable<(Point position, Angle angle)> LocateAllPositionsWithAngles((TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) rp0)
            => Enumerable.SelectMany([(rp0.upper, Angle.Up), (rp0.right, Angle.Right), (rp0.lower, Angle.Down), (rp0.left, Angle.Left)],
                a => TBitBoard.LocateAllBlocks(a.Item1).Select(b => (position: b, angle: a.Item2)));
        
        private static TBitBoard PlacePiece(Piece piece, Point position, Angle angle) => piece switch
        {
            Piece.T => PieceTPlacer<TBitBoard>.Place(angle, position.X, position.Y),
            Piece.I => PieceIPlacer<TBitBoard>.Place(angle, position.X, position.Y),
            Piece.O => PieceOPlacer<TBitBoard>.Place(angle, position.X, position.Y),
            Piece.J => PieceJPlacer<TBitBoard>.Place(angle, position.X, position.Y),
            Piece.L => PieceLPlacer<TBitBoard>.Place(angle, position.X, position.Y),
            Piece.S => PieceSPlacer<TBitBoard>.Place(angle, position.X, position.Y),
            Piece.Z => PieceZPlacer<TBitBoard>.Place(angle, position.X, position.Y),
            _ => default,
        };

        private static TBitBoard PlacePiece(CompressedPiecePlacement item) => PlacePiece(item.Piece, item.Position, item.Angle);

        private static (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) LocateReachabilityOf(Piece piece, TBitBoard board)
        {
            var spawn = TBitBoard.CreateSingleLine(0x0100, 21);
            switch (piece)
            {
                case Piece.O:
                    var mobO = PieceOMovablePointLocater<TBitBoard>.LocateSymmetricMovablePoints(board);
                    return (MovementTestUtils.TraceAllSymmetric(mobO, spawn, board), default, default, default);
                case Piece.T:
                    var mobT = PieceTMovablePointLocater<TBitBoard>.LocateMovablePoints(board);
                    return MovementTestUtils.TraceAll<TBitBoard, PieceTRotatabilityLocator<TBitBoard>>(mobT, spawn, board);
                case Piece.J:
                    var mobJ = PieceJMovablePointLocater<TBitBoard>.LocateMovablePoints(board);
                    return MovementTestUtils.TraceAll<TBitBoard, PieceJLSZRotatabilityLocator<TBitBoard>>(mobJ, spawn, board);
                case Piece.L:
                    var mobL = PieceLMovablePointLocater<TBitBoard>.LocateMovablePoints(board);
                    return MovementTestUtils.TraceAll<TBitBoard, PieceJLSZRotatabilityLocator<TBitBoard>>(mobL, spawn, board);
                case Piece.I:
                    var mobI = PieceIMovablePointLocater<TBitBoard>.LocateMovablePoints(board);
                    var reachedI = MovementTestUtils.TraceAll<TBitBoard, PieceIRotatabilityLocator<TBitBoard>>(mobI, spawn, board);
                    var (upperI, rightI) = PieceIMovablePointLocater<TBitBoard>.MergeToTwoRotationSymmetricMobility(reachedI);
                    return (upperI, rightI, default, default);
                case Piece.S:
                    var mobS = PieceSMovablePointLocater<TBitBoard>.LocateMovablePoints(board);
                    var reachedS = MovementTestUtils.TraceAll<TBitBoard, PieceJLSZRotatabilityLocator<TBitBoard>>(mobS, spawn, board);
                    var (upperS, rightS) = PieceSMovablePointLocater<TBitBoard>.MergeToTwoRotationSymmetricMobility(reachedS);
                    return (upperS, rightS, default, default);
                case Piece.Z:
                    var mobZ = PieceZMovablePointLocater<TBitBoard>.LocateMovablePoints(board);
                    var reachedZ = MovementTestUtils.TraceAll<TBitBoard, PieceJLSZRotatabilityLocator<TBitBoard>>(mobZ, spawn, board);
                    var (upperZ, rightZ) = PieceZMovablePointLocater<TBitBoard>.MergeToTwoRotationSymmetricMobility(reachedZ);
                    return (upperZ, rightZ, default, default);
                default:
                    return default;
            }
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
