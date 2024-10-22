using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;

using BenchmarkDotNet.Jobs;

using Cometris.Boards;
using Cometris.Pieces.Placing;

namespace Cometris.Benchmarks.Pieces.Placing
{
    [SimpleJob(runtimeMoniker: RuntimeMoniker.HostProcess)]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    //[AllCategoriesFilter(nameof(PlaceRight))]
    public partial class PiecePlacerBenchmarks<TPiecePlacer, TBitBoard, TLineMask>
        where TPiecePlacer : unmanaged, IPiecePlacer<TBitBoard>
        where TBitBoard : unmanaged, ICompactMaskableBitBoard<TBitBoard, ushort, TLineMask, uint>
        where TLineMask : struct, IEquatable<TLineMask>
    {
        public const int OperationsPerInvoke = 16384;

        [BenchmarkCategory(nameof(PlaceUp))]
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public TBitBoard PlaceUp()
        {
            var a = TBitBoard.Zero;
            var x = 0;
            var y = 0;
            for (var i = 0; i < OperationsPerInvoke / 2; i++)
            {
                var xv = x >> 3;
                a ^= TPiecePlacer.PlaceUp(xv, y);
                a ^= TPiecePlacer.PlaceUp(xv + 1, y + 1);
                y = 31 & (y + 1);
                x = (byte)(x + 1);
            }
            return a;
        }

        [BenchmarkCategory(nameof(PlaceRight))]
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public TBitBoard PlaceRight()
        {
            var a = TBitBoard.Zero;
            var x = 0;
            var y = 0;
            for (var i = 0; i < OperationsPerInvoke / 2; i++)
            {
                var xv = x >> 3;
                a ^= TPiecePlacer.PlaceRight(xv, y);
                a ^= TPiecePlacer.PlaceRight(xv + 1, y + 1);
                y = 31 & (y + 1);
                x = (byte)(x + 1);
            }
            return a;
        }

        [BenchmarkCategory(nameof(PlaceDown))]
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public TBitBoard PlaceDown()
        {
            var a = TBitBoard.Zero;
            var x = 0;
            var y = 0;
            for (var i = 0; i < OperationsPerInvoke / 2; i++)
            {
                var xv = x >> 3;
                a ^= TPiecePlacer.PlaceDown(xv, y);
                a ^= TPiecePlacer.PlaceDown(xv + 1, y + 1);
                y = 31 & (y + 1);
                x = (byte)(x + 1);
            }
            return a;
        }

        [BenchmarkCategory(nameof(PlaceLeft))]
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public TBitBoard PlaceLeft()
        {
            var a = TBitBoard.Zero;
            var x = 0;
            var y = 0;
            for (var i = 0; i < OperationsPerInvoke / 2; i++)
            {
                var xv = x >> 3;
                a ^= TPiecePlacer.PlaceLeft(xv, y);
                a ^= TPiecePlacer.PlaceLeft(xv + 1, y + 1);
                y = 31 & (y + 1);
                x = (byte)(x + 1);
            }
            return a;
        }
    }
}
