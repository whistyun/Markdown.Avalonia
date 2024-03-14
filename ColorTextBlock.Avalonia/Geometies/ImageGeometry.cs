using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.Diagnostics.CodeAnalysis;

namespace ColorTextBlock.Avalonia.Geometries
{
    public class ImageGeometry : CGeometry
    {
        public new double Width { get; }
        public new double Height { get; }
        public IImage Image { get; }

        internal ImageGeometry(
            CImage owner,
            IImage image, double width, double height,
            TextVerticalAlignment alignment) :
            base(owner, width, height, height, alignment, false)
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

        public override TextPointer CalcuatePointerFrom(double x, double y)
        {
            if (x < Left + Width / 2)
            {
                return GetBegin();
            }
            else
            {
                return GetEnd();
            }
        }
        public override TextPointer CalcuatePointerFrom(int index)
        {
            return index switch
            {
                0 => GetBegin(),
                1 => GetEnd(),
                _ => throw new ArgumentOutOfRangeException(nameof(index))
            };
        }
        public override TextPointer GetBegin()
        {
            return new TextPointer(this);
        }

        public override TextPointer GetEnd()
        {
            return new TextPointer(this, 1, Width);
        }
    }
}
