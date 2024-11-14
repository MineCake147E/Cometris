using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using ModernMemory.Collections;

namespace Cometris.Boards
{
    public readonly struct BoardLineEnumerable<TBitBoard, TLineElement>(TBitBoard value) : ITypedEnumerable<TLineElement, BoardLineEnumerable<TBitBoard, TLineElement>.Enumerator>
        where TBitBoard : unmanaged, IBitBoard<TBitBoard, TLineElement>
        where TLineElement : unmanaged, IBinaryNumber<TLineElement>, IUnsignedNumber<TLineElement>
    {
        readonly TBitBoard value = value;

        public Enumerator GetEnumerator() => new(value);

        public struct Enumerator(TBitBoard value) : IEnumerator<TLineElement>
        {
            readonly TBitBoard value = value;
            int index = -1;

            public readonly TLineElement Current => value[index];

            readonly object IEnumerator.Current => Current;

            public void Dispose() => index = TBitBoard.Height;
            public bool MoveNext() => ++index < TBitBoard.Height;
            public void Reset() => index = -1;
        }
    }
}
