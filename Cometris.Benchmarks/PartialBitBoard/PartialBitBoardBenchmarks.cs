using System.Numerics;
using System.Runtime.Intrinsics;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

using Cometris.Boards;

namespace Cometris.Benchmarks.PartialBitBoard
{
    [SimpleJob(runtimeMoniker: RuntimeMoniker.HostProcess)]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    [CategoriesColumn]
    [AllCategoriesFilter(nameof(FillDropReachable4Sets))]
    [GenericTypeArguments(typeof(PartialBitBoard256X2), typeof(PartialBitBoard256X2))]
    [GenericTypeArguments(typeof(PartialBitBoard512), typeof(Vector512<ushort>))]
    public class PartialBitBoardBenchmarks<TBitBoard, TLineMask>
        where TBitBoard : unmanaged, IMaskableBitBoard<TBitBoard, ushort, TLineMask, uint>
        where TLineMask : struct, IEquatable<TLineMask>
    {
        public PartialBitBoardBenchmarks() { }
        public const int OperationsPerInvoke = 16384;
        #region Advanced

        [BenchmarkCategory(nameof(ClearLines))]
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public TBitBoard ClearLines()
        {
            var board0 = TBitBoard.AllBitsSet;
            var board1 = TBitBoard.AllBitsSet;
            var mask0 = TBitBoard.AllBitsSetMask;
            var mask1 = TBitBoard.CreateMaskFromBoard(TBitBoard.FromBoard([ushort.MaxValue, 0, ushort.MaxValue, 0, ushort.MaxValue, 0, ushort.MaxValue, 0, ushort.MaxValue, 0, ushort.MaxValue, 0, ushort.MaxValue, 0, ushort.MaxValue, 0, ushort.MaxValue, 0, ushort.MaxValue, 0, ushort.MaxValue, 0, ushort.MaxValue, 0, ushort.MaxValue, 0, ushort.MaxValue, 0, ushort.MaxValue, 0, ushort.MaxValue, 0], 0));
            var mask2 = TBitBoard.ZeroMask;
            var emptyLine = TBitBoard.EmptyLine;
            var t = emptyLine | (uint)emptyLine << 16;
            for (var i = 0; i < OperationsPerInvoke / 2; i++)
            {
                emptyLine = (ushort)t;
                board0 = TBitBoard.ClearLines(board0, emptyLine, mask0);
                board1 = TBitBoard.ClearLines(board1, emptyLine, mask1);
                (mask0, mask1, mask2) = (mask1, mask2, mask0);
                t = BitOperations.RotateRight(t, 7);
            }
            return board0 ^ board1;
        }

        [BenchmarkCategory(nameof(ShiftUpOneLine))]
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public TBitBoard ShiftUpOneLine()
        {
            var board0 = TBitBoard.Empty;
            var board1 = board0;
            var board2 = board0;
            var board3 = board0;
            var empty = TBitBoard.Empty;
            for (var i = 0; i < OperationsPerInvoke / 8; i++)
            {
                board0 = TBitBoard.ShiftUpOneLine(board0, empty);
                board1 = TBitBoard.ShiftUpOneLine(board1, empty);
                board2 = TBitBoard.ShiftUpOneLine(board2, empty);
                board3 = TBitBoard.ShiftUpOneLine(board3, empty);
                board0 = TBitBoard.ShiftUpOneLine(board0, empty);
                board1 = TBitBoard.ShiftUpOneLine(board1, empty);
                board2 = TBitBoard.ShiftUpOneLine(board2, empty);
                board3 = TBitBoard.ShiftUpOneLine(board3, empty);
            }
            board0 ^= board1;
            board2 ^= board3;
            return board0 ^ board2;
        }

        [BenchmarkCategory(nameof(FillDropReachable))]
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public TBitBoard FillDropReachable()
        {
            var board0 = TBitBoard.InvertedEmpty;
            var board1 = TBitBoard.Zero;
            var start0 = TBitBoard.CreateSingleLine(TBitBoard.InvertedEmptyLine, 31);
            for (var i = 0; i < OperationsPerInvoke / 2; i++)
            {
                board1 ^= TBitBoard.FillDropReachable(board0, start0);
                start0 = TBitBoard.ShiftDownOneLine(start0, start0);
                board1 ^= TBitBoard.FillDropReachable(board0, start0);
                start0 = TBitBoard.ShiftDownOneLine(start0, start0);
            }
            return board1;
        }

        [BenchmarkCategory(nameof(FillDropReachable4Sets))]
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public (TBitBoard, TBitBoard, TBitBoard, TBitBoard) FillDropReachable4Sets()
        {
            var board0 = TBitBoard.InvertedEmpty;
            var board1 = TBitBoard.Zero;
            var board2 = TBitBoard.InvertedEmpty;
            var board3 = TBitBoard.Zero;
            var start0 = TBitBoard.CreateSingleLine(TBitBoard.InvertedEmptyLine, 31);
            var start1 = TBitBoard.CreateSingleLine(TBitBoard.InvertedEmptyLine, 30);
            var start2 = TBitBoard.CreateSingleLine(TBitBoard.InvertedEmptyLine, 32);
            var start3 = TBitBoard.CreateSingleLine(TBitBoard.InvertedEmptyLine, 29);
            for (var i = 0; i < OperationsPerInvoke / 2; i++)
            {
                (board0, board1, board2, board3) = TBitBoard.FillDropReachable4Sets((board0, board1, board2, board3), (start0, start1, start2, start3));
                start0 = TBitBoard.ShiftDownOneLine(start0, start0);
                start1 = TBitBoard.ShiftDownOneLine(start1, start1);
                start2 = TBitBoard.ShiftDownOneLine(start2, start2);
                start3 = TBitBoard.ShiftDownOneLine(start3, start3);
                (board0, board1, board2, board3) = TBitBoard.FillDropReachable4Sets((board0, board1, board2, board3), (start0, start1, start2, start3));
                start0 = TBitBoard.ShiftDownOneLine(start0, start0);
                start1 = TBitBoard.ShiftDownOneLine(start1, start1);
                start2 = TBitBoard.ShiftDownOneLine(start2, start2);
                start3 = TBitBoard.ShiftDownOneLine(start3, start3);
            }
            return (board0, board1, board2, board3);
        }

        [BenchmarkCategory(nameof(FillDropReachable4Sets))]
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public (TBitBoard, TBitBoard, TBitBoard, TBitBoard) FillDropReachable4SetsNonTuple()
        {
            var board0 = TBitBoard.InvertedEmpty;
            var board1 = TBitBoard.Zero;
            var board2 = TBitBoard.InvertedEmpty;
            var board3 = TBitBoard.Zero;
            var start0 = TBitBoard.CreateSingleLine(TBitBoard.InvertedEmptyLine, 31);
            var start1 = TBitBoard.CreateSingleLine(TBitBoard.InvertedEmptyLine, 30);
            var start2 = TBitBoard.CreateSingleLine(TBitBoard.InvertedEmptyLine, 32);
            var start3 = TBitBoard.CreateSingleLine(TBitBoard.InvertedEmptyLine, 29);
            for (var i = 0; i < OperationsPerInvoke / 2; i++)
            {
                (board0, board1, board2, board3) = TBitBoard.FillDropReachable4Sets(board0, board1, board2, board3, start0, start1, start2, start3);
                start0 = TBitBoard.ShiftDownOneLine(start0, start0);
                start1 = TBitBoard.ShiftDownOneLine(start1, start1);
                start2 = TBitBoard.ShiftDownOneLine(start2, start2);
                start3 = TBitBoard.ShiftDownOneLine(start3, start3);
                (board0, board1, board2, board3) = TBitBoard.FillDropReachable4Sets(board0, board1, board2, board3, start0, start1, start2, start3);
                start0 = TBitBoard.ShiftDownOneLine(start0, start0);
                start1 = TBitBoard.ShiftDownOneLine(start1, start1);
                start2 = TBitBoard.ShiftDownOneLine(start2, start2);
                start3 = TBitBoard.ShiftDownOneLine(start3, start3);
            }
            return (board0, board1, board2, board3);
        }

        #endregion
        #region Primitive
        [BenchmarkCategory(nameof(UnaryNegation))]
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public TBitBoard UnaryNegation()
        {
            var board0 = TBitBoard.Empty;
            var board1 = board0;
            var board2 = board0;
            for (var i = 0; i < OperationsPerInvoke / 8; i++)
            {
                board0 = ~board2;
                board1 = ~board0;
                board2 = ~board1;
                board0 = ~board2;
                board1 = ~board0;
                board2 = ~board1;
                board0 = ~board2;
                board1 = ~board0;
            }
            return board1;
        }
        #endregion
        #region Utils
        [BenchmarkCategory(nameof(WithLine))]
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public TBitBoard WithLine()
        {
            var board0 = TBitBoard.Empty;
            var board1 = board0;
            var board2 = board0;
            var board3 = board0;
            ushort line0 = 0, line1 = 1, line2 = 2, line3 = 3;
            int index = 0;
            ulong rng = 0;
            for (var i = 0; i < OperationsPerInvoke / 8; i++)
            {
                rng = rng * 9 + 127;
                board0 = board0.WithLine(line0, index);
                board1 = board1.WithLine(line1, index + 1);
                board2 = board2.WithLine(line2, index + 2);
                board3 = board3.WithLine(line3, index + 3);
                board0 = board0.WithLine(line1, index + 4);
                board1 = board1.WithLine(line2, index + 5);
                board2 = board2.WithLine(line3, index + 6);
                board3 = board3.WithLine(line0, index + 7);
                index = (int)((rng >> 43) & 31);
                var v = rng >> 48;
                line0 += (ushort)v;
                line1 += (ushort)v;
                line2 += (ushort)v;
                line3 += (ushort)v;
            }
            board0 ^= board1;
            board2 ^= board3;
            return board0 ^ board2;
        }

        public IEnumerable<int> FromBoardLengthSource() => [0, 1, 2, 3, 4, 7, 8, 15, 16, 31, 32];

        [ArgumentsSource(nameof(FromBoardLengthSource))]
        [BenchmarkCategory(nameof(FromBoard))]
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public TBitBoard FromBoard(int length)
        {
            ReadOnlySpan<ushort> boardSource = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31];
            var bslice = boardSource.Slice(0, length);
            var board0 = TBitBoard.Empty;
            for (var i = 0; i < OperationsPerInvoke; i++)
            {
                board0 ^= TBitBoard.FromBoard(bslice, TBitBoard.EmptyLine);
            }
            return board0;
        }
        #endregion
    }
}
