using HtmlAgilityPack;
using System.Collections.Generic;
using ColorDocument.Avalonia;
using ColorDocument.Avalonia.DocumentElements;
using Markdown.Avalonia.Html.Core.Utils;
using ListMark = ColorDocument.Avalonia.DocumentElements.TextMarkerStyle;

namespace Markdown.Avalonia.Html.Core.Parsers
{
    public class UnorderListParser : IBlockTagParser
    {
        public IEnumerable<string> SupportTag => new[] { "ul" };

        public bool TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<DocumentElement> generated)
        {
            var items = new List<ListItemElement>();

            foreach (var listItemTag in node.ChildNodes.CollectTag("li"))
            {
                var children = manager.ParseChildNodes(listItemTag);
                items.Add(new ListItemElement(children));
            }

            generated = new[] {
                new ListBlockElement(ListMark.Disc, items)
            };
            return true;
        }
    }
}
