using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;

using Cometris.Pieces.Counting;

using Cometris.Pieces;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;

namespace Cometris.Collections
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public readonly struct BagPieceSet : IImmutableSet<Piece>
    {
        public CombinablePieces Value { get; }
        public int Count => BitOperations.PopCount((byte)Value);

        public bool IsEmpty => Value == CombinablePieces.None;

        public static BagPieceSet Empty => new(CombinablePieces.None);

        public static BagPieceSet All => new(CombinablePieces.All);

        public BagPieceSet(CombinablePieces pieces)
        {
            Value = pieces;
        }

        public BagPieceSet(params ReadOnlySpan<Piece> pieces)
        {
            var m = CombinablePieces.None;
            foreach (var piece in pieces)
            {
                m |= piece.ToFlag();
            }
        }

        public BagPieceSet(PieceCountTuple countTuple)
        {
            if (Vector64.IsHardwareAccelerated)
            {
                var v0_8b = Vector64.CreateScalarUnsafe(countTuple.GetInternalValue()).AsByte();
                v0_8b = Vector64.Equals(v0_8b, Vector64<byte>.Zero);
                this = new((CombinablePieces)~v0_8b.ExtractMostSignificantBits() & CombinablePieces.All);
                return;
            }
            if (Vector128.IsHardwareAccelerated || Sse2.IsSupported)
            {
                var v0_16b = Vector128.CreateScalarUnsafe(countTuple.GetInternalValue()).AsByte();
                v0_16b = Vector128.Equals(v0_16b, Vector128<byte>.Zero);
                this = new((CombinablePieces)~v0_16b.ExtractMostSignificantBits() & CombinablePieces.All);
                return;
            }
        }

        public static BagPieceSet Create<TEnumerable>(TEnumerable pieces) where TEnumerable : IEnumerable<Piece>, allows ref struct
        {
            var m = CombinablePieces.None;
            foreach (var piece in pieces)
            {
                m |= piece.ToFlag();
            }
            return new(m);
        }

        public BagPieceSetEnumerator GetEnumerator() => new(Value);
        IEnumerator<Piece> IEnumerable<Piece>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #region IImmutableSet Implementation

        public BagPieceSet Remove(Piece value) => new(Value & ~value.ToFlag());
        public BagPieceSet Add(Piece value) => new(Value | value.ToFlag());
        public static BagPieceSet Clear() => new(CombinablePieces.None);
        public bool Contains(Piece value) => Value.HasFlag(value.ToFlag());
        public bool TryGetValue(Piece equalValue, out Piece actualValue)
        {
            actualValue = equalValue;
            return Contains(equalValue);
        }

        public BagPieceSet Except(BagPieceSet other) => new(Value & ~other.Value);
        public BagPieceSet Except<TEnumerable>(TEnumerable other) where TEnumerable : IEnumerable<Piece>, allows ref struct => Except(other.AsBagPieceSet());
        public BagPieceSet Intersect(BagPieceSet other) => new(Value & other.Value);
        public BagPieceSet Intersect<TEnumerable>(TEnumerable other) where TEnumerable : IEnumerable<Piece>, allows ref struct => Intersect(other.AsBagPieceSet());
        public bool IsProperSubsetOf(BagPieceSet other) => !SetEquals(other) && IsSubsetOf(other);
        public bool IsProperSubsetOf<TEnumerable>(TEnumerable other) where TEnumerable : IEnumerable<Piece>, allows ref struct => IsProperSubsetOf(other.AsBagPieceSet());
        public bool IsProperSupersetOf(BagPieceSet other) => !SetEquals(other) && IsSupersetOf(other);
        public bool IsProperSupersetOf<TEnumerable>(TEnumerable other) where TEnumerable : IEnumerable<Piece>, allows ref struct => IsProperSupersetOf(other.AsBagPieceSet());
        public bool IsSubsetOf(BagPieceSet other) => (Value | other.Value) == other.Value;
        public bool IsSubsetOf<TEnumerable>(TEnumerable other) where TEnumerable : IEnumerable<Piece>, allows ref struct => IsSubsetOf(other.AsBagPieceSet());
        public bool IsSupersetOf(BagPieceSet other) => (Value | other.Value) == Value;
        public bool IsSupersetOf<TEnumerable>(TEnumerable other) where TEnumerable : IEnumerable<Piece>, allows ref struct => IsSupersetOf(other.AsBagPieceSet());
        public bool Overlaps(BagPieceSet other) => (Value & other.Value) > 0;
        public bool Overlaps<TEnumerable>(TEnumerable other) where TEnumerable : IEnumerable<Piece>, allows ref struct => Overlaps(other.AsBagPieceSet());
        public bool SetEquals(BagPieceSet other) => Value == other.Value;
        public bool SetEquals<TEnumerable>(TEnumerable other) where TEnumerable : IEnumerable<Piece>, allows ref struct => SetEquals(other.AsBagPieceSet());
        public BagPieceSet SymmetricExcept(BagPieceSet other) => new(Value ^ other.Value);
        public BagPieceSet SymmetricExcept<TEnumerable>(TEnumerable other) where TEnumerable : IEnumerable<Piece>, allows ref struct => SymmetricExcept(other.AsBagPieceSet());
        public BagPieceSet Union(BagPieceSet other) => new(Value | other.Value);
        public BagPieceSet Union<TEnumerable>(TEnumerable other) where TEnumerable : IEnumerable<Piece>, allows ref struct => Union(other.AsBagPieceSet());

        bool IImmutableSet<Piece>.IsSubsetOf(IEnumerable<Piece> other) => IsSubsetOf(other.AsBagPieceSet());
        bool IImmutableSet<Piece>.IsSupersetOf(IEnumerable<Piece> other) => IsSupersetOf(other.AsBagPieceSet());
        bool IImmutableSet<Piece>.Overlaps(IEnumerable<Piece> other) => Overlaps(other.AsBagPieceSet());
        bool IImmutableSet<Piece>.SetEquals(IEnumerable<Piece> other) => SetEquals(other.AsBagPieceSet());
        bool IImmutableSet<Piece>.IsProperSubsetOf(IEnumerable<Piece> other) => IsProperSubsetOf(other.AsBagPieceSet());
        bool IImmutableSet<Piece>.IsProperSupersetOf(IEnumerable<Piece> other) => IsProperSupersetOf(other.AsBagPieceSet());
        IImmutableSet<Piece> IImmutableSet<Piece>.Add(Piece value) => Add(value);
        IImmutableSet<Piece> IImmutableSet<Piece>.Clear() => Clear();
        IImmutableSet<Piece> IImmutableSet<Piece>.Except(IEnumerable<Piece> other) => Except(other);
        IImmutableSet<Piece> IImmutableSet<Piece>.Intersect(IEnumerable<Piece> other) => Intersect(other);
        IImmutableSet<Piece> IImmutableSet<Piece>.Remove(Piece value) => Remove(value);
        IImmutableSet<Piece> IImmutableSet<Piece>.SymmetricExcept(IEnumerable<Piece> other) => SymmetricExcept(other);
        IImmutableSet<Piece> IImmutableSet<Piece>.Union(IEnumerable<Piece> other) => Union(other);
        #endregion

        public struct BagPieceSetEnumerator(CombinablePieces pieces) : IEnumerator<Piece>
        {
            private readonly CombinablePieces pieces = pieces;
            private CombinablePieces remainingPiece = pieces;

            public Piece Current { get; private set; }

            readonly object IEnumerator.Current => Current;

            public readonly void Dispose() { }
            public bool MoveNext()
            {
                var remainingPiece1 = (uint)(byte)remainingPiece;
                var m = BitOperations.TrailingZeroCount(remainingPiece1);
                Current = (Piece)(byte)(m + 1);
                remainingPiece = (CombinablePieces)(byte)(remainingPiece1 & (remainingPiece1 - 1));
                return m < BitOperations.TrailingZeroCount(0u);
            }
            public void Reset() => remainingPiece = pieces;
        }

        public static implicit operator BagPieceSet(CombinablePieces pieces) => new(pieces);

        public static explicit operator CombinablePieces(BagPieceSet pieces) => pieces.Value;

        public static explicit operator PieceCountTuple(BagPieceSet pieces) => new(pieces);

        private string GetDebuggerDisplay() => $"{Value}";

        public override string ToString() => GetDebuggerDisplay();
    }
}
