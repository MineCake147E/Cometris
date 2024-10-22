using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;

using Cometris.Boards;

namespace Cometris.Movements
{
    public readonly partial struct PieceTRotatabilityLocator<TBitBoard>
        where TBitBoard : unmanaged, IOperableBitBoard<TBitBoard, ushort>
    {
        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static TBitBoard LocateFullTwistRight(TBitBoard rightMobility, TBitBoard upReached, TBitBoard downReached)
        {
            if (rightMobility is PartialBitBoard512 zmm0 && upReached is PartialBitBoard512 zmm1 && downReached is PartialBitBoard512 zmm2)
            {
                if (LocateFullTwistRightInternal(zmm0, zmm1, zmm2) is TBitBoard t) return t;
                return default;
            }
            return LocateFullTwistRightInternal(rightMobility, upReached, downReached);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static TBitBoard LocateFullTwistRightInternal(TBitBoard rightMobility, TBitBoard upReached, TBitBoard downReached)
        {
            // Rotation from Down and Up to Right 
            var scc = downReached;
            var scw = upReached;
            var mobility = rightMobility;
            TBitBoard result;
            var filled = TBitBoard.Zero;
            TBitBoard rtcc0;
            TBitBoard rtcc1;
            TBitBoard rtcw0;
            TBitBoard rtcw1;
            // Calculate y = 0 offset #0 as m
            var m = mobility;
            // Offset #0 (0, 0)(m) is already available.
            // Test CounterClockwise rotation from Down with offset #0 (0, 0)(m)
            // Test Clockwise rotation from Up with offset #0 (0, 0)(m)
            scc = TBitBoard.AndNot(m, scc);
            scw = TBitBoard.AndNot(m, scw);
            // Calculate offset #1 (-1, 0) as ml1
            var ml1 = m >> 1;
            // Calculate y = 1 offset #2 as mu1
            var mu1 = TBitBoard.ShiftDownOneLine(mobility, filled);
            // Calculate y = -2 offset #2 as md2
            var md2 = TBitBoard.ShiftUpTwoLines(mobility, filled);
            // Test CounterClockwise rotation from Down with offset #1 (-1, 0)(ml1)
            // Test Clockwise rotation from Up with offset #1 (-1, 0)(ml1)
            scc = TBitBoard.AndNot(ml1, scc);
            scw = TBitBoard.AndNot(ml1, scw);
            // Calculate offset #2 (-1, 1) as ml1u1
            var ml1u1 = mu1 >> 1;
            // Offset #2 (0, -2)(md2) is already available.
            // Test Clockwise rotation from Up with offset #2 (-1, 1)(ml1u1)
            // Test CounterClockwise rotation from Down with offset #2 (0, -2)(md2)
            scw = TBitBoard.AndNot(ml1u1, scw);
            scc = TBitBoard.AndNot(md2, scc);
            // Calculate offset #3 (-1, -2) as ml1d2
            var ml1d2 = md2 >> 1;
            // Test CounterClockwise rotation from Down with offset #3 (-1, -2)(ml1d2)
            // Test Clockwise rotation from Up with offset #3 (-1, -2)(ml1d2)
            rtcc1 = scc & ml1d2;
            rtcw1 = scw & ml1d2;
            rtcc1 |= rtcw1;
            rtcc1 <<= 1;
            rtcc1 = TBitBoard.ShiftDownTwoLines(rtcc1, filled);
            result = rtcc1;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static PartialBitBoard512 LocateFullTwistRightInternal(PartialBitBoard512 rightMobility, PartialBitBoard512 upReached, PartialBitBoard512 downReached)
        {
            // Rotation from Down and Up to Right 
            Vector512<ushort> scc = downReached;
            Vector512<ushort> scw = upReached;
            Vector512<ushort> mobility = rightMobility;
            Vector512<ushort> result;
            var filled = Vector256<ushort>.Zero.ToVector512Unsafe();
            Vector512<ushort> rtcc0;
            Vector512<ushort> rtcc1;
            Vector512<ushort> rtcw0;
            Vector512<ushort> rtcw1;
            // Calculate y = 0 offset #0 as m
            var m = mobility;
            // Offset #0 (0, 0)(m) is already available.
            // Test CounterClockwise rotation from Down with offset #0 (0, 0)(m)
            // Test Clockwise rotation from Up with offset #0 (0, 0)(m)
            rtcc0 = scc & m;
            rtcw0 = scw & m;
            scc = PartialBitBoard512.AndNot(m, scc);
            scw = PartialBitBoard512.AndNot(m, scw);
            result = rtcc0 | rtcw0;
            // Calculate offset #1 (-1, 0) as ml1
            var ml1 = m >> 1;
            // Calculate y = 1 offset #2 as mu1
            var mu1 = PartialBitBoard512.ShiftDownOneLine(mobility, filled);
            // Calculate y = -2 offset #2 as md2
            var md2 = PartialBitBoard512.ShiftUpTwoLines(mobility, filled);
            // Test CounterClockwise rotation from Down with offset #1 (-1, 0)(ml1)
            // Test Clockwise rotation from Up with offset #1 (-1, 0)(ml1)
            rtcc1 = scc & ml1;
            rtcw1 = scw & ml1;
            rtcc1 |= rtcw1;
            scc = PartialBitBoard512.AndNot(ml1, scc);
            scw = PartialBitBoard512.AndNot(ml1, scw);
            // Calculate offset #2 (-1, 1) as ml1u1
            var ml1u1 = mu1 >> 1;
            // Offset #2 (0, -2)(md2) is already available.
            rtcc1 = rtcc1 << 1;
            // Test Clockwise rotation from Up with offset #2 (-1, 1)(ml1u1)
            // Test CounterClockwise rotation from Down with offset #2 (0, -2)(md2)
            rtcw0 = scw & ml1u1;
            rtcc0 = scc & md2;
            scw = PartialBitBoard512.AndNot(ml1u1, scw);
            scc = PartialBitBoard512.AndNot(md2, scc);
            result |= rtcc1;
            // Calculate offset #3 (-1, -2) as ml1d2
            var ml1d2 = md2 >> 1;
            rtcc0 = PartialBitBoard512.ShiftDownTwoLines(rtcc0, filled);
            rtcw0 = rtcw0 << 1;
            // Test CounterClockwise rotation from Down with offset #3 (-1, -2)(ml1d2)
            // Test Clockwise rotation from Up with offset #3 (-1, -2)(ml1d2)
            rtcw0 = PartialBitBoard512.ShiftUpOneLine(rtcw0, filled);
            rtcc1 = scc & ml1d2;
            rtcw1 = scw & ml1d2;
            rtcc1 |= rtcw1;
            scc = PartialBitBoard512.AndNot(ml1d2, scc);
            scw = PartialBitBoard512.AndNot(ml1d2, scw);
            result = PartialBitBoard512.OrAll(result, rtcw0, rtcc0);
            rtcc1 = rtcc1 << 1;
            rtcc1 = PartialBitBoard512.ShiftDownTwoLines(rtcc1, filled);
            result = rtcc1;
            return result;
        }

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static TBitBoard LocateFullTwistLeft(TBitBoard leftMobility, TBitBoard downReached, TBitBoard upReached)
        {
            if (leftMobility is PartialBitBoard512 zmm0 && downReached is PartialBitBoard512 zmm1 && upReached is PartialBitBoard512 zmm2)
            {
                if (LocateFullTwistLeftInternal(zmm0, zmm1, zmm2) is TBitBoard t) return t;
                return default;
            }
            return LocateFullTwistLeftInternal(leftMobility, downReached, upReached);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static TBitBoard LocateFullTwistLeftInternal(TBitBoard leftMobility, TBitBoard downReached, TBitBoard upReached)
        {
            // Rotation from Up and Down to Left 
            var scc = upReached;
            var scw = downReached;
            var mobility = leftMobility;
            TBitBoard result;
            var filled = TBitBoard.Zero;
            TBitBoard rtcc0;
            TBitBoard rtcc1;
            TBitBoard rtcw0;
            TBitBoard rtcw1;
            // Calculate y = 0 offset #0 as m
            var m = mobility;
            // Offset #0 (0, 0)(m) is already available.
            // Test CounterClockwise rotation from Up with offset #0 (0, 0)(m)
            // Test Clockwise rotation from Down with offset #0 (0, 0)(m)
            rtcc0 = scc & m;
            rtcw0 = scw & m;
            scc = TBitBoard.AndNot(m, scc);
            scw = TBitBoard.AndNot(m, scw);
            result = rtcc0 | rtcw0;
            // Calculate offset #1 (1, 0) as mr1
            var mr1 = m << 1;
            // Calculate y = 1 offset #2 as mu1
            var mu1 = TBitBoard.ShiftDownOneLine(mobility, filled);
            // Calculate y = -2 offset #2 as md2
            var md2 = TBitBoard.ShiftUpTwoLines(mobility, filled);
            // Test CounterClockwise rotation from Up with offset #1 (1, 0)(mr1)
            // Test Clockwise rotation from Down with offset #1 (1, 0)(mr1)
            rtcc1 = scc & mr1;
            rtcw1 = scw & mr1;
            rtcc1 |= rtcw1;
            scc = TBitBoard.AndNot(mr1, scc);
            scw = TBitBoard.AndNot(mr1, scw);
            // Calculate offset #2 (1, 1) as mr1u1
            var mr1u1 = mu1 << 1;
            // Offset #2 (0, -2)(md2) is already available.
            rtcc1 = rtcc1 >> 1;
            // Test CounterClockwise rotation from Up with offset #2 (1, 1)(mr1u1)
            // Test Clockwise rotation from Down with offset #2 (0, -2)(md2)
            rtcc0 = scc & mr1u1;
            rtcw0 = scw & md2;
            scc = TBitBoard.AndNot(mr1u1, scc);
            scw = TBitBoard.AndNot(md2, scw);
            result |= rtcc1;
            // Calculate offset #3 (1, -2) as mr1d2
            var mr1d2 = md2 << 1;
            rtcw0 = TBitBoard.ShiftDownTwoLines(rtcw0, filled);
            rtcc0 = rtcc0 >> 1;
            // Test CounterClockwise rotation from Up with offset #3 (1, -2)(mr1d2)
            // Test Clockwise rotation from Down with offset #3 (1, -2)(mr1d2)
            rtcc0 = TBitBoard.ShiftUpOneLine(rtcc0, filled);
            rtcc1 = scc & mr1d2;
            rtcw1 = scw & mr1d2;
            rtcc1 |= rtcw1;
            scc = TBitBoard.AndNot(mr1d2, scc);
            scw = TBitBoard.AndNot(mr1d2, scw);
            result = TBitBoard.OrAll(result, rtcc0, rtcw0);
            rtcc1 = rtcc1 >> 1;
            rtcc1 = TBitBoard.ShiftDownTwoLines(rtcc1, filled);
            result = rtcc1;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static PartialBitBoard512 LocateFullTwistLeftInternal(PartialBitBoard512 leftMobility, PartialBitBoard512 downReached, PartialBitBoard512 upReached)
        {
            // Rotation from Up and Down to Left 
            Vector512<ushort> scc = upReached;
            Vector512<ushort> scw = downReached;
            Vector512<ushort> mobility = leftMobility;
            Vector512<ushort> result;
            var filled = Vector256<ushort>.Zero.ToVector512Unsafe();
            Vector512<ushort> rtcc0;
            Vector512<ushort> rtcc1;
            Vector512<ushort> rtcw0;
            Vector512<ushort> rtcw1;
            // Calculate y = 0 offset #0 as m
            var m = mobility;
            // Offset #0 (0, 0)(m) is already available.
            // Test CounterClockwise rotation from Up with offset #0 (0, 0)(m)
            // Test Clockwise rotation from Down with offset #0 (0, 0)(m)
            rtcc0 = scc & m;
            rtcw0 = scw & m;
            scc = PartialBitBoard512.AndNot(m, scc);
            scw = PartialBitBoard512.AndNot(m, scw);
            result = rtcc0 | rtcw0;
            // Calculate offset #1 (1, 0) as mr1
            var mr1 = m << 1;
            // Calculate y = 1 offset #2 as mu1
            var mu1 = PartialBitBoard512.ShiftDownOneLine(mobility, filled);
            // Calculate y = -2 offset #2 as md2
            var md2 = PartialBitBoard512.ShiftUpTwoLines(mobility, filled);
            // Test CounterClockwise rotation from Up with offset #1 (1, 0)(mr1)
            // Test Clockwise rotation from Down with offset #1 (1, 0)(mr1)
            rtcc1 = scc & mr1;
            rtcw1 = scw & mr1;
            rtcc1 |= rtcw1;
            scc = PartialBitBoard512.AndNot(mr1, scc);
            scw = PartialBitBoard512.AndNot(mr1, scw);
            // Calculate offset #2 (1, 1) as mr1u1
            var mr1u1 = mu1 << 1;
            // Offset #2 (0, -2)(md2) is already available.
            rtcc1 = rtcc1 >> 1;
            // Test CounterClockwise rotation from Up with offset #2 (1, 1)(mr1u1)
            // Test Clockwise rotation from Down with offset #2 (0, -2)(md2)
            rtcc0 = scc & mr1u1;
            rtcw0 = scw & md2;
            scc = PartialBitBoard512.AndNot(mr1u1, scc);
            scw = PartialBitBoard512.AndNot(md2, scw);
            result |= rtcc1;
            // Calculate offset #3 (1, -2) as mr1d2
            var mr1d2 = md2 << 1;
            rtcw0 = PartialBitBoard512.ShiftDownTwoLines(rtcw0, filled);
            rtcc0 = rtcc0 >> 1;
            // Test CounterClockwise rotation from Up with offset #3 (1, -2)(mr1d2)
            // Test Clockwise rotation from Down with offset #3 (1, -2)(mr1d2)
            rtcc0 = PartialBitBoard512.ShiftUpOneLine(rtcc0, filled);
            rtcc1 = scc & mr1d2;
            rtcw1 = scw & mr1d2;
            rtcc1 |= rtcw1;
            scc = PartialBitBoard512.AndNot(mr1d2, scc);
            scw = PartialBitBoard512.AndNot(mr1d2, scw);
            result = PartialBitBoard512.OrAll(result, rtcc0, rtcw0);
            rtcc1 = rtcc1 >> 1;
            rtcc1 = PartialBitBoard512.ShiftDownTwoLines(rtcc1, filled);
            result = rtcc1;
            return result;
        }
    }
}
