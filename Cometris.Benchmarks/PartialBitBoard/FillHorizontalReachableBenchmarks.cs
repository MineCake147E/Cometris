using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

using Cometris.Boards;

namespace Cometris.Benchmarks.PartialBitBoard
{
    [SimpleJob(runtimeMoniker: RuntimeMoniker.HostProcess)]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    [CategoriesColumn]
    [AllCategoriesFilter(nameof(FillHorizontalReachable4Sets))]
    public class FillHorizontalReachableBenchmarks
    {
        public const int OperationsPerInvoke = 16384;

        [BenchmarkCategory(nameof(FillHorizontalReachable))]
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public PartialBitBoard512 FillHorizontalReachable()
        {
            var board0 = ~PartialBitBoard512.Empty;
            var board1 = PartialBitBoard512.Zero;
            var start0 = new PartialBitBoard512(0x01);
            for (var i = 0; i < OperationsPerInvoke / 2; i++)
            {
                board1 ^= PartialBitBoard512.FillHorizontalReachable(board0, start0);
                start0 = PartialBitBoard512.ShiftDownOneLine(start0, start0);
                board1 ^= PartialBitBoard512.FillHorizontalReachable(board0, start0);
                start0 = PartialBitBoard512.ShiftDownOneLine(start0, start0);
            }
            return board1;
        }

        [BenchmarkCategory(nameof(FillHorizontalReachable))]
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public PartialBitBoard512 FillHorizontalReachableOld()
        {
            var board0 = ~PartialBitBoard512.Empty;
            var board1 = PartialBitBoard512.Zero;
            var start0 = new PartialBitBoard512(0x01);
            for (var i = 0; i < OperationsPerInvoke / 2; i++)
            {
                board1 ^= PartialBitBoard512.FillHorizontalReachableInternalOld(board0, start0);
                start0 = PartialBitBoard512.ShiftDownOneLine(start0, start0);
                board1 ^= PartialBitBoard512.FillHorizontalReachableInternalOld(board0, start0);
                start0 = PartialBitBoard512.ShiftDownOneLine(start0, start0);
            }
            return board1;
        }

        [BenchmarkCategory(nameof(FillHorizontalReachable4Sets))]
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public (PartialBitBoard512, PartialBitBoard512, PartialBitBoard512, PartialBitBoard512) FillHorizontalReachable4Sets()
        {
            var board0 = PartialBitBoard512.InvertedEmpty;
            var board1 = PartialBitBoard512.Zero;
            var board2 = PartialBitBoard512.InvertedEmpty;
            var board3 = PartialBitBoard512.Zero;
            var start0 = new PartialBitBoard512(0x02);
            var start1 = new PartialBitBoard512(0x01);
            var start2 = new PartialBitBoard512(0x08);
            var start3 = new PartialBitBoard512(0x04);
            for (var i = 0; i < OperationsPerInvoke / 2; i++)
            {
                (board0, board1, board2, board3) = PartialBitBoard512.FillHorizontalReachable4Sets((board0, board1, board2, board3), (start0, start1, start2, start3));
                start0 = PartialBitBoard512.ShiftDownOneLine(start0, start0);
                start1 = PartialBitBoard512.ShiftDownOneLine(start1, start1);
                start2 = PartialBitBoard512.ShiftDownOneLine(start2, start2);
                start3 = PartialBitBoard512.ShiftDownOneLine(start3, start3);
                (board0, board1, board2, board3) = PartialBitBoard512.FillHorizontalReachable4Sets((board0, board1, board2, board3), (start0, start1, start2, start3));
                start0 = PartialBitBoard512.ShiftDownOneLine(start0, start0);
                start1 = PartialBitBoard512.ShiftDownOneLine(start1, start1);
                start2 = PartialBitBoard512.ShiftDownOneLine(start2, start2);
                start3 = PartialBitBoard512.ShiftDownOneLine(start3, start3);
            }
            return (board0, board1, board2, board3);
        }

