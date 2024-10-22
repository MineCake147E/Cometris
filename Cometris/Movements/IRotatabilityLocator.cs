using System.Runtime.CompilerServices;

using Cometris.Boards;

namespace Cometris.Movements
{
    public partial interface IRotatabilityLocator<TSelf, TBitBoard>
        where TSelf : unmanaged, IRotatabilityLocator<TSelf, TBitBoard>
        where TBitBoard : unmanaged, IOperableBitBoard<TBitBoard, ushort>
    {
        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        static virtual (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) RotateAll(
            (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) reached, (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) mobility)
        {
            (var upperReached, var rightReached, var lowerReached, var leftReached) = reached;
            (var upperMobility, var rightMobility, var lowerMobility, var leftMobility) = mobility;
            TBitBoard tempU, tempR, tempD, tempL;
            tempU = TSelf.RotateToUp(upperMobility, leftReached, rightReached);
            tempD = TSelf.RotateToDown(lowerMobility, rightReached, leftReached);
            tempR = TSelf.RotateToRight(rightMobility, upperReached, lowerReached);
            tempL = TSelf.RotateToLeft(leftMobility, lowerReached, upperReached);
            return (tempU, tempR, tempD, tempL);
        }

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        static virtual (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) RotateAllFirstStep(
            TBitBoard spawn, (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) mobility)
        {
            (_, var rightMobility, var lowerMobility, var leftMobility) = mobility;
            TBitBoard tempU = spawn, tempR, tempD, tempL;
            tempR = TSelf.RotateClockwiseFromUp(rightMobility, tempU);
            tempL = TSelf.RotateCounterClockwiseFromUp(leftMobility, tempU);
            tempD = TSelf.RotateToDown(lowerMobility, tempR, tempL);
            return (tempU, tempR, tempD, tempL);
        }
    }
}
