using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cometris.Boards;
using Cometris.Pieces;

namespace Cometris.Intelligence.Graph
{
    public readonly struct PlacementNode<TBitBoard>
        where TBitBoard : unmanaged, IOperableBitBoard<TBitBoard, ushort>
    {
        public ChildList<TBitBoard> Parent { get; init; }
        public CompressedPiecePlacement Placement { get; init; }
        public ChildList<TBitBoard> Children { get; init; }
        // evaluation
    }
}
