using HtmlAgilityPack;
using System.Collections.Generic;
using ColorDocument.Avalonia;
using ColorDocument.Avalonia.DocumentElements;
using Markdown.Avalonia.Html.Core.Utils;
using ListMark=ColorDocument.Avalonia.DocumentElements.TextMarkerStyle;

namespace Markdown.Avalonia.Html.Core.Parsers
{
    public class OrderListParser : IBlockTagParser
    {
        public IEnumerable<string> SupportTag => new[] { "ol" };

        public bool TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<DocumentElement> generated)
        {
            var start = 1;
            var startAttr = node.Attributes["start"];
            if (startAttr is not null && int.TryParse(startAttr.Value, out var parsedStart))
                start = parsedStart;

            var items = new List<ListItemElement>();

            foreach (var listItemTag in node.ChildNodes.CollectTag("li"))
            {
                var children = manager.ParseChildNodes(listItemTag);
                items.Add(new ListItemElement(children));
            }

            generated = new DocumentElement[]
            {
                new ListBlockElement(ListMark.Decimal, items, start),
            };
            return true;
        }
    }
}
