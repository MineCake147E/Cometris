using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

using BenchmarkDotNet.Jobs;

using Cometris.Boards;
using Cometris.Movements.Reachability;
using Cometris.Movements;
using Cometris.Pieces.Mobility;
using System.Runtime.CompilerServices;

namespace Cometris.Benchmarks.Movements.Reachability
{
    [SimpleJob(runtimeMoniker: RuntimeMoniker.HostProcess)]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    //[CategoriesColumn]
    [AllCategoriesFilter(nameof(PieceT))]
    [GenericTypeArguments(typeof(PartialBitBoard256X2), typeof(PartialBitBoard256X2))]
    [GenericTypeArguments(typeof(PartialBitBoard512), typeof(Vector512<ushort>))]
    public class AsymmetricPieceReachablePointLocaterBenchmarks<TBitBoard, TLineMask>
        where TBitBoard : unmanaged, IMaskableBitBoard<TBitBoard, ushort, TLineMask, uint>
        where TLineMask : struct, IEquatable<TLineMask>
    {
        public const int OperationsPerInvoke = 2048;

        #region ArgumentSources
        public static IEnumerable<TBitBoard> PieceTMobilityArgumentsSource()
        {
            yield return TBitBoard.Empty;
            yield return TBitBoard.FromBoard([0b1111111101111111, 0b1111111110111111, 0b1111111100111111, 0b1111111110111111, 0b1111110000111111, 0b1111000001111111, 0b1111000000011111, 0b1111000000011111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111], FullBitBoard.EmptyRow);
            yield return TBitBoard.FromBoard([0b1111101010000111, 0b1111001000000111, 0b1110001000110111, 0b1110000001000111, 0b1110111101001111, 0b1110000001011111, 0b1111001001000111, 0b1111101000100111, 0b1110001011110111, 0b1110011000000111, 0b1110111001001111, 0b1110000001011111, 0b1111001101000111, 0b1111100001100111, 0b1110001001110111, 0b1110011000000111, 0b1110111011001111, 0b1110000001011111, 0b1111000001000111, 0b1111111111100111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111], FullBitBoard.EmptyRow);
        }
        public static IEnumerable<TBitBoard> PieceJMobilityArgumentsSource()
        {
            yield return TBitBoard.Empty;
            yield return TBitBoard.FromBoard([0b1111111110111111, 0b1111111110111111, 0b1111111110111111, 0b1111111110111111, 0b1111111110111111, 0b1111111110111111, 0b1111111110111111, 0b1111111110111111, 0b1111111110111111, 0b1111111110111111, 0b1111111110111111, 0b1111111110111111, 0b1110001100011111, 0b1110111000111111, 0b1110000000001111, 0b1110000000011111, 0b1110000000001111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111], FullBitBoard.EmptyRow);
            yield return TBitBoard.FromBoard([0b1111000011100111, 0b1111001011110111, 0b1111001001110111, 0b1110000001000111, 0b1110101001011111, 0b1111101000001111, 0b1110001011000111, 0b1110111001010111, 0b1110010001010111, 0b1110001101000111, 0b1110101000011111, 0b1111101011001111, 0b1110001001000111, 0b1110110001010111, 0b1110011101110111, 0b1110001000000111, 0b1110101011011111, 0b1111101000001111, 0b1110001000001111, 0b1110111111111111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111], FullBitBoard.EmptyRow);
        }

        public static IEnumerable<TBitBoard> PieceLMobilityArgumentsSource()
        {
            yield return TBitBoard.Empty;
            yield return TBitBoard.FromBoard([0b1110011100001111, 0b1110111101001111, 0b1110111001001111, 0b1110001000000111, 0b1111101001010111, 0b1111000001011111, 0b1110001101000111, 0b1110101001110111, 0b1110101000100111, 0b1110001011000111, 0b1111100001010111, 0b1111001101011111, 0b1110001001000111, 0b1110101000110111, 0b1110111011100111, 0b1110000001000111, 0b1111101101010111, 0b1111000001011111, 0b1111000001000111, 0b1111111111110111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111], FullBitBoard.EmptyRow);
        }

        public static IEnumerable<TBitBoard> PieceSMobilityArgumentsSource()
        {
            yield return TBitBoard.Empty;
            yield return TBitBoard.FromBoard([0b1110000011001111, 0b1111000001100111, 0b1110001001001111, 0b1110111010011111, 0b1110000000001111, 0b1111001001000111, 0b1110011010001111, 0b1111000001100111, 0b1110001001001111, 0b1110111010011111, 0b1110000000001111, 0b1111001001000111, 0b1110011010001111, 0b1111000000100111, 0b1110001001001111, 0b1110111010011111, 0b1110000000001111, 0b1111001000000111, 0b1110011111111111, 0b1111001111111111, 0b1110010000000111, 0b1110000000000111, 0b1110000000000111], FullBitBoard.EmptyRow);
        }

        public static IEnumerable<TBitBoard> PieceZMobilityArgumentsSource()
        {
            yield return TBitBoard.Empty;
            yield return TBitBoard.FromBoard([0b1111001100000111, 0b1110011000001111, 0b1111001001000111, 0b1111100101110111, 0b1111000000000111, 0b1110001001001111, 0b1111000101100111, 0b1110011000001111, 0b1111001001000111, 0b1111100101110111, 0b1111000000000111, 0b1110001001001111, 0b1111000101100111, 0b1110010000001111, 0b1111001001000111, 0b1111100101110111, 0b1111000000000111, 0b1110000001001111, 0b1111111111100111, 0b1111111111001111, 0b1110000000100111, 0b1110000000000111, 0b1110000000000111], FullBitBoard.EmptyRow);
        }
        public static IEnumerable<TBitBoard> PieceIMobilityArgumentsSource()
        {
            yield return TBitBoard.Empty;
            yield return TBitBoard.FromBoard([0b1111111110111111, 0b1111111110111111, 0b1111111110111111, 0b1111111110111111, 0b1111111110111111, 0b1111111110111111, 0b1111111110111111, 0b1111111110111111, 0b1111111110111111, 0b1111111110111111, 0b1111111110111111, 0b1111111110111111, 0b1111111100011111, 0b1111111000111111, 0b1111110000001111, 0b1111100000011111, 0b1111000000001111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111], FullBitBoard.EmptyRow);
            yield return TBitBoard.FromBoard([0b1111111100000111, 0b1111111101110111, 0b1110000001110111, 0b1110111000000111, 0b1110111011100111, 0b1110000011100111, 0b1111110000000111, 0b1111110001110111, 0b1110000001110111, 0b1110111000000111, 0b1110111011100111, 0b1110000011100111, 0b1111110000000111, 0b1111110001110111, 0b1110000001110111, 0b1110111000000111, 0b1110111011100111, 0b1110000011100111, 0b1111111000000111, 0b1111111111110111, 0b1110000000000111, 0b1110000000000111, 0b1110000000000111], FullBitBoard.EmptyRow);
        }
        #endregion

        [BenchmarkCategory(nameof(PieceT))]
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        [ArgumentsSource(nameof(PieceTMobilityArgumentsSource))]
        public (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) PieceT(TBitBoard board)
        {
            var mob = PieceTMovablePointLocater<TBitBoard>.LocateMovablePoints(board);
            var spawn = TBitBoard.CreateSingleLine(0x0100, 20);
            return TraceAllBenchmark<PieceTRotatabilityLocator<TBitBoard>>(mob, spawn);
        }

        [BenchmarkCategory(nameof(PieceJ))]
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        [ArgumentsSource(nameof(PieceJMobilityArgumentsSource))]
        public (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) PieceJ(TBitBoard board)
        {
            var mob = PieceJMovablePointLocater<TBitBoard>.LocateMovablePoints(board);
            var spawn = TBitBoard.CreateSingleLine(0x0100, 20);
            return TraceAllBenchmark<PieceJLSZRotatabilityLocator<TBitBoard>>(mob, spawn);
        }

        [BenchmarkCategory(nameof(PieceL))]
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        [ArgumentsSource(nameof(PieceLMobilityArgumentsSource))]
        public (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) PieceL(TBitBoard board)
        {
            var mob = PieceLMovablePointLocater<TBitBoard>.LocateMovablePoints(board);
            var spawn = TBitBoard.CreateSingleLine(0x0100, 20);
            return TraceAllBenchmark<PieceJLSZRotatabilityLocator<TBitBoard>>(mob, spawn);
        }

        [BenchmarkCategory(nameof(PieceS))]
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        [ArgumentsSource(nameof(PieceSMobilityArgumentsSource))]
        public (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) PieceS(TBitBoard board)
        {
            var mob = PieceSMovablePointLocater<TBitBoard>.LocateSymmetricMovablePoints(board);
            var spawn = TBitBoard.CreateSingleLine(0x0100, 20);
            return TraceAllBenchmark<PieceJLSZRotatabilityLocator<TBitBoard>>(TBitBoard.ConvertVerticalSymmetricToAsymmetricMobility(mob), spawn);
        }

        [BenchmarkCategory(nameof(PieceZ))]
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        [ArgumentsSource(nameof(PieceZMobilityArgumentsSource))]
        public (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) PieceZ(TBitBoard board)
        {
            var mob = PieceZMovablePointLocater<TBitBoard>.LocateSymmetricMovablePoints(board);
            var spawn = TBitBoard.CreateSingleLine(0x0100, 20);
            return TraceAllBenchmark<PieceJLSZRotatabilityLocator<TBitBoard>>(TBitBoard.ConvertVerticalSymmetricToAsymmetricMobility(mob), spawn);
        }

        [BenchmarkCategory(nameof(PieceI))]
        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        [ArgumentsSource(nameof(PieceIMobilityArgumentsSource))]
        public (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) PieceI(TBitBoard board)
        {
            var mob = PieceIMovablePointLocater<TBitBoard>.LocateSymmetricMovablePoints(board);
            var spawn = TBitBoard.CreateSingleLine(0x0100, 20);
            return TraceAllBenchmark<PieceIRotatabilityLocator<TBitBoard>>(TBitBoard.ConvertHorizontalSymmetricToAsymmetricMobility(mob), spawn);
        }

        private static (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) TraceAllBenchmark<TRotatabilityLocator>((TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) mob, TBitBoard spawn)
            where TRotatabilityLocator : unmanaged, IRotatabilityLocator<TRotatabilityLocator, TBitBoard>
        {
            (var upperMobility, var rightMobility, var lowerMobility, var leftMobility) = mob;
            (var upperReached, var rightReached, var lowerReached, var leftReached) = (TBitBoard.Zero, TBitBoard.Zero, TBitBoard.Zero, TBitBoard.Zero);
            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                var (upper, right, lower, left) = TraceAll<TRotatabilityLocator>((upperMobility, rightMobility, lowerMobility, leftMobility), spawn);
                upperReached |= upper;
                rightReached |= right;
                lowerReached |= lower;
                leftReached |= left;
            }
            return (upperReached, rightReached, lowerReached, leftReached);
        }

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) TraceAll<TRotatabilityLocator>((TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) mob, TBitBoard spawn)
            where TRotatabilityLocator : unmanaged, IRotatabilityLocator<TRotatabilityLocator, TBitBoard>
        {
            (_, var rightMobility, _, var leftMobility) = mob;
            var upperReached = spawn;
            var rightReached = TRotatabilityLocator.RotateClockwiseFromUp(rightMobility, upperReached);
            var leftReached = TRotatabilityLocator.RotateCounterClockwiseFromUp(leftMobility, upperReached);
            var lowerReached = TBitBoard.Zero;
            var r2 = (upperReached, rightReached, lowerReached, leftReached);
            var steps = 0;
            do
            {
                steps++;
                (var upperNewBoard, var rightNewBoard, var lowerNewBoard, var leftNewBoard) = AsymmetricPieceReachablePointLocater<TBitBoard, TRotatabilityLocator>.LocateNewReachablePoints(r2, mob);
                var upperDiff = TBitBoard.AndNot(upperReached, upperNewBoard);
                var rightDiff = TBitBoard.AndNot(rightReached, rightNewBoard);
                var lowerDiff = TBitBoard.AndNot(lowerReached, lowerNewBoard);
                var leftDiff = TBitBoard.AndNot(leftReached, leftNewBoard);
                upperReached |= upperNewBoard;
                rightReached |= rightNewBoard;
                lowerReached |= lowerNewBoard;
                leftReached |= leftNewBoard;
                var diffAll = upperDiff | rightDiff;
                r2 = (upperReached, rightReached, lowerReached, leftReached);
                diffAll = TBitBoard.OrAll(diffAll, lowerDiff, leftDiff);
                if (TBitBoard.IsBoardZero(diffAll)) break;
            } while (true);
            return r2;
        }

    }
}
