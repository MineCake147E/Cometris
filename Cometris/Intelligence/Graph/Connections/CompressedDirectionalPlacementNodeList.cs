using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;

using Cometris.Utils.Vector;

using MikoMino;

namespace Cometris.Intelligence.Graph.Connections
{
    public readonly struct CompressedDirectionalPlacementNodeList
    {
        private readonly QuadVector128<uint> highestNodePerColumn;
        /// <summary>
        /// Metadata bit fields:<br/>
        /// 0bsmYY_YYYY<br/>
        /// Y: Y coordinate of highest placeable position in range [0,63].<br/>
        /// m: 1 if the column contains multiple nodes (reader should be looking at external list of nodes), 0 if single.<br/>
        /// s: 1 if the column contains any node, 0 if empty.
        /// </summary>
        private readonly Vector128<sbyte> metadataPerColumn;
        private readonly List<CompressedPositionNodeList>? values;
    }

    public readonly struct CompressedPositionNodeList
    {
        private readonly Vector128<uint> container;
        public CompressedPointList Positions => new(container.GetElement(0));
        public uint Item0 => container.GetElement(1);
        public uint Item1 => container.GetElement(2);
        public uint Item2 => container.GetElement(3);

        public CompressedPositionNodeList(CompressedPointList positions, uint item0, uint item1, uint item2)
        {
            container = Vector128.Create(positions.Value, item0, item1, item2);
        }
    }
}
