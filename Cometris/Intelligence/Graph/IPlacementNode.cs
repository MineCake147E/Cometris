using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cometris.Boards;
using Cometris.Pieces;

namespace Cometris.Intelligence.Graph
{
    public interface IPlacementNode<TSelf, TPiecePlacement, TBitBoard>
        where TSelf : struct, IPlacementNode<TSelf, TPiecePlacement, TBitBoard>
        where TPiecePlacement : unmanaged, IPiecePlacement<TPiecePlacement>
        where TBitBoard : unmanaged, IOperableBitBoard<TBitBoard, ushort>
    {
        TPiecePlacement Placement { get; }
    }
}
