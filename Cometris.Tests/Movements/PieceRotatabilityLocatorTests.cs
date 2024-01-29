using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cometris.Boards;
using Cometris.Utils;

namespace Cometris.Tests.Movements
{
    [TestFixture(typeof(PartialBitBoard256X2))]
    [TestFixture(typeof(PartialBitBoard512))]
    public partial class PieceRotatabilityLocatorTests<TBitBoard> where TBitBoard : unmanaged, IBitBoard<TBitBoard, ushort>
    {
        #region Offsets
        static IReadOnlyList<Point> OffsetsUpIPiece => [new Point(0, 0), new Point(-1, 0), new Point(+2, 0), new Point(-1, 0), new Point(+2, 0)];
        static IReadOnlyList<Point> OffsetsRightIPiece => [new Point(-1, 0), new Point(0, 0), new Point(0, 0), new Point(0, +1), new Point(0, -2)];
        static IReadOnlyList<Point> OffsetsDownIPiece => [new Point(-1, +1), new Point(+1, +1), new Point(-2, +1), new Point(+1, 0), new Point(-2, 0)];
        static IReadOnlyList<Point> OffsetsLeftIPiece => [new Point(0, +1), new Point(0, +1), new Point(0, +1), new Point(0, -1), new Point(0, +2)];
        static AngleTuple<IReadOnlyList<Point>> OffsetsIPiece => new(OffsetsUpIPiece, OffsetsRightIPiece, OffsetsDownIPiece, OffsetsLeftIPiece);

        static IReadOnlyList<Point> OffsetsUp => [new Point(0, 0), new Point(0, 0), new Point(0, 0), new Point(0, 0), new Point(0, 0)];
        static IReadOnlyList<Point> OffsetsRight => [new Point(0, 0), new Point(+1, 0), new Point(+1, -1), new Point(0, +2), new Point(+1, +2)];
        static IReadOnlyList<Point> OffsetsDown => [new Point(0, 0), new Point(0, 0), new Point(0, 0), new Point(0, 0), new Point(0, 0)];
        static IReadOnlyList<Point> OffsetsLeft => [new Point(0, 0), new Point(-1, 0), new Point(-1, -1), new Point(0, +2), new Point(-1, +2)];
        static AngleTuple<IReadOnlyList<Point>> Offsets => new(OffsetsUp, OffsetsRight, OffsetsDown, OffsetsLeft);
        static IReadOnlyList<bool> IgnoreForTPieceUp => [false, false, false, true, false];
        static IReadOnlyList<bool> IgnoreForTPieceDown => [false, false, true, false, false];
        static IReadOnlyList<bool> IgnoreForTPieceHorizontal => [false, false, false, false, false];
        static AngleTuple<IReadOnlyList<bool>> IgnoreForTPiece => new(IgnoreForTPieceUp, IgnoreForTPieceHorizontal, IgnoreForTPieceDown, IgnoreForTPieceHorizontal);
        static IReadOnlyList<(string name, int offset)> Rotations => [("Clockwise", 1), ("CounterClockwise", -1)];
        static AngleTuple<Angle> Orientations => new(Angle.Up, Angle.Right, Angle.Down, Angle.Left);
        #endregion

    }
}
