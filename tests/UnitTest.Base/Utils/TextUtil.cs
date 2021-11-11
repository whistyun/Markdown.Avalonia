using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace UnitTest.Base.Utils
{
    public static class TextUtil
    {
        public static string HereDoc(string value)
        {
            // like PHP's flexible_heredoc_nowdoc_syntaxes,
            // The indentation of the closing tag dictates 
            // the amount of whitespace to strip from each line 
            var lines = Regex.Split(value, "\r\n|\r|\n", RegexOptions.Multiline);

            // count last line indent
            int lastIdtCnt = TextUtil.CountIndent(lines.Last());
            // count full indent
            int someIdtCnt = lines
                .Where(line => !String.IsNullOrWhiteSpace(line))
                .Select(line => TextUtil.CountIndent(line))
                .Min();

            var indentCount = Math.Max(lastIdtCnt, someIdtCnt);

            return String.Join(
                "\n",
                lines
                    // skip first blank line
                    .Skip(String.IsNullOrWhiteSpace(lines[0]) ? 1 : 0)
                    // strip indent
                    .Select(line =>
                    {
                        var realIdx = 0;
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
                                viewIdx = ((viewIdx >> 2) + 1) << 2;
                            }
                            else break;
                        }

                        return line.Substring(realIdx);
                    })
                );
        }

        private static int CountIndent(string line)
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
    }
}
