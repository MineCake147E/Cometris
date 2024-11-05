using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cometris.Boards
{
    public struct SquareDisplayBuilder
    {
        int upperStreakCount = -1;
        uint lineCount = 0;
        ushort previousItem = 0;
        readonly List<(uint offsetFromUpper, uint streakCount, ushort blocks)> lineSections;
        public SquareDisplayBuilder()
        {
            lineSections = [];
        }

        public SquareDisplayBuilder(int capacity)
        {
            lineSections = new(capacity);
        }

        public void Append(params ReadOnlySpan<ushort> lines)
        {
            if (lines.IsEmpty) return;
            if (upperStreakCount < 0)
            {
                previousItem = lines[^1];
                upperStreakCount = 0;
                lineCount++;
                if (lines.Length <= 1) return;
                lines = lines.Slice(0, lines.Length - 1);
            }
            var lc = lineCount;
            var prev = previousItem;
            for (int i = lines.Length - 1; i >= 0; i--)
            {
                var item = lines[i];
                if (item != prev)
                {
                    lineSections.Add((lc, (uint)upperStreakCount, prev));
                    upperStreakCount = 1;
                }
                else
                {
                    upperStreakCount++;
                }
                prev = item;
                lc++;
            }
            previousItem = prev;
            lineCount = lc;
        }
    }
}
