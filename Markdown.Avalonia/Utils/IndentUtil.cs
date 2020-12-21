using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Markdown.Avalonia
{
    static class IndentUtil
    {
        public static int CountIndent(string line)
        {
            var count = 0;
            foreach (var c in line)
            {
                if (c == ' ') count += 1;
                else if (c == '\t')
                {
                    // In default in vs, tab is treated as four-spaces.
                    count = ((count >> 2) + 1) << 2;
                }
                else break;
            }
            return count;
        }

        public static string DetentBestEffort(string line, int indentCount)
        {
            // this index count tab as 1: for String.Substring
            var realIdx = 0;
            // this index count tab as 4: for human (I think most text-editor treats tab as 4spaces)
            var viewIdx = 0;

            while (viewIdx < indentCount && realIdx < line.Length)
            {
                var c = line[realIdx];
                if (c == ' ')
                {
                    realIdx += 1;
                    viewIdx += 1;
                }
                else if (c == '\t')
                {
                    realIdx += 1;
                    // when mixing space and tab (ex: space space tab), some space should be ignored.
                    viewIdx = ((viewIdx >> 2) + 1) << 2;
                }

                // give up ded
                else break;
            }

            return line.Substring(realIdx);
        }

        public static bool TryDetendLine(string line, int indentCount, out string detendedLine)
        {
            // this index count tab as 1: for String.Substring
            var realIdx = 0;
            // this index count tab as 4: for human (I think most text-editor treats tab as 4spaces)
            var viewIdx = 0;

            while (viewIdx < indentCount && realIdx < line.Length)
            {
                var c = line[realIdx];
                if (c == ' ')
                {
                    realIdx += 1;
                    viewIdx += 1;
                }
                else if (c == '\t')
                {
                    realIdx += 1;
                    // when mixing space and tab (ex: space space tab), some space should be ignored.
                    viewIdx = ((viewIdx >> 2) + 1) << 2;
                }

                // give up ded
                else
                {
                    detendedLine = null;
                    return false;
                }
            }

            detendedLine = line.Substring(realIdx);
            return true;
        }
    }
}
