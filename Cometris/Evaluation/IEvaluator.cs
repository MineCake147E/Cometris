using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Cometris.Boards;

namespace Cometris.Evaluation
{
    public interface IEvaluator<TBitBoard, TVectorLineMask, TBinaryLineMask>
        where TBitBoard : unmanaged, IMaskableBitBoard<TBitBoard, ushort, TVectorLineMask, TBinaryLineMask>
        where TVectorLineMask : struct, IEquatable<TVectorLineMask>
        where TBinaryLineMask : unmanaged, IBinaryInteger<TBinaryLineMask>
    {
        
    }
}
