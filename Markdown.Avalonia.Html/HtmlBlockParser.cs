using Markdown.Avalonia.Html.Core;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Markdown.Avalonia.Parsers;
using Markdown.Avalonia;
using ColorDocument.Avalonia;
using Markdown.Avalonia.SyntaxHigh;
using Markdown.Avalonia.Plugins;

namespace Markdown.Avalonia.Html
{
    public class HtmlBlockParser : BlockParser2
    {
        private static readonly Regex s_emptyLine = new Regex("\n{2,}", RegexOptions.Compiled);
        private static readonly Regex s_headTagPattern = new(@"^<[\t ]*(?'tagname'[a-z][a-z0-9]*)(?'attributes'[ \t][^>]*|/)?>",
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex s_tagPattern = new(@"<(?'close'/?)[\t ]*(?'tagname'[a-z][a-z0-9]*)(?'attributes'[ \t][^>]*|/)?>",
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private ReplaceManager _replacer;

        public HtmlBlockParser(SyntaxHighlight highlight, SetupInfo info) : base(s_headTagPattern, nameof(HtmlBlockParser))
        {
            _replacer = new ReplaceManager(highlight, info, false);
        }

        public override IEnumerable<DocumentElement>? Convert2(
            string text,
            Match firstMatch,
            ParseStatus status,
            IMarkdownEngine2 engine,
            out int parseTextBegin, out int parseTextEnd)
        {
            parseTextBegin = firstMatch.Index;
            parseTextEnd = SimpleHtmlUtils.SearchTagRangeContinuous(text, firstMatch);

            _replacer.Engine = engine;

            var textchip = text.Substring(parseTextBegin, parseTextEnd - parseTextBegin);

            return _replacer.Parse(textchip);
        }
    }
}
