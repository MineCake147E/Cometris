using System.Runtime.InteropServices;

namespace Cometris.Utils
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct AngleTuple<T>(T Upper, T Right, T Lower, T Left)
    {
        public T this[Angle angle] => (Angle)((uint)angle & 0x3u) switch
        {
            Angle.Right => Right,
            Angle.Down => Lower,
            Angle.Left => Left,
            _ => Upper,
        };
    }
    public static class AngleTupleUtils
    {
        public static AngleTuple<T2> As<T1, T2>(this AngleTuple<T1> value) where T1: T2
            => new(value.Upper, value.Right, value.Lower, value.Left);
    }
}