        [BenchmarkCategory(nameof(FillHorizontalReachable4Sets))]
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public (PartialBitBoard512, PartialBitBoard512, PartialBitBoard512, PartialBitBoard512) FillHorizontalReachable4SetsOld()
        {
            var board0 = PartialBitBoard512.InvertedEmpty;
            var board1 = PartialBitBoard512.Zero;
            var board2 = PartialBitBoard512.InvertedEmpty;
            var board3 = PartialBitBoard512.Zero;
            var start0 = new PartialBitBoard512(0x02);
            var start1 = new PartialBitBoard512(0x01);
            var start2 = new PartialBitBoard512(0x08);
            var start3 = new PartialBitBoard512(0x04);
            for (var i = 0; i < OperationsPerInvoke / 2; i++)
            {
                (board0, board1, board2, board3) = PartialBitBoard512.FillHorizontalReachable4SetsOld((board0, board1, board2, board3), (start0, start1, start2, start3));
                start0 = PartialBitBoard512.ShiftDownOneLine(start0, start0);
                start1 = PartialBitBoard512.ShiftDownOneLine(start1, start1);
                start2 = PartialBitBoard512.ShiftDownOneLine(start2, start2);
                start3 = PartialBitBoard512.ShiftDownOneLine(start3, start3);
                (board0, board1, board2, board3) = PartialBitBoard512.FillHorizontalReachable4SetsOld((board0, board1, board2, board3), (start0, start1, start2, start3));
                start0 = PartialBitBoard512.ShiftDownOneLine(start0, start0);
                start1 = PartialBitBoard512.ShiftDownOneLine(start1, start1);
                start2 = PartialBitBoard512.ShiftDownOneLine(start2, start2);
                start3 = PartialBitBoard512.ShiftDownOneLine(start3, start3);
            }
            return (board0, board1, board2, board3);
        }

        [BenchmarkCategory(nameof(FillHorizontalReachable4SetsNonTuple))]
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public (PartialBitBoard512, PartialBitBoard512, PartialBitBoard512, PartialBitBoard512) FillHorizontalReachable4SetsNonTuple()
        {
            var board0 = PartialBitBoard512.InvertedEmpty;
            var board1 = PartialBitBoard512.Zero;
            var board2 = PartialBitBoard512.InvertedEmpty;
            var board3 = PartialBitBoard512.Zero;
            var start0 = new PartialBitBoard512(0x02);
            var start1 = new PartialBitBoard512(0x01);
            var start2 = new PartialBitBoard512(0x08);
            var start3 = new PartialBitBoard512(0x04);
            for (var i = 0; i < OperationsPerInvoke / 2; i++)
            {
                (board0, board1, board2, board3) = PartialBitBoard512.FillHorizontalReachable4Sets(board0, board1, board2, board3, start0, start1, start2, start3);
                start0 = PartialBitBoard512.ShiftDownOneLine(start0, start0);
                start1 = PartialBitBoard512.ShiftDownOneLine(start1, start1);
                start2 = PartialBitBoard512.ShiftDownOneLine(start2, start2);
                start3 = PartialBitBoard512.ShiftDownOneLine(start3, start3);
                (board0, board1, board2, board3) = PartialBitBoard512.FillHorizontalReachable4Sets(board0, board1, board2, board3, start0, start1, start2, start3);
                start0 = PartialBitBoard512.ShiftDownOneLine(start0, start0);
                start1 = PartialBitBoard512.ShiftDownOneLine(start1, start1);
                start2 = PartialBitBoard512.ShiftDownOneLine(start2, start2);
                start3 = PartialBitBoard512.ShiftDownOneLine(start3, start3);
            }
            return (board0, board1, board2, board3);
        }

        [BenchmarkCategory(nameof(FillHorizontalReachable4SetsNonTuple))]
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public (PartialBitBoard512, PartialBitBoard512, PartialBitBoard512, PartialBitBoard512) FillHorizontalReachable4SetsOldNonTuple()
        {
            var board0 = PartialBitBoard512.InvertedEmpty;
            var board1 = PartialBitBoard512.Zero;
            var board2 = PartialBitBoard512.InvertedEmpty;
            var board3 = PartialBitBoard512.Zero;
            var start0 = new PartialBitBoard512(0x02);
            var start1 = new PartialBitBoard512(0x01);
            var start2 = new PartialBitBoard512(0x08);
            var start3 = new PartialBitBoard512(0x04);
            for (var i = 0; i < OperationsPerInvoke / 2; i++)
            {
                (board0, board1, board2, board3) = PartialBitBoard512.FillHorizontalReachable4SetsOld(board0, board1, board2, board3, start0, start1, start2, start3);
                start0 = PartialBitBoard512.ShiftDownOneLine(start0, start0);
                start1 = PartialBitBoard512.ShiftDownOneLine(start1, start1);
                start2 = PartialBitBoard512.ShiftDownOneLine(start2, start2);
                start3 = PartialBitBoard512.ShiftDownOneLine(start3, start3);
                (board0, board1, board2, board3) = PartialBitBoard512.FillHorizontalReachable4SetsOld(board0, board1, board2, board3, start0, start1, start2, start3);
                start0 = PartialBitBoard512.ShiftDownOneLine(start0, start0);
                start1 = PartialBitBoard512.ShiftDownOneLine(start1, start1);
                start2 = PartialBitBoard512.ShiftDownOneLine(start2, start2);
                start3 = PartialBitBoard512.ShiftDownOneLine(start3, start3);
            }
            return (board0, board1, board2, board3);
        }
    }
}
