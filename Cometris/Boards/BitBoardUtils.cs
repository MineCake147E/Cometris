using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;

using MikoMino.Blocks;

using Shamisen;

namespace Cometris.Boards
{
    public static class BitBoardUtils
    {
        #region Conversion

        public static void ConvertFullToPartial384(Span<PartialBitBoard384> destination, ReadOnlySpan<FullBitBoard> source)
        {
            //TODO: Avx2 Optimization
            ref var dst = ref MemoryMarshal.GetReference(destination);
            ref var src = ref MemoryMarshal.GetReference(source);
            nint i, length = MathI.Min(destination.Length, source.Length);
            for (i = 0; i < length; i++)
            {
                Unsafe.Add(ref dst, i) = new(Unsafe.Add(ref src, i));
            }
        }


        public static byte[] DecodeFumen64(string fumen)
        {
            fumen = ReformatPadLessBase64(fumen);
            return Convert.FromBase64String(fumen);
        }

        private static string ReformatPadLessBase64(string text)
        {
            text = text.Replace("?", "");
            text += (text.Length & 3) switch
            {
                1 => "A==",
                2 => "==",
                3 => "=",
                _ => ""
            };
            return text;
        }

        public static (PartialBitBoard256X2 upper, PartialBitBoard256X2 right, PartialBitBoard256X2 lower, PartialBitBoard256X2 left) ConvertVerticalSymmetricToAsymmetricMobility(this (PartialBitBoard256X2 upper, PartialBitBoard256X2 right) boards)
            => (boards.upper, boards.right, PartialBitBoard256X2.ShiftUpOneLine(boards.upper, 0), boards.right >> 1);

        public static (PartialBitBoard256X2 upper, PartialBitBoard256X2 right, PartialBitBoard256X2 lower, PartialBitBoard256X2 left) ConvertHorizontalSymmetricToAsymmetricMobility(this (PartialBitBoard256X2 upper, PartialBitBoard256X2 right) boards)
            => (boards.upper, boards.right, boards.upper >> 1, PartialBitBoard256X2.ShiftDownOneLine(boards.right, 0));

        public static (PartialBitBoard256X2 upper, PartialBitBoard256X2 right, PartialBitBoard256X2 lower, PartialBitBoard256X2 left) ConvertOPieceToAsymmetricMobility(this PartialBitBoard256X2 board)
        {
            var rightBoard = PartialBitBoard256X2.ShiftUpOneLine(board, 0);
            return (board, rightBoard, rightBoard >> 1, board >> 1);
        }
        #endregion

        #region Operation

        #endregion

        #region Visualization
        public static void AppendSquareDisplay(this StringBuilder sb, ReadOnlySpan<ushort> source)
        {
            var a = source;
            var upperStreak = true;
            var upperStreakCount = 0;
            var previousItem = a[^1];
            for (var i = a.Length - 1; i >= 0; i--)
            {
                var item = a[i];
                if (!upperStreak | item != previousItem)
                {
                    _ = sb.Append($"{i + 1,2}: ");
                    _ = sb.Append(VisualizeLine(previousItem));
                    _ = sb.AppendLine(!upperStreak | (upperStreakCount < 2) ? "" : $" x{upperStreakCount}");
                    upperStreakCount = 1;
                }
                else
                {
                    upperStreakCount++;
                }
                previousItem = item;
            }
            _ = sb.Append($"{0,2}: ");
            _ = sb.Append(VisualizeLine(previousItem));
            _ = sb.Append(!upperStreak | (upperStreakCount < 2) ? "" : $" x{upperStreakCount}");
        }

        private static string VisualizeLine(ushort line) => Convert.ToString(line, 2).PadLeft(16, '0').Replace('0', '□').Replace('1', '■');

