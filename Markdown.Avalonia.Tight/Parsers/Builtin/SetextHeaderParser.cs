using ColorDocument.Avalonia;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Markdown.Avalonia.Parsers.Builtin
{
    internal class SetextHeaderParser : AbstractHeaderParser
    {
        /// <summary>
        /// Turn Markdown headers into HTML header tags
        /// </summary>
        /// <remarks>
        /// Header 1  
        /// ========  
        /// 
        /// Header 2  
        /// --------  
        /// </remarks>
        private static readonly Regex _headerSetext = new(@"
                ^(.+?)
                [ ]*
                \n
                (=+|-+)     # $1 = string of ='s or -'s
                [ ]*
                \n+",
                RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        public SetextHeaderParser() : base(_headerSetext, "SetextHeaderEvaluator")
        {
        }

        public override IEnumerable<DocumentElement>? Convert2(string text, Match firstMatch, ParseStatus status, IMarkdownEngine2 engine, out int parseTextBegin, out int parseTextEnd)
        {
            parseTextBegin = firstMatch.Index;
            parseTextEnd = parseTextBegin + firstMatch.Length;

            string header = firstMatch.Groups[1].Value;
            int level = firstMatch.Groups[2].Value.StartsWith("=") ? 1 : 2;

            return Create(level, header, engine);
        }
    }
}
