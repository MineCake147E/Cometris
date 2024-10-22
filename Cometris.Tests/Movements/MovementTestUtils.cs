using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cometris.Boards;
using Cometris.Movements;
using Cometris.Movements.Reachability;

namespace Cometris.Tests.Movements
{
    internal static class MovementTestUtils
    {
        internal static (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) TraceAll<TBitBoard, TRotatabilityLocator>((TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) mob, TBitBoard spawn, TBitBoard background, bool dumpSteps = false)
            where TBitBoard : unmanaged, IOperableBitBoard<TBitBoard, ushort>
            where TRotatabilityLocator : unmanaged, IRotatabilityLocator<TRotatabilityLocator, TBitBoard>
        {
            var reached = AsymmetricPieceReachablePointLocater<TBitBoard, TRotatabilityLocator>.LocateReachablePointsFirstStep(spawn, mob);
            TBitBoard diffAll;
            (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) newBoards, diff = reached;
            var steps = 0;
            if (dumpSteps)
            {
                Console.WriteLine($"Step {steps}");
                Console.WriteLine(BitBoardUtils.VisualizeOrientations(reached));
            }
            do
            {
                steps++;
                newBoards = AsymmetricPieceReachablePointLocater<TBitBoard, TRotatabilityLocator>.LocateNewReachablePoints(reached, mob);
                if (dumpSteps) Console.WriteLine($"\nStep {steps} Difference:");
                diff.upper = newBoards.upper & ~reached.upper;
                diff.right = newBoards.right & ~reached.right;
                diff.lower = newBoards.lower & ~reached.lower;
                diff.left = newBoards.left & ~reached.left;
                if (dumpSteps) Console.WriteLine(BitBoardUtils.VisualizeOrientations(diff, background));
                reached.upper |= newBoards.upper;
                reached.right |= newBoards.right;
                reached.lower |= newBoards.lower;
                reached.left |= newBoards.left;
                diffAll = diff.upper | diff.right | (diff.lower | diff.left);
            } while (diffAll != TBitBoard.Zero);
            if (dumpSteps)
            {
                Console.WriteLine($"\nTotal Steps: {steps}");
                Console.WriteLine(BitBoardUtils.VisualizeOrientations(reached, background));
            }
            return reached;
        }

        internal static TBitBoard TraceAllSymmetric<TBitBoard>(TBitBoard mob, TBitBoard spawn, TBitBoard background, bool dumpSteps = false)
            where TBitBoard : unmanaged, IOperableBitBoard<TBitBoard, ushort>
        {
            var reached = SymmetricPieceReachablePointLocater<TBitBoard>.LocateReachablePointsFirstStep(spawn, mob);
            TBitBoard diffAll;
            TBitBoard newBoards;
            var steps = 0;
            if (dumpSteps)
            {
                Console.WriteLine($"Step {steps}");
                Console.WriteLine(reached.ToString());
            }
            do
            {
                steps++;
                newBoards = SymmetricPieceReachablePointLocater<TBitBoard>.LocateNewReachablePoints(reached, mob);
                if (dumpSteps) Console.WriteLine($"\nStep {steps} Difference:");
                var diff = newBoards & ~reached;
                if (dumpSteps) Console.WriteLine(diff.ToString());
                reached |= newBoards;
                diffAll = diff;
            } while (diffAll != TBitBoard.Zero);
            if (dumpSteps)
            {
                Console.WriteLine($"\nTotal Steps: {steps}");
                Console.WriteLine(reached.ToString());
            }
            return reached;
        }


    }
}
