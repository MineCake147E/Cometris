using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Cometris.Boards.Hashing
{
    public readonly struct BoardHashProvider<TBitBoard> : IBoardHashProvider<BoardHashProvider<TBitBoard>, TBitBoard, ushort>, IEqualityComparer<TBitBoard>
        where TBitBoard : unmanaged, IOperableBitBoard<TBitBoard, ushort>, IHashBitBoard<TBitBoard, ushort>
    {
        public static bool IsSupported => TBitBoard.IsSupported;

        public static ulong CalculateHash(TBitBoard board, ulong key = 0) => TBitBoard.CalculateHash(board, key);
        public bool Equals(TBitBoard x, TBitBoard y) => x == y;
        public int GetHashCode([DisallowNull] TBitBoard obj) => obj.GetHashCode();
    }
}
