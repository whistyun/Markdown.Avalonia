using ColorTextBlock.Avalonia;
using Markdown.Avalonia.Html.Core;
using Markdown.Avalonia;
using Markdown.Avalonia.Parsers;
using Markdown.Avalonia.Plugins;
using Markdown.Avalonia.SyntaxHigh;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Markdown.Avalonia.Html
{
    public class HtmlInlineParser : InlineParser
    {
        private readonly ReplaceManagerSyntax _syntax;

        public HtmlInlineParser(SyntaxHighlight highlight, SetupInfo info) : this(new ReplaceManagerSyntax(highlight, info, true)) { }

        private HtmlInlineParser(ReplaceManagerSyntax replacer) : base(SimpleHtmlUtils.CreateTagstartPattern(replacer.InlineTags), nameof(HtmlInlineParser))
        {
            _syntax = replacer;
            FirstMatchPattern = SimpleHtmlUtils.CreateTagstartPattern(_syntax.InlineTags);
        }

        public Regex FirstMatchPattern { get; }

        public override IEnumerable<CInline> Convert(
            string text,
            Match firstMatch,
            IMarkdownEngine engine,
            out int parseTextBegin, out int parseTextEnd)
        {
            parseTextBegin = firstMatch.Index;
            parseTextEnd = SimpleHtmlUtils.SearchTagRange(text, firstMatch);

            var replacer = _syntax.Create(engine.Upgrade());
            return replacer.ParseInline(text.Substring(parseTextBegin, parseTextEnd - parseTextBegin));
        }
    }
}
