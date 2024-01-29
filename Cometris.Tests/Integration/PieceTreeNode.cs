using System.Runtime.InteropServices;

using Cometris.Pieces;

namespace Cometris.Tests.Integration
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct PieceTreeNode<TItem>
    {
        public Piece Piece { get; }
        public TItem Item { get; }
    }
}
