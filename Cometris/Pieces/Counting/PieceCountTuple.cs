using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace Cometris.Pieces.Counting
{
    public readonly struct PieceCountTuple
    {
        private readonly double medium;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PieceCountTuple(Piece initialPiece, byte count = 1)
        {
            var x8 = (byte)initialPiece * 8;
            var s0 = BitConverter.UInt64BitsToDouble((ulong)count << x8);
            medium = s0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PieceCountTuple(double medium)
        {
            this.medium = medium;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PieceCountTuple(Vector128<byte> medium)
        {
            this.medium = medium.AsDouble().GetElement(0);
        }

        public byte this[Piece piece]
        {
            get
            {
                var x8 = (byte)piece * 8;
                var x9 = BitConverter.DoubleToUInt64Bits(medium);
                return (byte)(x9 >> x8);
            }
        }

        public readonly PieceCountTuple AddPieces(Piece piece, byte count = 1)
        {
            var x8 = (byte)piece * 8;
            var x9 = (ulong)count << x8;
            var v0_8b = Vector128.CreateScalarUnsafe(medium).AsByte();
            var v1_8b = Vector128.CreateScalarUnsafe(x9).AsByte();
            return new(v0_8b + v1_8b);
        }

        public static PieceCountTuple operator +(PieceCountTuple left, PieceCountTuple right)
        {
            var v0_8b = Vector128.CreateScalarUnsafe(left.medium).AsByte();
            var v1_8b = Vector128.CreateScalarUnsafe(right.medium).AsByte();
            return new(v0_8b + v1_8b);
        }
    }
}
