using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

using Cometris.Boards;
using Cometris.Movements;
using Cometris.Pieces.Mobility;

namespace Cometris.Benchmarks.Movements
{
    [SimpleJob(runtimeMoniker: RuntimeMoniker.HostProcess)]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public partial class RotatabilityLocatorBenchmarks<TBitBoard, TRotatabilityLocator>
        where TRotatabilityLocator : unmanaged, IRotatabilityLocator<TRotatabilityLocator, TBitBoard>
        where TBitBoard : unmanaged, IBitBoard<TBitBoard, ushort>
    {
        private const int OperationsPerInvoke = 4096;

        TBitBoard Board { get; }

        public RotatabilityLocatorBenchmarks()
        {
            ReadOnlySpan<ushort> data = [0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0380, 0x07C0, 0x07C0, 0x07C0, 0x0380, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000];
            Board = TBitBoard.FromBoard(data, TBitBoard.ZeroLine);
        }

        [SkipLocalsInit]
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public TBitBoard RotateAll()
        {
            var board = Board;
            TBitBoard dummy = default;
            for (var i = 0; i < OperationsPerInvoke; i++)
            {
                var (upper, right, lower, left) = TRotatabilityLocator.RotateAll((board, board, board, board), (board, board, board, board));
                dummy |= upper | right;
                dummy |= lower | left;
            }
            return dummy;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public TBitBoard RotateToUp()
        {
            var board = Board;
            TBitBoard dummy = default;
            for (var i = 0; i < OperationsPerInvoke; i++)
            {
                var result = TRotatabilityLocator.RotateToUp(board, board, board);
                dummy |= result;
            }
            return dummy;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public TBitBoard RotateToRight()
        {
            var board = Board;
            TBitBoard dummy = default;
            for (var i = 0; i < OperationsPerInvoke; i++)
            {
                var result = TRotatabilityLocator.RotateToRight(board, board, board);
                dummy |= result;
            }
            return dummy;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public TBitBoard RotateToDown()
        {
            var board = Board;
            TBitBoard dummy = default;
            for (var i = 0; i < OperationsPerInvoke; i++)
            {
                var result = TRotatabilityLocator.RotateToDown(board, board, board);
                dummy |= result;
            }
            return dummy;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public TBitBoard RotateToLeft()
        {
            var board = Board;
            TBitBoard dummy = default;
            for (var i = 0; i < OperationsPerInvoke; i++)
            {
                var result = TRotatabilityLocator.RotateToLeft(board, board, board);
                dummy |= result;
            }
            return dummy;
        }

    }
}
