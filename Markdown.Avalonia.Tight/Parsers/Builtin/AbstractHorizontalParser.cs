using Avalonia.Controls;
using ColorDocument.Avalonia;
using ColorDocument.Avalonia.DocumentElements;
using Markdown.Avalonia.Controls;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Markdown.Avalonia.Parsers.Builtin
{
    internal abstract class AbstractHorizontalParser : BlockParser2
    {
        protected AbstractHorizontalParser(Regex pattern) : base(pattern, "RuleEvaluator")
        {
        }

        public override IEnumerable<DocumentElement>? Convert2(string text, Match firstMatch, ParseStatus status, IMarkdownEngine2 engine, out int parseTextBegin, out int parseTextEnd)
        {
            parseTextBegin = firstMatch.Index;
            parseTextEnd = parseTextBegin + firstMatch.Length;

            return new[] { new UnBlockElement(RuleEvaluator(firstMatch)) };
        }

        public override IEnumerable<Control>? Convert(string text, Match firstMatch, ParseStatus status, IMarkdownEngine engine, out int parseTextBegin, out int parseTextEnd)
        {
            parseTextBegin = firstMatch.Index;
            parseTextEnd = parseTextBegin + firstMatch.Length;

            return new[] { RuleEvaluator(firstMatch) };
        }

        private static Rule RuleEvaluator(Match match)
        {
            return match.Groups[1].Value switch
            {
                "=" => new Rule(RuleType.TwoLines),
                "*" => new Rule(RuleType.Bold),
                "_" => new Rule(RuleType.BoldWithSingle),
                "-" => new Rule(RuleType.Single),
                _ => new Rule(RuleType.Single),
            };
        }
    }
}
