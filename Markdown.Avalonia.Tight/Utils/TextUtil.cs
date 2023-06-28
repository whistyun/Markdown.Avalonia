using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Markdown.Avalonia.Utils
{
    static class TextUtil
    {
        /// <summary>
        /// Count the number of leading whilte-spaces.
        /// tab is treated as 4-spaces.
        /// </summary>
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

        /// <summary>
        /// Removes the leading white-space. the number of removed spaces is `indentCount`.
        /// 
        /// If the leading white-space is too short than `indentCount`,
        /// this method removes all leading white-spaces.
        /// </summary>
        public static string DetentLineBestEffort(string line, int indentCount)
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


        /// <summary>
        /// Removes the leading white-space. the number of removed spaces is `indentCount`.
        /// 
        /// If the leading white-space is too short than `indentCount`,
        /// this method return 'false' and `detendedLine` is set null.
        /// </summary>
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
                    detendedLine = string.Empty;
                    return false;
                }
            }

            detendedLine = line.Substring(realIdx);
            return true;
        }

        /// <summary>
        /// convert all tabs to _tabWidth spaces; 
        /// standardizes line endings from DOS (CR LF) or Mac (CR) to UNIX (LF); 
        /// makes sure text ends with a couple of newlines; 
        /// removes any blank lines (only spaces) in the text
        /// </summary>
        public static string Normalize(string text, int tabWidth = 4)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            var output = new StringBuilder(text.Length);
            var line = new StringBuilder();
            bool valid = false;

            for (int i = 0; i < text.Length; i++)
            {
                switch (text[i])
                {
                    case '\n':
                        if (valid)
                            output.Append(line);
                        output.Append('\n');
                        line.Length = 0;
                        valid = false;
                        break;
                    case '\r':
                        if ((i < text.Length - 1) && (text[i + 1] != '\n'))
                        {
                            if (valid)
                                output.Append(line);
                            output.Append('\n');
                            line.Length = 0;
                            valid = false;
                        }
                        break;
                    case '\t':
                        int width = (tabWidth - line.Length % tabWidth);
                        for (int k = 0; k < width; k++)
                            line.Append(' ');
                        break;
                    case '\x1A':
                        break;
                    default:
                        if (!valid && text[i] != ' ')
                            valid = true;
                        line.Append(text[i]);
                        break;
                }
            }

            if (valid)
                output.Append(line);

            if (output.Length >= 1 && output[output.Length - 1] != '\n')
                output.Append('\n');

            // add two newlines to the end before return
            return output.ToString();
        }

    }
}
