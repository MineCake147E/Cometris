using System;
using System.Runtime.Intrinsics;

using MikoMino;

namespace Cometris.Pieces
{
    public interface IPiecePlacement<TSelf> : IEquatable<TSelf>
        where TSelf : unmanaged, IPiecePlacement<TSelf>
    {
        Piece Piece { get; }
        Point Position { get; }
        Angle Angle { get; }

        static abstract ulong CalculateHashCode(TSelf value);

        static abstract ulong CalculateKeyedHashCode(TSelf value, ulong key);
    }
}
