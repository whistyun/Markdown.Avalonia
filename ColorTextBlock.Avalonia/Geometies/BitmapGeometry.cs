using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColorTextBlock.Avalonia.Geometries
{
    public class BitmapGeometry : CGeometry
    {
        public Bitmap Bitmap { get; }

        public BitmapGeometry(Bitmap bitmap) : base(bitmap.Size.Width, bitmap.Size.Height, false)
        {
            this.Bitmap = bitmap;
        }

        public override void Render(DrawingContext ctx)
        {
            ctx.DrawImage(
                Bitmap, 1,
                new Rect(Bitmap.Size),
                new Rect(Left, Top, Bitmap.Size.Width, Bitmap.Size.Height)
                );
        }
    }
}
