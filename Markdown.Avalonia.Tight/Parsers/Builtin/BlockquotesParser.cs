using ColorDocument.Avalonia;
using ColorDocument.Avalonia.DocumentElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Markdown.Avalonia.Parsers.Builtin
{
    internal class BlockquotesParser : BlockParser2
    {
        private static readonly Regex _blockquoteFirst = new(@"
            ^
            ([>].*)
            (\n[>].*)*
            [\n]*
            ", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private bool _supportTextAlignment;

        public BlockquotesParser(bool supportTextAlignment) : base(_blockquoteFirst, "BlockquotesEvaluator")
        {
            _supportTextAlignment = supportTextAlignment;
        }

        public override IEnumerable<DocumentElement>? Convert2(string text, Match firstMatch, ParseStatus status, IMarkdownEngine2 engine, out int parseTextBegin, out int parseTextEnd)
        {
            parseTextBegin = firstMatch.Index;
            parseTextEnd = firstMatch.Index + firstMatch.Length;

            // trim '>'
            var trimmedTxt = string.Join(
                    "\n",
                    firstMatch.Value.Trim().Split('\n')
                        .Select(txt =>
                        {
                            if (txt.Length <= 1) return string.Empty;
                            var trimmed = txt.Substring(1);
                            if (trimmed.FirstOrDefault() == ' ') trimmed = trimmed.Substring(1);
                            return trimmed;
                        })
                        .ToArray()
            );

            var newStatus = new ParseStatus(true & _supportTextAlignment);
            var blocks = engine.ParseGamutElement(trimmedTxt + "\n", newStatus);

            return new[] { new BlockquoteElement(blocks) };
        }
    }
}
