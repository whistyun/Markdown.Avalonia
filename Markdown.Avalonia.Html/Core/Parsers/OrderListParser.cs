using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using Markdown.Avalonia.Html.Core.Utils;
using System.Windows;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using System.Reflection;
using ColorTextBlock.Avalonia;
using Avalonia.Media;

namespace Markdown.Avalonia.Html.Core.Parsers
{
    public class OrderListParser : IBlockTagParser
    {
        public IEnumerable<string> SupportTag => new[] { "ol" };

        bool ITagParser.TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<StyledElement> generated)
        {
            var rtn = TryReplace(node, manager, out var list);
            generated = list;
            return rtn;
        }

        public bool TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<Control> generated)
        {
            var list = new Grid();
            list.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            list.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));

            int index = 0;
            int order = 1;

            var startAttr = node.Attributes["start"];
            if (startAttr is not null && Int32.TryParse(startAttr.Value, out var start))
            {
                order = start;
            }

            foreach (var listItemTag in node.ChildNodes.CollectTag("li"))
            {
                var itemContent = manager.ParseChildrenAndGroup(listItemTag);

                var markerTxt = new CTextBlock(order + ".");
                markerTxt.TextAlignment = TextAlignment.Right;
                markerTxt.TextWrapping = TextWrapping.NoWrap;
                markerTxt.Classes.Add(global::Markdown.Avalonia.Markdown.ListMarkerClass);

                var item = CreateItem(itemContent);

                list.RowDefinitions.Add(new RowDefinition());
                list.Children.Add(markerTxt);
                list.Children.Add(item);

                Grid.SetRow(markerTxt, index);
                Grid.SetColumn(markerTxt, 0);

                Grid.SetRow(item, index);
                Grid.SetColumn(item, 1);

                ++index;
                ++order;
            }

            generated = new[] { list };
            return true;
        }

        private StackPanel CreateItem(IEnumerable<Control> children)
        {
            var panel = new StackPanel() { Orientation = Orientation.Vertical };
            foreach (var child in children)
                panel.Children.Add(child);

            return panel;
        }
    }
}
