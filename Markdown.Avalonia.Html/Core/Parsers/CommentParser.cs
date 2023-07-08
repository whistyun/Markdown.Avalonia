using Avalonia;
using Avalonia.Controls;
using ColorTextBlock.Avalonia;
using HtmlAgilityPack;
using Markdown.Avalonia.Html.Core.Utils;
using System;
using System.Collections.Generic;

namespace Markdown.Avalonia.Html.Core.Parsers
{
    /// <summary>
    /// remove comment element
    /// </summary>
    public class CommentParsre : IBlockTagParser, IInlineTagParser
    {
        public IEnumerable<string> SupportTag => new[] { HtmlNode.HtmlNodeTypeNameComment };

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
