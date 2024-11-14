using System;
using System.Collections.Generic;
using System.Numerics;

using Cometris.Collections;

namespace Cometris.Pieces
{
    public enum Piece : byte
    {
        None = 0,
        /// <summary>
        /// The T-Shaped Polyomino piece.
        /// </summary>
        T,
        I,
        O,
        J,
        L,
        S,
        Z
    }

    [Flags]
    public enum CombinablePieces : byte
    {
        None = 0,
        T = 1 << 0,
        I = 1 << 1,
        O = 1 << 2,
        J = 1 << 3,
        L = 1 << 4,
        S = 1 << 5,
        Z = 1 << 6,
        All = T | I | O | J | L | S | Z
    }

    public static class PiecesUtils
    {
        public static CombinablePieces ToFlag(this Piece piece) => (CombinablePieces)(1u << ((int)(uint)(byte)piece - 1));

        public static Piece GetFirstPiece(this CombinablePieces pieces)
        {
            var p = (byte)pieces | 0x80u;
            return (Piece)((BitOperations.TrailingZeroCount(p) + 1) & 7);
        }

        public static Piece GetFirstPieceUnsafe(this CombinablePieces pieces)
        {
            var p = (uint)pieces * 2u;
            return (Piece)BitOperations.TrailingZeroCount(p);
        }

        public static CombinablePieces ClearFirstPiece(this CombinablePieces pieces)
        {
            var p = (uint)(byte)pieces;
            return (CombinablePieces)(p & (p - 1));
        }

        public static BagPieceSet AsBagPieceSet<TEnumerable>(this TEnumerable pieces) where TEnumerable : IEnumerable<Piece>, allows ref struct
            => BagPieceSet.Create(pieces);
    }
}
