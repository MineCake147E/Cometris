using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

using Cometris.Pieces.Permutation;

namespace Cometris.Benchmarks.Pieces.Permutation
{
    [SimpleJob(runtimeMoniker: RuntimeMoniker.HostProcess)]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public class CalculatePermutationBenchmarks
    {
        private uint id = 0;

        public const int OperationsPerInvoke = 16384;

        private static uint IncrementId(uint id, uint increment)
        {
            var s = id;
            s += increment;
            s = uint.Min(s - 5040, s);
            return s;
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public uint CalculatePermutationBmi2()
        {
            var s = id;
            var res = 0u;
            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                res = PiecePermutationUtils.CalculatePermutationBmi2((ushort)s);
                s = IncrementId(s, (res & 1) + 1);
            }
            id = s;
            return res;
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public uint CalculatePermutationFallback()
        {
            var s = id;
            var res = 0u;
            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                res = PiecePermutationUtils.CalculatePermutationFallback((ushort)s);
                s = IncrementId(s, (res & 1) + 1);
            }
            id = s;
            return res;
        }
    }
}
