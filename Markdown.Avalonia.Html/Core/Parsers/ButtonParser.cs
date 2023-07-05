using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using ColorTextBlock.Avalonia;
using HtmlAgilityPack;
using System.Collections.Generic;

namespace Markdown.Avalonia.Html.Core.Parsers
{
    public class ButtonParser : IInlineTagParser
    {
        public IEnumerable<string> SupportTag => new[] { "button" };

        bool ITagParser.TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<StyledElement> generated)
        {
            var rtn = TryReplace(node, manager, out var list);
            generated = list;
            return rtn;
        }

        public bool TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<CInline> generated)
        {
            var doc = new StackPanel() { Orientation = Orientation.Vertical };
            doc.Children.AddRange(manager.ParseChildrenAndGroup(node));

            doc.Loaded += (s, e) =>
            {
                var desiredWidth = doc.DesiredSize.Width;
                var desiredHeight = doc.DesiredSize.Height;


                for (int i = 0; i < 10; ++i)
                {
                    desiredWidth /= 2;
                    var size = new Size(desiredWidth, double.PositiveInfinity);

                    doc.Measure(size);

                    if (desiredHeight != doc.DesiredSize.Height) break;

                    // Give up because it will not be wrapped back.
                    if (i == 9) return;
                }

                var preferedWidth = desiredWidth * 2;

                for (int i = 0; i < 10; ++i)
                {
                    var width = (desiredWidth + preferedWidth) / 2;

                    var size = new Size(width, double.PositiveInfinity);
                    doc.Measure(size);

                    if (desiredHeight == doc.DesiredSize.Height)
                    {
                        preferedWidth = width;
                    }
                    else
                    {
                        desiredWidth = width;
                    }
                }

                doc.Width = preferedWidth;
            };


            var btn = new Button()
            {
                Content = doc,
                IsEnabled = false,
            };

            generated = new[] { new CInlineUIContainer(btn) };
            return true;
        }
    }
}
