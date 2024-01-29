using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cometris.Pieces
{
    [StructLayout(LayoutKind.Explicit, Size = sizeof(ulong) + 2 * sizeof(ushort))]
    public readonly struct PiecePlacement : IPiecePlacement<PiecePlacement>
    {
        [FieldOffset(0)]
        private readonly Point position;
        [FieldOffset(sizeof(ulong))]
        private readonly Piece piece;
        [FieldOffset(sizeof(ulong) + sizeof(Piece))]
        private readonly Angle angle;
        public PiecePlacement(Point position, Piece piece, Angle angle)
        {
            Position = position;
            Piece = piece;
            Angle = angle;
        }

        public PiecePlacement(CompressedPiecePlacement compressed)
        {
            Unsafe.SkipInit(out position);
            Unsafe.SkipInit(out piece);
            Unsafe.SkipInit(out angle);
            this = (PiecePlacement)compressed;
        }

        public Point Position { get => position; init => position = value; }
        public Piece Piece { get => piece; init => piece = value; }
        public Angle Angle { get => angle; init => angle = value; }
    }
}
