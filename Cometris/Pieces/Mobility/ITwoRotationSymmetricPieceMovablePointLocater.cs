using System.Runtime.CompilerServices;

using Cometris.Boards;

namespace Cometris.Pieces.Mobility
{
    public interface IAsymmetricPieceMovablePointLocater<TBitBoard> where TBitBoard : unmanaged, IBitBoard<TBitBoard, ushort>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) LocateMovablePoints(TBitBoard bitBoard);
    }

    public interface ITwoRotationSymmetricPieceMovablePointLocater<TBitBoard> : IAsymmetricPieceMovablePointLocater<TBitBoard> where TBitBoard : unmanaged, IBitBoard<TBitBoard, ushort>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract (TBitBoard upper, TBitBoard right) LocateSymmetricMovablePoints(TBitBoard bitBoard);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) ConvertToAsymmetricMobility((TBitBoard upper, TBitBoard right) boards);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract (TBitBoard upper, TBitBoard right) MergeToTwoRotationSymmetricMobility((TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) boards);
    }
    public interface ISymmetricPieceMovablePointLocater<TBitBoard> : IAsymmetricPieceMovablePointLocater<TBitBoard> where TBitBoard : unmanaged, IBitBoard<TBitBoard, ushort>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract TBitBoard LocateSymmetricMovablePoints(TBitBoard bitBoard);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) ConvertToAsymmetricMobility(TBitBoard board);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract TBitBoard MergeToSymmetricMobility((TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) board);
    }
}
