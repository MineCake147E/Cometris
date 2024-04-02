using MikoMino;

namespace Cometris.Pieces
{
    public interface IPiecePlacement<TSelf>
        where TSelf : unmanaged, IPiecePlacement<TSelf>
    {
        Piece Piece { get; }
        Point Position { get; }
        Angle Angle { get; }

        static abstract ulong CalculateHashCode(TSelf value);
    }
}
