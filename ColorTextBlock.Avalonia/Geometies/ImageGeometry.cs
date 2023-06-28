using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace ColorTextBlock.Avalonia.Geometries
{
    public class ImageGeometry : CGeometry
    {
        public new double Width { get; }
        public new double Height { get; }
        public IImage Image { get; }

        internal ImageGeometry(IImage image, double width, double height,
            TextVerticalAlignment alignment) : base(width, height, height, alignment, false)
        {
            this.Image = image;
            this.Width = width;
            this.Height = height;
        }

        public override void Render(DrawingContext ctx)
        {
            ctx.DrawImage(
                Image,
                new Rect(Image.Size),
                new Rect(Left, Top, Width, Height));
        }
    }
}
