using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System.Diagnostics.CodeAnalysis;

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

        public override bool TryMoveNext(
            TextPointer current,
#if NETCOREAPP3_0_OR_GREATER
            [MaybeNullWhen(false)]
            out TextPointer? next
#else
            out TextPointer next
#endif
            )
        {
            if (current == GetBegin())
            {
                next = GetEnd();
                return true;
            }
            else
            {
                next = null;
                return false;
            }
        }

        public override bool TryMovePrev(
            TextPointer current,
#if NETCOREAPP3_0_OR_GREATER
            [MaybeNullWhen(false)]
            out TextPointer? prev
#else
            out TextPointer prev
#endif
            )
        {
            if (current == GetEnd())
            {
                prev = GetBegin();
                return true;
            }
            else
            {
                prev = null;
                return false;
            }
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

        public override TextPointer GetBegin()
        {
            return new TextPointer(this, 0, Left, Top, Height);
        }

        public override TextPointer GetEnd()
        {
            return new TextPointer(this, 1, Left, Top, Height);
        }
    }
}
