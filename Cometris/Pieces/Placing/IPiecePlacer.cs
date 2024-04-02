using System.Runtime.CompilerServices;

using Cometris.Boards;

using MikoMino;

namespace Cometris.Pieces.Placing
{
    public interface IPiecePlacer<TBitBoard>
        where TBitBoard : unmanaged, IBitBoard<TBitBoard, ushort>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract TBitBoard Place(Angle angle, int x, int y);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract TBitBoard PlaceUp(int x, int y);
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract TBitBoard PlaceRight(int x, int y);
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract TBitBoard PlaceDown(int x, int y);
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static abstract TBitBoard PlaceLeft(int x, int y);
    }

    public static class PiecePlacerImplementationUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static TBitBoard Place<TBitBoard, TPiecePlacer>(Angle angle, int x, int y)
            where TBitBoard : unmanaged, IBitBoard<TBitBoard, ushort>
            where TPiecePlacer : unmanaged, IPiecePlacer<TBitBoard>
            => angle switch
            {
                Angle.Up => TPiecePlacer.PlaceUp(x, y),
                Angle.Right => TPiecePlacer.PlaceRight(x, y),
                Angle.Down => TPiecePlacer.PlaceDown(x, y),
                Angle.Left => TPiecePlacer.PlaceLeft(x, y),
                _ => default,
            };
    }
}
