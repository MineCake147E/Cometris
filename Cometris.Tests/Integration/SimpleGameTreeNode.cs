using Cometris.Boards;

namespace Cometris.Tests.Integration
{
    public sealed class SimpleGameTree<TBitBoard> where TBitBoard : unmanaged, IOperableBitBoard<TBitBoard, ushort>
    {
        private ReadOnlyMemory<CompressedPositionsTuple> Positions { get; }

        private ReadOnlyMemory<PieceTreeNode<int>> PieceTree { get; }
    }
}
