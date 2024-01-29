using System.Buffers;

using Cometris.Boards;
using Cometris.Movements;
using Cometris.Movements.Reachability;
using Cometris.Pieces;
using Cometris.Pieces.Mobility;
using Cometris.Utils;

namespace Cometris.Tests.Integration
{
    public sealed class SimpleRouteFinder<TBitBoard>
        where TBitBoard : unmanaged, IBitBoard<TBitBoard, ushort>
    {
        private static void FindNextStepsWithPiece<TBufferWriter>(TBitBoard board, Piece piece, AngleTuple<TBufferWriter> writer)
            where TBufferWriter : IBufferWriter<CompressedPositionsTuple>
        {
            switch (piece)
            {
                case Piece.T:
                    FindNextStepsAsymmetric<PieceTMovablePointLocater<TBitBoard>, AsymmetricPieceReachablePointLocater<TBitBoard, PieceTRotatabilityLocator<TBitBoard>>>(board, writer.As<TBufferWriter, IBufferWriter<CompressedPositionsTuple>>());
                    return;
                case Piece.I:
                    FindNextStepsTwoRotationSymmetric<PieceIMovablePointLocater<TBitBoard>, TwoRotationSymmetricPieceReachablePointLocater<TBitBoard, PieceIRotatabilityLocator<TBitBoard>, PieceIMovablePointLocater<TBitBoard>>>(board, writer.As<TBufferWriter, IBufferWriter<CompressedPositionsTuple>>());
                    return;
                case Piece.O:
                    FindNextStepsSymmetric<PieceOMovablePointLocater<TBitBoard>, SymmetricPieceReachablePointLocater<TBitBoard>>(board, writer.As<TBufferWriter, IBufferWriter<CompressedPositionsTuple>>());
                    return;
                case Piece.J:
                    FindNextStepsAsymmetric<PieceJMovablePointLocater<TBitBoard>, AsymmetricPieceReachablePointLocater<TBitBoard, PieceJLSZRotatabilityLocator<TBitBoard>>>(board, writer.As<TBufferWriter, IBufferWriter<CompressedPositionsTuple>>());
                    return;
                case Piece.L:
                    FindNextStepsAsymmetric<PieceLMovablePointLocater<TBitBoard>, AsymmetricPieceReachablePointLocater<TBitBoard, PieceJLSZRotatabilityLocator<TBitBoard>>>(board, writer.As<TBufferWriter, IBufferWriter<CompressedPositionsTuple>>());
                    return;
                case Piece.S:
                    FindNextStepsTwoRotationSymmetric<PieceSMovablePointLocater<TBitBoard>, TwoRotationSymmetricPieceReachablePointLocater<TBitBoard, PieceJLSZRotatabilityLocator<TBitBoard>, PieceSMovablePointLocater<TBitBoard>>>(board, writer.As<TBufferWriter, IBufferWriter<CompressedPositionsTuple>>());
                    return;
                case Piece.Z:
                    FindNextStepsTwoRotationSymmetric<PieceZMovablePointLocater<TBitBoard>, TwoRotationSymmetricPieceReachablePointLocater<TBitBoard, PieceJLSZRotatabilityLocator<TBitBoard>, PieceZMovablePointLocater<TBitBoard>>>(board, writer.As<TBufferWriter, IBufferWriter<CompressedPositionsTuple>>());
                    return;
            }
        }

        private static void FindNextStepsAsymmetric<TPieceMovablePointLocater, TPieceReachablePointLocater>(TBitBoard board, AngleTuple<IBufferWriter<CompressedPositionsTuple>> writer)
            where TPieceMovablePointLocater : IAsymmetricPieceMovablePointLocater<TBitBoard>
            where TPieceReachablePointLocater : IAsymmetricPieceReachablePointLocater<TBitBoard>
        {
            var mobility = TPieceMovablePointLocater.LocateMovablePoints(board);
            var (upper, right, lower, left) = TPieceReachablePointLocater.LocateHardDropReachablePoints(TBitBoard.CreateSingleBlock(7, 19), mobility);
            var hardDropPlaceableUpper = upper & ~TBitBoard.ShiftUpOneLine(upper, 0);
            var hardDropPlaceableRight = right & ~TBitBoard.ShiftUpOneLine(right, 0);
            var hardDropPlaceableLower = lower & ~TBitBoard.ShiftUpOneLine(lower, 0);
            var hardDropPlaceableLeft = left & ~TBitBoard.ShiftUpOneLine(left, 0);
            _ = TBitBoard.LocateAllBlocks(hardDropPlaceableUpper, writer.Upper);
            _ = TBitBoard.LocateAllBlocks(hardDropPlaceableRight, writer.Right);
            _ = TBitBoard.LocateAllBlocks(hardDropPlaceableLower, writer.Lower);
            _ = TBitBoard.LocateAllBlocks(hardDropPlaceableLeft, writer.Left);
        }

        private static void FindNextStepsTwoRotationSymmetric<TPieceMovablePointLocater, TPieceReachablePointLocater>(TBitBoard board, AngleTuple<IBufferWriter<CompressedPositionsTuple>> writer)
            where TPieceMovablePointLocater : ITwoRotationSymmetricPieceMovablePointLocater<TBitBoard>
            where TPieceReachablePointLocater : ITwoRotationSymmetricPieceReachablePointLocater<TBitBoard>
        {
            var mobility = TPieceMovablePointLocater.LocateSymmetricMovablePoints(board);
            var hardDropReachable = TPieceReachablePointLocater.LocateHardDropReachablePoints(TBitBoard.CreateSingleBlock(7, 19), mobility);
            var hardDropPlaceableUpper = hardDropReachable.upper & ~TBitBoard.ShiftUpOneLine(hardDropReachable.upper, 0);
            var hardDropPlaceableRight = hardDropReachable.right & ~TBitBoard.ShiftUpOneLine(hardDropReachable.right, 0);
            var hardDropPlaceableLower = hardDropReachable.lower & ~TBitBoard.ShiftUpOneLine(hardDropReachable.lower, 0);
            var hardDropPlaceableLeft = hardDropReachable.left & ~TBitBoard.ShiftUpOneLine(hardDropReachable.left, 0);
            (var upper, var right) = TPieceMovablePointLocater.MergeToTwoRotationSymmetricMobility((hardDropPlaceableUpper, hardDropPlaceableRight, hardDropPlaceableLower, hardDropPlaceableLeft));
            _ = TBitBoard.LocateAllBlocks(upper, writer.Upper);
            _ = TBitBoard.LocateAllBlocks(right, writer.Right);
        }

        private static void FindNextStepsSymmetric<TPieceMovablePointLocater, TPieceReachablePointLocater>(TBitBoard board, AngleTuple<IBufferWriter<CompressedPositionsTuple>> writer)
            where TPieceMovablePointLocater : ISymmetricPieceMovablePointLocater<TBitBoard>
            where TPieceReachablePointLocater : ISymmetricPieceReachablePointLocater<TBitBoard>
        {
            var mobility = TPieceMovablePointLocater.LocateSymmetricMovablePoints(board);
            var hardDropReachable = TPieceReachablePointLocater.LocateHardDropReachablePoints(TBitBoard.CreateSingleBlock(7, 19), mobility);
            var hardDropPlaceable = hardDropReachable & ~TBitBoard.ShiftUpOneLine(hardDropReachable, 0);
            _ = TBitBoard.LocateAllBlocks(hardDropPlaceable, writer.Upper);
        }
    }
}
