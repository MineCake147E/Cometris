using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Cometris.Boards;

namespace Cometris.Evaluation
{
    public static class BoardStatistics
    {
        public static (int overhangs, int overhangedCells) CountOverhangs<TBitBoard>(TBitBoard board)
            where TBitBoard : unmanaged, IBitBoard<TBitBoard, ushort>
        {
            var b = board;
            var up = TBitBoard.ShiftDownOneLine(b, TBitBoard.Zero);
            var notB = ~b;
            var ohBoard = TBitBoard.AndNot(b, up);
            var filled = TBitBoard.FillDropReachable(notB, ohBoard);
            return (TBitBoard.TotalBlocks(ohBoard), TBitBoard.TotalBlocks(filled));
        }

        public static (int walls, int doubleSidedWalls, int tripleConsecutiveDoubleWalls) CountWalls<TBitBoard>(TBitBoard board)
            where TBitBoard : unmanaged, IBitBoard<TBitBoard, ushort>
        {
            var b = board;
            var left = b >> 1;
            var right = b << 1;
            var singleWalls = TBitBoard.AndNot0Or12(b, left, right);
            var doubleWalls = TBitBoard.AndNot0And12(b, left, right);
            var low = TBitBoard.ShiftUpOneLine(doubleWalls, TBitBoard.Zero);
            var high = TBitBoard.ShiftDownOneLine(doubleWalls, TBitBoard.Zero);
            var tcdw = TBitBoard.AndAll(doubleWalls, low, high);
            return (TBitBoard.TotalBlocks(singleWalls), TBitBoard.TotalBlocks(doubleWalls), TBitBoard.TotalBlocks(tcdw));
        }
        public static (int safeBlocks, int safeHeight, int dangerousBlocks, int dangerousHeight) AggregateTerrain<TBitBoard, TVectorLineMask, TCompactLineMask>(TBitBoard board, ushort safeMask = 0xfc3f)
            where TBitBoard : unmanaged, IBitBoard<TBitBoard, ushort>, IMaskableBitBoard<TBitBoard, ushort, TVectorLineMask, TCompactLineMask>
            where TVectorLineMask : struct, IEquatable<TVectorLineMask>
            where TCompactLineMask : unmanaged, IBinaryInteger<TCompactLineMask>
        {
            var b = board & TBitBoard.InvertedEmpty;
            var heightMap = TBitBoard.FillDropReachable(TBitBoard.AllBitsSet, b);
            var safePositions = TBitBoard.CreateFilled(safeMask);
            var safeMap = heightMap & safePositions;
            var dangerousMap = TBitBoard.AndNot(safePositions, heightMap);
            var safeBlocks = TBitBoard.TotalBlocks(safeMap);
            var dangerousBlocks = TBitBoard.TotalBlocks(dangerousMap);
            var safeHeight = TBitBoard.LineHeight(TBitBoard.CompareNotEqualPerLineCompact(safeMap, TBitBoard.Zero));
            var dangerousHeight = TBitBoard.LineHeight(TBitBoard.CompareNotEqualPerLineCompact(dangerousMap, TBitBoard.Zero));
            return (safeBlocks, safeHeight, dangerousBlocks, dangerousHeight);
        }
    }
}
