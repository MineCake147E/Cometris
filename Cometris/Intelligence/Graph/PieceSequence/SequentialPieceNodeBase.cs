using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cometris.Intelligence.Graph.PieceCount;
using Cometris.Pieces;

namespace Cometris.Intelligence.Graph.PieceSequence
{
    public abstract class SequentialPieceNodeBase
    {
        public Piece PieceToPlace { get; }
        public Piece CurrentHold { get; }
        public SequentialPieceNodeBase? Parent { get; }
        public PieceCountNode? CombinatorialSiblings { get; }
        public SequentialPieceNodeBase? HoldReleasingChild { get; }

        public virtual ReadOnlySpan<SequentialPieceNodeBase> Children => [];
    }
}
