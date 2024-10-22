using System.Runtime.CompilerServices;

using Cometris.Boards;
using Cometris.Pieces.Mobility;

namespace Cometris.Movements.Reachability
{
    public readonly struct AsymmetricPieceReachablePointLocater<TBitBoard, TRotatabilityLocator> : IAsymmetricPieceReachablePointLocater<TBitBoard>
        where TBitBoard : unmanaged, IOperableBitBoard<TBitBoard, ushort>
        where TRotatabilityLocator : unmanaged, IRotatabilityLocator<TRotatabilityLocator, TBitBoard>
    {
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) LocateReachablePointsFirstStep(TBitBoard spawn, (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) mobilityBoards)
        {
            // Spawn
            TBitBoard tempU = spawn & mobilityBoards.upper, tempR, tempL;
            var rightMobility = mobilityBoards.right;
            var leftMobility = mobilityBoards.left;
            // Initial Rotation
            tempR = TRotatabilityLocator.RotateClockwiseFromUp(rightMobility, tempU);
            tempL = TRotatabilityLocator.RotateCounterClockwiseFromUp(leftMobility, tempU);
            return (tempU, tempR, TBitBoard.Zero, tempL);
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) LocateNewReachablePoints(in (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) reached, in (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) mobilityBoards)
        {
            var (upperReached, rightReached, lowerReached, leftReached) = reached;
            var (upperMobility, rightMobility, lowerMobility, leftMobility) = mobilityBoards;
            TBitBoard tempU, tempR, tempD, tempL;
            // Rotation
            tempU = upperReached | TRotatabilityLocator.RotateToUp(upperMobility, leftReached, rightReached);
            tempD = lowerReached | TRotatabilityLocator.RotateToDown(lowerMobility, rightReached, leftReached);
            tempR = rightReached | TRotatabilityLocator.RotateToRight(rightMobility, upperReached, lowerReached);
            tempL = leftReached | TRotatabilityLocator.RotateToLeft(leftMobility, lowerReached, upperReached);
            // Horizontal Movement
            (tempU, tempR, tempD, tempL) = TBitBoard.FillHorizontalReachable4Sets((upperMobility, rightMobility, lowerMobility, leftMobility), (tempU, tempR, tempD, tempL));
            // Vertical Movement
            (tempU, tempR, tempD, tempL) = TBitBoard.FillDropReachable4Sets((upperMobility, rightMobility, lowerMobility, leftMobility), (tempU, tempR, tempD, tempL));
            return (tempU, tempR, tempD, tempL);
        }

        public static (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) LocateHardDropReachablePoints(TBitBoard spawn, (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) mobilityBoards)
        {
            var upperMobility = mobilityBoards.upper;
            var rightMobility = mobilityBoards.right;
            var lowerMobility = mobilityBoards.lower;
            var leftMobility = mobilityBoards.left;
            // Initial Rotation
            TBitBoard tempU = spawn, tempR, tempD, tempL;
            tempR = TRotatabilityLocator.RotateClockwiseFromUp(rightMobility, spawn);
            tempL = TRotatabilityLocator.RotateCounterClockwiseFromUp(leftMobility, spawn);
            tempD = TRotatabilityLocator.RotateToDown(lowerMobility, tempR, tempL);
            // Horizontal Movement
            (tempU, tempR, tempD, tempL) = TBitBoard.FillHorizontalReachable4Sets((upperMobility, rightMobility, lowerMobility, leftMobility), (tempU, tempR, tempD, tempL));
            // Vertical Movement
            (tempU, tempR, tempD, tempL) = TBitBoard.FillDropReachable4Sets((upperMobility, rightMobility, lowerMobility, leftMobility), (tempU, tempR, tempD, tempL));
            return (tempU, tempR, tempD, tempL);
        }
    }

    public readonly struct TwoRotationSymmetricPieceReachablePointLocater<TBitBoard, TRotatabilityLocator, TPieceMovablePointLocater> : ITwoRotationSymmetricPieceReachablePointLocater<TBitBoard>
        where TBitBoard : unmanaged, IOperableBitBoard<TBitBoard, ushort>
        where TRotatabilityLocator : unmanaged, IRotatabilityLocator<TRotatabilityLocator, TBitBoard>
        where TPieceMovablePointLocater : ITwoRotationSymmetricPieceMovablePointLocater<TBitBoard>
    {
        public static (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) LocateNewReachablePoints((TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) reached, (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) mobilityBoards)
            => AsymmetricPieceReachablePointLocater<TBitBoard, TRotatabilityLocator>.LocateNewReachablePoints(reached, mobilityBoards);

        public static (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) LocateHardDropReachablePoints(TBitBoard spawn, (TBitBoard upper, TBitBoard right) mobilityBoards)
                    => AsymmetricPieceReachablePointLocater<TBitBoard, TRotatabilityLocator>.LocateHardDropReachablePoints(spawn, TPieceMovablePointLocater.ConvertToAsymmetricMobility(mobilityBoards));
        public static (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) LocateReachablePointsFirstStep(TBitBoard spawn, (TBitBoard upper, TBitBoard right) mobilityBoards)
            => AsymmetricPieceReachablePointLocater<TBitBoard, TRotatabilityLocator>.LocateReachablePointsFirstStep(spawn, TPieceMovablePointLocater.ConvertToAsymmetricMobility(mobilityBoards));
    }
}
