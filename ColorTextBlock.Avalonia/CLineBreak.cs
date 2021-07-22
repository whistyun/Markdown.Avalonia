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

            var creator = new LayoutCreateor(
                FontFamily,
                FontStyle,
                FontWeight,
                FontSize);

            yield return new LineBreakMarkGeometry(this, creator.Create("Ty", Foreground));
        }
    }
}
