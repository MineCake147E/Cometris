using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using Cometris.Benchmarks.PartialBitBoard;
using Cometris.Boards;
using Cometris.Pieces.Mobility;

namespace Cometris.Benchmarks.Running
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            _ = BenchmarkSwitcher
            .FromAssembly(typeof(PartialBitBoardBenchmarks<PartialBitBoard256X2, PartialBitBoard256X2>).Assembly)
            .Run(args, DefaultConfig.Instance.WithSummaryStyle(SummaryStyle.Default.WithMaxParameterColumnWidth(256)).AddDiagnoser(new DisassemblyDiagnoser(new(int.MaxValue)))
            );
            Console.Write("Press any key to exit:");
            _ = Console.ReadKey();
        }
    }
}