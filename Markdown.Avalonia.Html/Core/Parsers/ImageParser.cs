using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using ColorTextBlock.Avalonia;
using HtmlAgilityPack;
using Markdown.Avalonia.Html.Core.Utils;
using Markdown.Avalonia.Plugins;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Markdown.Avalonia.Html.Core.Parsers
{
    public class ImageParser : IInlineTagParser
    {
        private SetupInfo _setupInfo;

        public ImageParser(SetupInfo info)
        {
            _setupInfo = info;
        }

        public IEnumerable<string> SupportTag => new[] { "img", "image" };

        bool ITagParser.TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<StyledElement> generated)
        {
            var rtn = TryReplace(node, manager, out var list);
            generated = list;
            return rtn;
        }

        public bool TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<CInline> generated)
        {
            var link = node.Attributes["src"]?.Value;
            var alt = node.Attributes["alt"]?.Value;
            if (link is null)
            {
                generated = EnumerableExt.Empty<CInline>();
                return false;
            }
            var title = node.Attributes["title"]?.Value;
            var widthTxt = node.Attributes["width"]?.Value;
            var heightTxt = node.Attributes["height"]?.Value;


            CImage image = _setupInfo.LoadImage(link);
            if (!String.IsNullOrEmpty(title)
                && !title.Any(ch => !Char.IsLetterOrDigit(ch)))
            {
                image.Classes.Add(title);
            }


            if (Length.TryParse(heightTxt, out var heightLen))
            {
                if (heightLen.Unit == Unit.Percentage)
                {
                    image.Bind(CImage.LayoutHeightProperty,
                               new Binding(nameof(Layoutable.Height))
                               {
                                   RelativeSource = new RelativeSource()
                                   {
                                       Mode = RelativeSourceMode.FindAncestor,
                                       AncestorType = typeof(CTextBlock),
                                   },
                                   Converter = new MultiplyConverter(heightLen.Value / 100)
                               });
                }
                else
                {
                    image.LayoutHeight = heightLen.ToPoint();
                }
            }

            // Bind size so document is updated when image is downloaded
            if (Length.TryParse(widthTxt, out var widthLen))
            {
                if (widthLen.Unit == Unit.Percentage)
                {
                    image.Bind(CImage.LayoutHeightProperty,
                               new Binding(nameof(Layoutable.Width))
                               {
                                   RelativeSource = new RelativeSource()
                                   {
                                       Mode = RelativeSourceMode.FindAncestor,
                                       AncestorType = typeof(CTextBlock),
                                   },
                                   Converter = new MultiplyConverter(widthLen.Value / 100)
                               });
                }
                else
                {
                    if (image.LayoutHeight.HasValue)
                        image.LayoutWidth = widthLen.ToPoint();
                    else
                        image.RelativeWidth = widthLen.ToPoint();
                }
            }

            generated = new[] { image };
            return true;
        }

        class MultiplyConverter : IValueConverter
        {
            public double Value { get; }

            public MultiplyConverter(double v)
            {
                Value = v;
            }

            public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            {
                return value is null ? 0d : Value * (Double)value;
            }

            public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            {
                return value is null ? 0d : ((Double)value) / Value;
            }
        }
    }
}
