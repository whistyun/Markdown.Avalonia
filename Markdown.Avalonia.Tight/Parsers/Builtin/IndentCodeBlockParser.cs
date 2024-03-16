using ColorDocument.Avalonia;
using ColorDocument.Avalonia.DocumentElements;
using Markdown.Avalonia.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Markdown.Avalonia.Parsers.Builtin
{
    internal class IndentCodeBlockParser : BlockParser2
    {
        private static readonly Regex _newlinesLeadingTrailing = new(@"^\n+|\n+\z", RegexOptions.Compiled);
        private static readonly Regex _indentCodeBlock = new(@"
                    (?:\A|^[ ]*\n)
                    (
                    [ ]{4}.+
                    (\n([ ]{4}.+|[ ]*))*
                    \n?
                    )
                    ", RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.Compiled);

        public IndentCodeBlockParser() : base(_indentCodeBlock, "CodeBlocksWithoutLangEvaluator")
        {
        }

        public override IEnumerable<DocumentElement>? Convert2(string text, Match firstMatch, ParseStatus status, IMarkdownEngine2 engine, out int parseTextBegin, out int parseTextEnd)
        {
            parseTextBegin = firstMatch.Index;
            parseTextEnd = firstMatch.Index + firstMatch.Length;

            var detentTxt = String.Join("\n", firstMatch.Groups[1].Value.Split('\n').Select(line => TextUtil.DetentLineBestEffort(line, 4)));
            var border = FencedCodeBlockParser.Create(_newlinesLeadingTrailing.Replace(detentTxt, ""));
            return new[] { new UnBlockElement(border) };
        }
    }
}
