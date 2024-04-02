using MikoMino;

namespace Cometris.Tests
{
    public class PointTests
    {
        private static IEnumerable<TestCaseData> RotateRotatesCorrectlyTestCaseSource()
        {
            var up = new Point(0, 1);
            yield return new TestCaseData(up, Angle.Up, up);
            yield return new TestCaseData(up, Angle.Right, new Point(1, 0));
            yield return new TestCaseData(up, Angle.Down, new Point(0, -1));
            yield return new TestCaseData(up, Angle.Left, new Point(-1, 0));
            var upperRight = new Point(1, 1);
            yield return new TestCaseData(upperRight, Angle.Up, upperRight);
            yield return new TestCaseData(upperRight, Angle.Right, new Point(1, -1));
            yield return new TestCaseData(upperRight, Angle.Down, new Point(-1, -1));
            yield return new TestCaseData(upperRight, Angle.Left, new Point(-1, 1));
        }

        private static IEnumerable<TestCaseData> RotateIndividualDirectionsRotatesCorrectlyTestCaseSource()
        {
            var up = new Point(0, 1);
            yield return new TestCaseData(up);
            var upperRight = new Point(1, 1);
            yield return new TestCaseData(upperRight);
        }

        [TestCaseSource(nameof(RotateRotatesCorrectlyTestCaseSource))]
        public void RotateRotatesCorrectly(Point point, Angle angle, Point expected)
            => Assert.That(Point.Rotate(point, angle), Is.EqualTo(expected));

        [TestCaseSource(nameof(RotateIndividualDirectionsRotatesCorrectlyTestCaseSource))]
        public void RotateLeftRotatesCorrectly(Point point)
            => Assert.That(Point.RotateLeft(point), Is.EqualTo(Point.Rotate(point, Angle.Left)));

        [TestCaseSource(nameof(RotateIndividualDirectionsRotatesCorrectlyTestCaseSource))]
        public void RotateRightRotatesCorrectly(Point point)
            => Assert.That(Point.RotateRight(point), Is.EqualTo(Point.Rotate(point, Angle.Right)));
    }
}
