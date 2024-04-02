using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using MikoMino;

namespace Cometris.Pieces
{
    public readonly struct PiecePlacement : IPiecePlacement<PiecePlacement>
    {
        public Point Position { get; init; }
        public Piece Piece { get; init; }
        public Angle Angle { get; init; }

        public PiecePlacement(Point position, Piece piece, Angle angle)
        {
            Position = position;
            Piece = piece;
            Angle = angle;
        }

        public PiecePlacement(CompressedPiecePlacement compressed)
        {
            this = (PiecePlacement)compressed;
        }

        public static ulong CalculateHashCode(PiecePlacement value) => (ulong)value.GetHashCode();
    }
}
