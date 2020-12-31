using Avalonia;
using Avalonia.Media;
using ColorTextBlock.Avalonia.Geometries;
using System.Collections.Generic;

namespace ColorTextBlock.Avalonia
{
    public class CLineBreak : CRun
    {
        public CLineBreak()
        {
            Text = "\n";
        }

        protected override IEnumerable<CGeometry> MeasureOverride(
            double entireWidth,
            double remainWidth)
        {
            var family = FontFamily;
            var size = FontSize;
            var style = FontStyle;
            var weight = FontWeight;
            var foreground = Foreground;
            var background = Background;
            var underline = IsUnderline;
            var strikethrough = IsStrikethrough;

            FormattedText fmt = Measure(Size.Infinity, family, size, style, weight, TextWrapping.Wrap);
            fmt.Text = "Ty";

            yield return new TextGeometry(
                0, fmt.Bounds.Height, true,
                this,
                "", null);
        }
    }
}
