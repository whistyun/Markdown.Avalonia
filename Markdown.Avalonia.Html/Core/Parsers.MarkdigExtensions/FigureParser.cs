using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using HtmlAgilityPack;
using Markdown.Avalonia.Html.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Markdown.Avalonia.Html.Core.Parsers.MarkdigExtensions
{
    public class FigureParser : IBlockTagParser
    {
        public IEnumerable<string> SupportTag => new[] { "figure" };

        bool ITagParser.TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<StyledElement> generated)
        {
            var rtn = TryReplace(node, manager, out var list);
            generated = list;
            return rtn;
        }

        public bool TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<Control> generated)
        {
            var captionPair =
                node.ChildNodes
                    .SkipComment()
                    .Filter(nd => string.Equals(nd.Name, "figcaption", StringComparison.OrdinalIgnoreCase));

            var captionList = captionPair.Item1;
            var contentList = captionPair.Item2;


            var captionBlock = captionList.SelectMany(c => manager.Grouping(manager.ParseBlockAndInline(c)));
            var contentBlock = contentList.SelectMany(c => manager.Grouping(manager.ParseChildrenJagging(c)));

            var section = new DockPanel() { LastChildFill = true };
            section.Tag = Tags.TagFigure.GetClass();

            foreach (var caption in captionBlock)
            {
                DockPanel.SetDock(caption, Dock.Top);
                section.Children.Add(caption);
            }

            var contentPanel = new StackPanel() { Orientation = Orientation.Vertical };
            foreach (var content in contentBlock)
            {
                contentPanel.Children.Add(content);
            }
            section.Children.Add(contentPanel);

            generated = new[] { section };
            return false;
        }
    }
}