        public static string VisualizeOrientations<TBitBoard>((TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) boards)
            where TBitBoard : unmanaged, IOperableBitBoard<TBitBoard, ushort>
        {
            (var upper, var right, var lower, var left) = boards;
            var sb = new StringBuilder();
            var f16 = new FixedArray16<char>();
            Span<char> buffer = f16;
            var upperStreak = true;
            var upperStreakCount = 0;
            ulong previousItem = 0;
            for (var i = TBitBoard.Height - 1; i >= 0; i--)
            {
                uint upperLine = upper[i];
                uint rightLine = right[i];
                uint lowerLine = lower[i];
                uint leftLine = left[i];
                var combinedLine = CombineOrientations(upperLine, rightLine, lowerLine, leftLine);
                if (!upperStreak | (combinedLine != previousItem))
                {
                    var st = ConvertCombinedLineToString(buffer, previousItem);
                    if (st is not OperationStatus.Done) throw new InvalidOperationException("Unknown Error!");
                    _ = sb.Append($"{i + 1,2}: ");
                    _ = sb.Append(buffer);
                    _ = sb.AppendLine(!upperStreak | (upperStreakCount < 2) ? "" : $" x{upperStreakCount}");
                    upperStreakCount = 1;
                }
                else
                {
                    upperStreakCount++;
                }
                previousItem = combinedLine;
            }
            if (ConvertCombinedLineToString(buffer, previousItem) != OperationStatus.Done) throw new InvalidOperationException("Unknown Error!");
            _ = sb.Append($"{0,2}: ");
            _ = sb.Append(buffer);
            _ = sb.Append(!upperStreak | (upperStreakCount < 2) ? "" : $" x{upperStreakCount}");
            return sb.ToString();
        }

        public static string VisualizeOrientations<TBitBoard>((TBitBoard upper, TBitBoard right, TBitBoard lower, TBitBoard left) mob, TBitBoard board)
            where TBitBoard : unmanaged, IOperableBitBoard<TBitBoard, ushort>
        {
            (var upper, var right, var lower, var left) = mob;
            var sb = new StringBuilder();
            var f16 = new FixedArray16<char>();
            Span<char> buffer = f16;
            var upperStreak = true;
            var upperStreakCount = 0;
            ulong previousItem = 0;
            ushort previousBgLine = 0;
            for (var i = TBitBoard.Height - 1; i >= 0; i--)
            {
                var bgLine = board[i];
                uint upperLine = upper[i];
                uint rightLine = right[i];
                uint lowerLine = lower[i];
                uint leftLine = left[i];
                var combinedLine = CombineOrientations(upperLine, rightLine, lowerLine, leftLine);
                if (!upperStreak | (combinedLine != previousItem) | (bgLine != previousBgLine))
                {
                    var st = ConvertCombinedLineToString(buffer, previousItem, previousBgLine);
                    if (st is not OperationStatus.Done) throw new InvalidOperationException("Unknown Error!");
                    _ = sb.Append($"{i + 1,2}: ");
                    _ = sb.Append(buffer);
                    _ = sb.AppendLine(!upperStreak | (upperStreakCount < 2) ? "" : $" x{upperStreakCount}");
                    upperStreakCount = 1;
                }
                else
                {
                    upperStreakCount++;
                }
                previousItem = combinedLine;
                previousBgLine = bgLine;
            }
            if (ConvertCombinedLineToString(buffer, previousItem, previousBgLine) != OperationStatus.Done) throw new InvalidOperationException("Unknown Error!");
            _ = sb.Append($"{0,2}: ");
            _ = sb.Append(buffer);
            _ = sb.Append(!upperStreak | (upperStreakCount < 2) ? "" : $" x{upperStreakCount}");
            return sb.ToString();
        }

