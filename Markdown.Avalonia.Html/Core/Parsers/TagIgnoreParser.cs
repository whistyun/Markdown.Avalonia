using Avalonia;
using Avalonia.Controls;
using ColorTextBlock.Avalonia;
using HtmlAgilityPack;
using Markdonw.Avalonia.Html.Core.Utils;
using System.Collections.Generic;

namespace Markdonw.Avalonia.Html.Core.Parsers
{
    public class TagIgnoreParser : IBlockTagParser, IInlineTagParser
    {
        public IEnumerable<string> SupportTag => new[] { "title", "meta", "link", "script", "style", "datalist" };

        bool ITagParser.TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<StyledElement> generated)
        {
            generated = EnumerableExt.Empty<StyledElement>();
            return true;
        }

        public bool TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<Control> generated)
        {
            generated = EnumerableExt.Empty<Control>();
            return true;
        }

        public bool TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<CInline> generated)
        {
            generated = EnumerableExt.Empty<CInline>();
            return true;
        }
    }
}
