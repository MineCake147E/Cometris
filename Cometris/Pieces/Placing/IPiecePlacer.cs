using System.Runtime.CompilerServices;

using Cometris.Boards;

namespace Cometris.Pieces.Placing
{
    public interface IPiecePlacer<TBitBoard>
        where TBitBoard : unmanaged, IBitBoard<TBitBoard, ushort>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract TBitBoard PlaceUp(int x, int y);
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract TBitBoard PlaceRight(int x, int y);
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract TBitBoard PlaceDown(int x, int y);
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract TBitBoard PlaceLeft(int x, int y);
    }
}
