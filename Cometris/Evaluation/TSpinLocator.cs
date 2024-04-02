using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cometris.Boards;

namespace Cometris.Evaluation
{
    public readonly struct TSpinLocator
    {
        public static (TBitBoard mini, (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) full) LocateTwistablePoints<TBitBoard>(TBitBoard bitBoard, in (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) reached, in (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) mobility) where TBitBoard : unmanaged, IBitBoard<TBitBoard, ushort>
        {
            (var mini, (var upper, var right, var lower, var left)) = LocatePossibleTwistPoints(bitBoard);
            var (upperReached, rightReached, lowerReached, leftReached) = reached;
            var (upperMobility, rightMobility, lowerMobility, leftMobility) = mobility;
            mini &= TBitBoard.OrAll(upperMobility, rightMobility, lowerMobility) | leftMobility;
            return (mini, (upper, right, lower, left));
        }

        private static (TBitBoard mini, (TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) full) LocatePossibleTwistPoints<TBitBoard>(TBitBoard bitBoard) where TBitBoard : unmanaged, IBitBoard<TBitBoard, ushort>
        {
            var s = ~bitBoard;
            var upperBoard = TBitBoard.ShiftDownOneLine(s, TBitBoard.InvertedEmpty);
            var lowerBoard = TBitBoard.ShiftUpOneLine(s, TBitBoard.Zero);
            var a = upperBoard & lowerBoard;
            var o = upperBoard | lowerBoard;
            var al = a >> 1;
            var ar = a << 1;
            var ol = o >> 1;
            var or = o << 1;
            var oa = al | ar;
            var ul = upperBoard >> 1;
            var ur = upperBoard << 1;
            var ao = ol & or;
            var dl = lowerBoard >> 1;
            var dr = lowerBoard << 1;
            var mini = ~(oa | ao);
            var ou = ul | ur;
            var od = dl | dr;
            return (mini, (ou, or, od, ol));
        }
    }
}
