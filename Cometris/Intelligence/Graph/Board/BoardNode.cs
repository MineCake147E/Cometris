using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Cometris.Boards;
using Cometris.Pieces;

namespace Cometris.Intelligence.Graph.Board
{
    public class BoardNode<TBitBoard>
        where TBitBoard : unmanaged, IOperableBitBoard<TBitBoard, ushort>
    {
        private readonly TBitBoard board;
        private readonly Piece nextPiece;
        private readonly Piece holdPiece;
        private readonly IList<PlacementNode<TBitBoard>> nextChildren = [];
        private readonly IList<PlacementNode<TBitBoard>> holdChildren = [];
        private readonly IList<(BoardNode<TBitBoard>, short index)> parents = [];
        private Lock parentsLock = new();

        public PlacementNode<TBitBoard> GetChildAt(short index)
        {
            int v = index;
            var list = v < 0 ? holdChildren : nextChildren;
            var s = v >> ~0;
            v = (v ^ s) - s;
            return list[v];
        }
    }
}