        public static string VisualizeDifferences<TBitBoard>(TBitBoard newBoard, TBitBoard board)
            where TBitBoard : unmanaged, IOperableBitBoard<TBitBoard, ushort>
        {
            var sb = new StringBuilder();
            var f16 = new FixedArray16<char>();
            Span<char> buffer = f16;
            var upperStreak = true;
            var upperStreakCount = 1;
            uint previousItem = CombineDifferences(newBoard[TBitBoard.Height - 1], board[TBitBoard.Height - 1]);
            for (var i = TBitBoard.Height - 2; i >= 0; i--)
            {
                var bgLine = board[i];
                uint line = newBoard[i];
                var combinedLine = CombineDifferences(line, bgLine);
                if (!upperStreak | (combinedLine != previousItem))
                {
                    var st = ConvertCombinedDifferenceLineToString(buffer, previousItem);
                    if (st is not OperationStatus.Done) throw new InvalidOperationException("Unknown Error!");
                    _ = sb.Append($"{i + 1,2}: ");
                    _ = sb.Append(buffer);
                    _ = sb.AppendLine(!upperStreak | (upperStreakCount < 2) ? "" : $" x{upperStreakCount}");
                    upperStreakCount = 1;
                }
                else
                {
                    upperStreakCount++;
                }
                previousItem = combinedLine;
            }
            if (ConvertCombinedDifferenceLineToString(buffer, previousItem) != OperationStatus.Done) throw new InvalidOperationException("Unknown Error!");
            _ = sb.Append($"{0,2}: ");
            _ = sb.Append(buffer);
            _ = sb.Append(!upperStreak | (upperStreakCount < 2) ? "" : $" x{upperStreakCount}");
            return sb.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static uint CombineDifferences(uint newLine, uint backgroundLine)
        {
            var combinedLine = 0u;
            if (Bmi2.IsSupported)
            {
                combinedLine = Bmi2.ParallelBitDeposit(backgroundLine, 0x5555_5555u);
                combinedLine |= Bmi2.ParallelBitDeposit(newLine, 0xaaaa_aaaau);
            }
            else
            {
                for (var i = 16 - 1; i >= 0; i--)
                {
                    uint orientation = (backgroundLine >> i) & 1;
                    uint o2 = (newLine >> i) & 1;
                    orientation |= o2 << 1;
                    combinedLine |= orientation << (i * 2);
                }
            }
            return combinedLine;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static ulong CombineOrientations(uint upperLine, uint rightLine, uint lowerLine, uint leftLine)
        {
            var combinedLine = 0ul;
            if (Bmi2.X64.IsSupported)
            {
                combinedLine = Bmi2.X64.ParallelBitDeposit(upperLine, 0x8888_8888_8888_8888ul);
                combinedLine |= Bmi2.X64.ParallelBitDeposit(rightLine, 0x4444_4444_4444_4444ul);
                combinedLine |= Bmi2.X64.ParallelBitDeposit(lowerLine, 0x2222_2222_2222_2222ul);
                combinedLine |= Bmi2.X64.ParallelBitDeposit(leftLine, 0x1111_1111_1111_1111ul);
            }
            else
            {
                for (var i = 16 - 1; i >= 0; i--)
                {
                    ulong orientation = (leftLine >> i) & 1;
                    ulong o2 = (rightLine >> i) & 1;
                    orientation |= ((lowerLine >> i) & 1) << 1;
                    o2 |= ((upperLine >> i) & 1) << 1;
                    orientation |= o2 << 2;
                    combinedLine |= orientation << (i * 4);
                }
            }
            return combinedLine;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static OperationStatus ConvertCombinedDifferenceLineToString(Span<char> destination, uint line)
        {
            ReadOnlySpan<char> orientationCombinations = ['□', '※', '╋'];
            ref var ocHead = ref MemoryMarshal.GetReference(orientationCombinations);
            var j = 64;
            for (var i = 0; i < destination.Length; i++)
            {
                j -= 2;
                var combinedBlock = (line >> j) & 0x3;
                destination[i] = combinedBlock >= orientationCombinations.Length ? '■' : Unsafe.Add(ref ocHead, combinedBlock);
            }
            return OperationStatus.Done;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static OperationStatus ConvertCombinedLineToString(Span<char> destination, ulong line)
        {
            ReadOnlySpan<char> orientationCombinations = ['□', '←', '↓', '┓', '→', '━', '┏', '┳', '↑', '┛', '┃', '┫', '┗', '┻', '┣', '╋'];
            ref var ocHead = ref MemoryMarshal.GetReference(orientationCombinations);
            var j = 64;
            for (var i = 0; i < destination.Length; i++)
            {
                j -= 4;
                var combinedBlock = (int)(line >> j) & 0xf;
                destination[i] = Unsafe.Add(ref ocHead, combinedBlock);
            }
            return OperationStatus.Done;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static OperationStatus ConvertCombinedLineToString(Span<char> destination, ulong line, ushort bgLine)
        {
            ReadOnlySpan<char> orientationCombinations = ['□', '←', '↓', '┓', '→', '━', '┏', '┳', '↑', '┛', '┃', '┫', '┗', '┻', '┣', '╋'];
            ref var ocHead = ref MemoryMarshal.GetReference(orientationCombinations);
            var j = 64;
            for (var i = 0; i < destination.Length; i++)
            {
                j -= 4;
                var combinedBlock = (int)(line >> j) & 0xf;
                var bgBlock = (bgLine << i) & 0x8000;
                destination[i] = bgBlock > 0 ? '■' : Unsafe.Add(ref ocHead, combinedBlock);
            }
            return OperationStatus.Done;
        }
        #endregion
    }
}
