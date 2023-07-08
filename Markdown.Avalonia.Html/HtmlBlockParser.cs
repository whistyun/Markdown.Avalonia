using Markdown.Avalonia.Html.Core;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Markdown.Avalonia.Parsers;
using Markdown.Avalonia;
using Avalonia.Controls;
using Markdown.Avalonia.SyntaxHigh;
using Markdown.Avalonia.Plugins;

namespace Markdown.Avalonia.Html
{
    public class HtmlBlockParser : BlockParser
    {
        private static readonly Regex s_emptyLine = new Regex("\n{2,}", RegexOptions.Compiled);
        private static readonly Regex s_headTagPattern = new(@"^<[\t ]*(?'tagname'[a-z][a-z0-9]*)(?'attributes'[ \t][^>]*|/)?>",
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex s_tagPattern = new(@"<(?'close'/?)[\t ]*(?'tagname'[a-z][a-z0-9]*)(?'attributes'[ \t][^>]*|/)?>",
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private ReplaceManager _replacer;

        public HtmlBlockParser(SyntaxHighlight highlight, SetupInfo info) : base(s_headTagPattern, nameof(HtmlBlockParser))
        {
            _replacer = new ReplaceManager(highlight, info);
        }

        public override IEnumerable<Control> Convert(
            string text,
            Match firstMatch,
            ParseStatus status,
            IMarkdownEngine engine,
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
