using Avalonia;
using Avalonia.Media;
using ColorTextBlock.Avalonia.Geometries;
using System;
using System.Collections.Generic;
using System.Text;

namespace ColorTextBlock.Avalonia
{
    public class CLineBreak : CRun
    {
        public CLineBreak()
        {
            Text = "\n";
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
            var family = FontFamily ?? parentFontFamily;
            var size = FontSize.HasValue ? FontSize.Value : parentFontSize;
            var style = FontStyle.HasValue ? FontStyle.Value : parentFontStyle;
            var weight = FontWeight.HasValue ? FontWeight.Value : parentFontWeight;
            var foreground = Foreground ?? parentForeground;
            var background = Background ?? parentBackground;
            var underline = IsUnderline || parentUnderline;
            var strikethrough = IsStrikethrough || parentStrikethough;

            var fmt = Measure(Size.Infinity, family, size, style, weight, TextWrapping.Wrap);
            fmt.Text = "Ty";

            yield return new TextGeometry(
                0, fmt.Bounds.Height, true,
                foreground, background,
                underline, strikethrough,
                "", fmt);
        }
    }
}
