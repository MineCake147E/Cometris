using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Cometris.Boards
{
    public partial interface IOperableBitBoard<TSelf, TLineElement>
    {
        #region Operator Supplement
        /// <summary>
        /// Negate the <paramref name="left"/>, and computes AND with <paramref name="right"/>.
        /// </summary>
        /// <param name="left">The first board to be negated.</param>
        /// <param name="right">The second board.</param>
        /// <returns>~<paramref name="left"/> &amp; <paramref name="right"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual TSelf AndNot(TSelf left, TSelf right) => ~left & right;

        /// <summary>
        /// Calculates the boolean expression (<paramref name="b0"/> &amp; <paramref name="b2"/>) | <paramref name="b1"/> for each blocks.
        /// </summary>
        /// <param name="b0">The value to be ANDed with <paramref name="b2"/>.</param>
        /// <param name="b1">The value to be ORed with (<paramref name="b0"/> &amp; <paramref name="b2"/>).</param>
        /// <param name="b2">The value to be ANDed with <paramref name="b0"/>.</param>
        /// <returns>(<paramref name="b0"/> &amp; <paramref name="b2"/>) | <paramref name="b1"/></returns>
        static virtual TSelf Or1And02(TSelf b0, TSelf b1, TSelf b2) => (b0 & b2) | b1;

        /// <summary>
        /// Blend the <paramref name="left"/> and <paramref name="right"/> according to the bitwise <paramref name="mask"/>.
        /// </summary>
        /// <param name="mask">The bit mask. 1 means <paramref name="right"/>, and 0 means <paramref name="left"/>.</param>
        /// <param name="left">The values to be selected when the corresponding bit of <paramref name="mask"/> is 0.</param>
        /// <param name="right">The values to be selected when the corresponding bit of <paramref name="mask"/> is 1.</param>
        /// <returns><paramref name="left"/> &amp; ((<paramref name="left"/> ^ <paramref name="right"/>) &amp; <paramref name="mask"/>)</returns>
        static virtual TSelf BitwiseSelect(TSelf mask, TSelf left, TSelf right) => left ^ ((right ^ left) & mask);

        /// <summary>
        /// Calculates the boolean expression <paramref name="b0"/> | (<paramref name="b1"/> | <paramref name="b2"/>) for each blocks.
        /// </summary>
        /// <param name="b0">The value to be ORed with (<paramref name="b1"/> | <paramref name="b2"/>).</param>
        /// <param name="b1">The value to be ORed with <paramref name="b2"/>.</param>
        /// <param name="b2">The value to be ORed with <paramref name="b1"/>.</param>
        /// <returns><paramref name="b0"/> | (<paramref name="b1"/> &amp; <paramref name="b2"/>)</returns>
        static virtual TSelf OrAll(TSelf b0, TSelf b1, TSelf b2) => b0 | (b1 | b2);

        /// <summary>
        /// Calculates the boolean expression <paramref name="b0"/> &amp; (<paramref name="b1"/> &amp; <paramref name="b2"/>) for each blocks.
        /// </summary>
        /// <param name="b0">The value to be ANDed with (<paramref name="b1"/> &amp; <paramref name="b2"/>).</param>
        /// <param name="b1">The value to be ANDed with <paramref name="b2"/>.</param>
        /// <param name="b2">The value to be ANDed with <paramref name="b1"/>.</param>
        /// <returns><paramref name="b0"/> &amp; (<paramref name="b1"/> &amp; <paramref name="b2"/>)</returns>
        static virtual TSelf AndAll(TSelf b0, TSelf b1, TSelf b2) => b0 & (b1 & b2);

        /// <summary>
        /// Calculates the boolean expression <paramref name="b0"/> &amp; (<paramref name="b1"/> | <paramref name="b2"/>) for each blocks.
        /// </summary>
        /// <param name="b0">The value to be ANDed with (<paramref name="b1"/> | <paramref name="b2"/>).</param>
        /// <param name="b1">The value to be ORed with <paramref name="b2"/>.</param>
        /// <param name="b2">The value to be ORed with <paramref name="b1"/>.</param>
        /// <returns><paramref name="b0"/> &amp; (<paramref name="b1"/> | <paramref name="b2"/>)</returns>
        static virtual TSelf And0Or12(TSelf b0, TSelf b1, TSelf b2) => b0 & (b1 | b2);

        /// <summary>
        /// Calculates the boolean expression ~<paramref name="b0"/> &amp; (<paramref name="b1"/> | <paramref name="b2"/>) for each blocks.
        /// </summary>
        /// <param name="b0">The value to be negated and ANDed with (<paramref name="b1"/> | <paramref name="b2"/>).</param>
        /// <param name="b1">The value to be ORed with <paramref name="b2"/>.</param>
        /// <param name="b2">The value to be ORed with <paramref name="b1"/>.</param>
        /// <returns>~<paramref name="b0"/> &amp; (<paramref name="b1"/> | <paramref name="b2"/>)</returns>
        static virtual TSelf AndNot0Or12(TSelf b0, TSelf b1, TSelf b2) => ~b0 & (b1 | b2);

        /// <summary>
        /// Calculates the boolean expression ~<paramref name="b0"/> &amp; (<paramref name="b1"/> &amp; <paramref name="b2"/>) for each blocks.
        /// </summary>
        /// <param name="b0">The value to be negated and ANDed with (<paramref name="b1"/> &amp; <paramref name="b2"/>).</param>
        /// <param name="b1">The value to be ANDed with <paramref name="b2"/>.</param>
        /// <param name="b2">The value to be ANDed with <paramref name="b1"/>.</param>
        /// <returns>~<paramref name="b0"/> &amp; (<paramref name="b1"/> &amp; <paramref name="b2"/>)</returns>
        static virtual TSelf AndNot0And12(TSelf b0, TSelf b1, TSelf b2) => ~b0 & (b1 & b2);
        #endregion

        #region Tuple Operations
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual (TSelf upper, TSelf right, TSelf lower, TSelf left) Or4Sets((TSelf upper, TSelf right, TSelf lower, TSelf left) board, (TSelf upper, TSelf right, TSelf lower, TSelf left) reached)
        {
            (var upperBoard, var rightBoard, var lowerBoard, var leftBoard) = (board.upper, board.right, board.lower, board.left);
            (var upperReached, var rightReached, var lowerReached, var leftReached) = (reached.upper, reached.right, reached.lower, reached.left);
            var upper = upperBoard | upperReached;
            var right = rightBoard | rightReached;
            var lower = lowerBoard | lowerReached;
            var left = leftBoard | leftReached;
            return (upper, right, lower, left);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual (TSelf upper, TSelf right, TSelf lower, TSelf left) And4Sets((TSelf upper, TSelf right, TSelf lower, TSelf left) board, (TSelf upper, TSelf right, TSelf lower, TSelf left) reached)
        {
            (var upperBoard, var rightBoard, var lowerBoard, var leftBoard) = (board.upper, board.right, board.lower, board.left);
            (var upperReached, var rightReached, var lowerReached, var leftReached) = (reached.upper, reached.right, reached.lower, reached.left);
            var upper = upperBoard & upperReached;
            var right = rightBoard & rightReached;
            var lower = lowerBoard & lowerReached;
            var left = leftBoard & leftReached;
            return (upper, right, lower, left);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual (TSelf upper, TSelf right, TSelf lower, TSelf left) AndNot4Sets((TSelf upper, TSelf right, TSelf lower, TSelf left) left, (TSelf upper, TSelf right, TSelf lower, TSelf left) right)
        {
            (var upperNegated, var rightNegated, var lowerNegated, var leftNegated) = (left.upper, left.right, left.lower, left.left);
            (var upperOperand, var rightOperand, var lowerOperand, var leftOperand) = (right.upper, right.right, right.lower, right.left);
            var tempU = TSelf.AndNot(upperNegated, upperOperand);
            var tempR = TSelf.AndNot(rightNegated, rightOperand);
            var tempD = TSelf.AndNot(lowerNegated, lowerOperand);
            var tempL = TSelf.AndNot(leftNegated, leftOperand);
            return (tempU, tempR, tempD, tempL);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static virtual (TSelf upper, TSelf right, TSelf lower, TSelf left) AndNot4Sets(TSelf exclusion, (TSelf upper, TSelf right, TSelf lower, TSelf left) right)
        {
            (var upperOperand, var rightOperand, var lowerOperand, var leftOperand) = (right.upper, right.right, right.lower, right.left);
            var tempU = TSelf.AndNot(exclusion, upperOperand);
            var tempR = TSelf.AndNot(exclusion, rightOperand);
            var tempD = TSelf.AndNot(exclusion, lowerOperand);
            var tempL = TSelf.AndNot(exclusion, leftOperand);
            return (tempU, tempR, tempD, tempL);
        }
        #endregion
    }
}
