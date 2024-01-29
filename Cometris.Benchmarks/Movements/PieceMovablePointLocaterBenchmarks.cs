using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

using Cometris.Boards;
using Cometris.Pieces.Mobility;

namespace Cometris.Benchmarks
{
    [SimpleJob(runtimeMoniker: RuntimeMoniker.HostProcess)]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public partial class PieceMovablePointLocaterBenchmarks<TBitBoard, TPieceMovablePointLocater>
        where TPieceMovablePointLocater : IAsymmetricPieceMovablePointLocater<TBitBoard>
        where TBitBoard : unmanaged, IBitBoard<TBitBoard, ushort>
    {
        private const int OperationsPerInvoke = 2048;

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public TBitBoard LocateMovablePoints()
        {
            ReadOnlySpan<ushort> data = [0xE007, 0xE007, 0xE7FF, 0xE007, 0xE007, 0xFFE7, 0xE007, 0xE007, 0xE7FF, 0xE007, 0xE007, 0xFFE7, 0xE007, 0xE007, 0xE7FF, 0xE007, 0xE007, 0xFFE7, 0xE007, 0xE007, 0xE7FF, 0xE007, 0xE007, 0xE007, 0xE007, 0xE007, 0xE007, 0xE007, 0xE007, 0xE007, 0xE007, 0xE007];
            var board = TBitBoard.FromBoard(data, TBitBoard.ZeroLine);
            TBitBoard dummy = default;
            for (var i = 0; i < OperationsPerInvoke; i++)
            {
                var (upper, right, lower, left) = TPieceMovablePointLocater.LocateMovablePoints(board);
                dummy |= upper;
                dummy |= right;
                dummy |= lower;
                dummy |= left;
            }
            return dummy;
        }
    }
}
