using System.Runtime.CompilerServices;

using Cometris.Boards;

namespace Cometris.Movements.Reachability
{
    public interface IAsymmetricPieceReachablePointLocater<TBitBoard> where TBitBoard : unmanaged, IBitBoard<TBitBoard, ushort>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) LocateReachablePointsFirstStep(TBitBoard spawn, (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) mobilityBoards);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) LocateHardDropReachablePoints(TBitBoard spawn, (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) mobilityBoards);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) LocateNewReachablePoints(in (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) reached, in (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) mobilityBoards);
    }

    public interface ITwoRotationSymmetricPieceReachablePointLocater<TBitBoard> where TBitBoard : unmanaged, IBitBoard<TBitBoard, ushort>
    {
        // Angle matters when it comes to reachability.
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) LocateReachablePointsFirstStep(TBitBoard spawn, (TBitBoard upper, TBitBoard right) mobilityBoards);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) LocateHardDropReachablePoints(TBitBoard spawn, (TBitBoard upper, TBitBoard right) mobilityBoards);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) LocateNewReachablePoints((TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) reached, (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) mobilityBoards);
    }
    public interface ISymmetricPieceReachablePointLocater<TBitBoard> where TBitBoard : unmanaged, IBitBoard<TBitBoard, ushort>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract TBitBoard LocateReachablePointsFirstStep(TBitBoard spawn, TBitBoard mobilityBoard);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract TBitBoard LocateHardDropReachablePoints(TBitBoard spawn, TBitBoard mobilityBoard);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract TBitBoard LocateNewReachablePoints(TBitBoard reached, TBitBoard mobilityBoard);
    }
}
