using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Cometris.Boards.Hashing
{
    public interface IBoardHashProvider<TSelf, TBitBoard, TLineElement>
        where TSelf : unmanaged, IBoardHashProvider<TSelf, TBitBoard, TLineElement>
        where TBitBoard : unmanaged, IBitBoard<TBitBoard, TLineElement>
        where TLineElement : unmanaged, IBinaryNumber<TLineElement>, IUnsignedNumber<TLineElement>
    {
        static abstract bool IsSupported { get; }

        static abstract ulong CalculateHash(TBitBoard board, ulong key = default);
    }
}
