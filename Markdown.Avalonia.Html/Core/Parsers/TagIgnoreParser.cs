using Avalonia;
using Avalonia.Controls;
using ColorDocument.Avalonia;
using ColorTextBlock.Avalonia;
using HtmlAgilityPack;
using Markdown.Avalonia.Html.Core.Utils;
using System.Collections.Generic;

namespace Markdown.Avalonia.Html.Core.Parsers
{
    public class TagIgnoreParser : IBlockTagParser, IInlineTagParser
    {
        public IEnumerable<string> SupportTag => new[] { "title", "meta", "link", "script", "style", "datalist" };

        public bool TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<DocumentElement> generated)
        {
            generated = EnumerableExt.Empty<DocumentElement>();
            return true;
        }

        public bool TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<CInline> generated)
        {
            generated = EnumerableExt.Empty<CInline>();
            return true;
        }
    }
}
