using Avalonia;
using Avalonia.Media;
using ColorTextBlock.Avalonia.Geometries;
using System.Collections.Generic;
using System.Globalization;

namespace ColorTextBlock.Avalonia
{
    /// <summary>
    /// Expression of the linebreak.
    /// </summary>
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
            var ftxt = new FormattedText(
                        "Ty",
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface(FontFamily, FontStyle, FontWeight),
                        FontSize,
                        Foreground);

            yield return new LineBreakMarkGeometry(this, ftxt.Height);
        }
    }
}
