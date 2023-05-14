using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace ColorTextBlock.Avalonia.Geometries
{
    public class BitmapGeometry : CGeometry
    {
        public new double Width { get; }
        public new double Height { get; }
        public Bitmap Bitmap { get; }

        internal BitmapGeometry(Bitmap bitmap, double width, double height,
            TextVerticalAlignment alignment) : base(width, height, height, alignment, false)
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
                new Rect(Left, Top, Width, Height));
        }
    }
}
