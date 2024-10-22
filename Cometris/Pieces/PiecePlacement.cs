using System;
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

        public static ulong CalculateHashCode(PiecePlacement value) => CompressedPiecePlacement.CalculateHashCode(new(value));
        public static ulong CalculateKeyedHashCode(PiecePlacement value, ulong key) => CompressedPiecePlacement.CalculateKeyedHashCode(new(value), key);
        public override bool Equals(object? obj) => obj is PiecePlacement placement && Equals(placement);
        public bool Equals(PiecePlacement other) => Position.Equals(other.Position) && Piece == other.Piece && Angle == other.Angle;
        public override int GetHashCode() => HashCode.Combine(Position, Piece, Angle);

        public static bool operator ==(PiecePlacement left, PiecePlacement right) => left.Equals(right);
        public static bool operator !=(PiecePlacement left, PiecePlacement right) => !(left == right);
    }
}
