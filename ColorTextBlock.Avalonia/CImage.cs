using Avalonia.Media;
using Avalonia.Media.Imaging;
using ColorTextBlock.Avalonia.Geometries;
using System;
using System.Collections.Generic;
using System.Text;

namespace ColorTextBlock.Avalonia
{
    public class CImage : CInline
    {

        public Bitmap Image { get; }

        public CImage(Bitmap bitmap)
        {
            this.Image = Image;
        }

        protected internal override IEnumerable<CGeometry> Measure(
            FontFamily parentFontFamily, double parentFontSize, FontStyle parentFontStyle, FontWeight parentFontWeight,
            IBrush parentForeground, IBrush parentBackground,
            bool parentUnderline, bool parentStrikethough,
            double entireWidth, double remainWidth)
        {
            yield return new BitmapGeometry(Image);
        }
    }
}
