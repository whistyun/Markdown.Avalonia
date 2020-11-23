using Avalonia;
using Avalonia.Media;
using Avalonia.Metadata;
using ColorTextBlock.Avalonia.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColorTextBlock.Avalonia
{
    public class CSpan : CInline
    {
        public static readonly StyledProperty<IEnumerable<CInline>> ContentProperty =
            AvaloniaProperty.Register<CSpan, IEnumerable<CInline>>(nameof(Content));

        [Content]
        public IEnumerable<CInline> Content
        {
            get { return GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public CSpan(IEnumerable<CInline> inlines)
        {
            Content = inlines.ToArray();
        }

        protected internal override IEnumerable<CGeometry> Measure(
            FontFamily parentFontFamily,
            double parentFontSize,
            FontStyle parentFontStyle,
            FontWeight parentFontWeight,
            IBrush parentForeground,
            IBrush parentBackground,
            bool parentUnderline,
            bool parentStrikethough,
            double entireWidth,
            double remainWidth)
        {
            var metries = new List<CGeometry>();

            foreach (CInline inline in Content)
            {
                metries.AddRange(inline.Measure(
                        FontFamily ?? parentFontFamily,
                        FontSize.HasValue ? FontSize.Value : parentFontSize,
                        FontStyle.HasValue ? FontStyle.Value : parentFontStyle,
                        FontWeight.HasValue ? FontWeight.Value : parentFontWeight,
                        Foreground ?? parentForeground,
                        Background ?? parentBackground,
                        IsUnderline || parentUnderline,
                        IsStrikethrough || parentStrikethough,
                        entireWidth,
                        remainWidth));

                CGeometry last = metries[metries.Count - 1];

                remainWidth = last.LineBreak ?
                    entireWidth : entireWidth - last.Width;
            }

            return metries;
        }
    }


}
