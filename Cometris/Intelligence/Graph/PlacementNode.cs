using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cometris.Boards;
using Cometris.Intelligence.Graph.Board;
using Cometris.Pieces;

namespace Cometris.Intelligence.Graph
{
    public readonly struct PlacementNode<TBitBoard>
        where TBitBoard : unmanaged, IOperableBitBoard<TBitBoard, ushort>
    {
        public BoardNode<TBitBoard> Parent { get; init; }
        public CompressedPiecePlacement Placement { get; init; }
        public BoardNode<TBitBoard> Children { get; init; }
        // evaluation
    }
}
