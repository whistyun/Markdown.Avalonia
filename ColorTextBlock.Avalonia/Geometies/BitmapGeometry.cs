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
    class BitmapGeometry : CGeometry
    {
        public new double Width { get; }
        public new double Height { get; }
        public Bitmap Bitmap { get; }

        public BitmapGeometry(Bitmap bitmap) : this(bitmap, bitmap.Size.Width, bitmap.Size.Height)
        {
            this.Bitmap = bitmap;
            this.Width = bitmap.Size.Width;
            this.Height = bitmap.Size.Height;
        }

        public BitmapGeometry(Bitmap bitmap, double width, double height) : base(width, height, false)
        {
            this.Bitmap = bitmap;
            this.Width = width;
            this.Height = height;
        }

        public override void Render(DrawingContext ctx)
        {
            ctx.DrawImage(
                Bitmap,
                new Rect(Bitmap.Size),
                new Rect(Left, Top, Width, Height)
                );
        }
    }
}
