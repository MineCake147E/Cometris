using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics;
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
    [GenericTypeArguments(typeof(PartialBitBoard256X2), typeof(PartialBitBoard256X2))]
    [GenericTypeArguments(typeof(PartialBitBoard512), typeof(Vector512<ushort>))]
    public class LocateAllBlocksBenchmarks<TBitBoard, TLineMask>
        where TBitBoard : unmanaged, ICompactMaskableBitBoard<TBitBoard, ushort, TLineMask, uint>
        where TLineMask : struct, IEquatable<TLineMask>
    {
        public const int OperationsPerInvoke = 16384;

    }
}
