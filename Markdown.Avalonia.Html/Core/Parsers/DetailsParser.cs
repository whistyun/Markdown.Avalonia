using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using HtmlAgilityPack;
using Markdown.Avalonia.Html.Core.Utils;
using Markdown.Avalonia;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Markdown.Avalonia.Html.Core.Parsers
{
    public class DetailsParser : IBlockTagParser
    {
        public IEnumerable<string> SupportTag => new[] { "details" };

        bool ITagParser.TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<StyledElement> generated)
        {
            var rtn = TryReplace(node, manager, out var list);
            generated = list;
            return rtn;
        }

        public bool TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<Control> generated)
        {
            var summary = node.ChildNodes.FirstOrDefault(e => e.IsElement("summary"));
            if (summary is null)
            {
                generated = EnumerableExt.Empty<Control>();
                return false;
            }

            var content = node.ChildNodes.Where(e => !ReferenceEquals(e, summary));

            var header = Create(manager.Engine, manager.ParseChildrenAndGroup(summary));

            var expander = new Expander()
            {
                Header = header,
                Content = Create(manager.Engine, manager.Grouping(manager.ParseChildrenJagigng(content))),
            };

            if (node.Attributes["open"] is HtmlAttribute openAttr
                && bool.TryParse(openAttr.Value, out var isOpened))
            {
                expander.IsExpanded = isOpened;
            }

            generated = new[] { expander };
            return true;
        }

        private static StackPanel Create(IMarkdownEngine engine, IEnumerable<Control> blocks)
        {
            var doc = new StackPanel() { Orientation = Orientation.Vertical };
            doc.Children.AddRange(blocks);

            return doc;
        }
    }
}
