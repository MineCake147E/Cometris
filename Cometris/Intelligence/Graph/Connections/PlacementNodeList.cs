using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cometris.Utils;

namespace Cometris.Intelligence.Graph.Connections
{
    public sealed class PlacementNodeList
    {
        
    }

    public interface IPlacementNodeList
    {
        public static abstract CombinableAngles SupportedAngles { get; }
    }
}
