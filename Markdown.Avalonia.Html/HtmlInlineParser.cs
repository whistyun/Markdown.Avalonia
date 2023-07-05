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
        private readonly ReplaceManager _replacer;

        public HtmlInlineParser(SyntaxHighlight highlight, SetupInfo info) : this(new ReplaceManager(highlight, info)) { }

        private HtmlInlineParser(ReplaceManager replacer) : base(SimpleHtmlUtils.CreateTagstartPattern(replacer.InlineTags), nameof(HtmlInlineParser))
        {
            _replacer = replacer;
            FirstMatchPattern = SimpleHtmlUtils.CreateTagstartPattern(_replacer.InlineTags);
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

            _replacer.Engine = engine;

            return _replacer.ParseInline(text.Substring(parseTextBegin, parseTextEnd - parseTextBegin));
        }
    }
}
