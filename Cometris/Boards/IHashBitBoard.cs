using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Cometris.Boards
{
    public interface IHashBitBoard<TSelf, TLineElement> : IBitBoard<TSelf, TLineElement>
        where TSelf : unmanaged, IHashBitBoard<TSelf, TLineElement>
        where TLineElement : unmanaged, IBinaryNumber<TLineElement>, IUnsignedNumber<TLineElement>, IBinaryInteger<TLineElement>
    {
        static abstract ulong CalculateHash(TSelf board, ulong key = default);
    }
}
