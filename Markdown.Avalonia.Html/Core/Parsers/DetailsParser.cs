using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using ColorDocument.Avalonia;
using ColorDocument.Avalonia.DocumentElements;
using ColorTextBlock.Avalonia;
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

        public bool TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<DocumentElement> generated)
        {
            var summary = node.ChildNodes.FirstOrDefault(e => e.IsElement("summary"));
            if (summary is null)
            {
                generated = EnumerableExt.Empty<DocumentElement>();
                return false;
            }

            var content = node.ChildNodes.Where(e => !ReferenceEquals(e, summary));

            var header = Create(manager.Engine, manager.ParseChildNodes(summary));

            var expander = new Expander()
            {
                Header = header,
                Content = Create(manager.Engine, manager.Parse(content)),
            };

            if (node.Attributes["open"] is HtmlAttribute openAttr
                && bool.TryParse(openAttr.Value, out var isOpened))
            {
                expander.IsExpanded = isOpened;
            }

            generated = new DocumentElement[] { new UnBlockElement(expander) };
            return true;
        }

        private static StackPanel Create(IMarkdownEngine2 engine, IEnumerable<object> blocks)
        {
            var doc = new StackPanel() { Orientation = Orientation.Vertical };
            doc.Children.AddRange(
                blocks.Select(e => e switch
                {
                    DocumentElement de => de.Control,
                    CInline inline => (Control)new CTextBlock(inline),
                    _ => null
                }).OfType<Control>());

            return doc;
        }
    }
}
