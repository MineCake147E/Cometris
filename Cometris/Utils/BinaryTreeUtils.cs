using System.Numerics;

namespace Cometris.Utils
{
    public static class BinaryTreeUtils
    {
        public static T GetLeftChildren<T>(T node) where T : unmanaged, IBinaryInteger<T> => node + node + T.One;

        public static T GetRightChildren<T>(T node) where T : unmanaged, IBinaryInteger<T> => node + node + T.One + T.One;

        public static T GetParent<T>(T node) where T : unmanaged, IBinaryInteger<T> => (node - T.One) >>> 1;
    }
}
