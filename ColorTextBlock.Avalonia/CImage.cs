using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ColorTextBlock.Avalonia.Geometries;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColorTextBlock.Avalonia
{
    public class CImage : CInline
    {
        public static readonly StyledProperty<double?> LayoutWidthProperty =
            AvaloniaProperty.Register<CImage, double?>(nameof(LayoutWidth));

        public static readonly StyledProperty<double?> LayoutHeightProperty =
            AvaloniaProperty.Register<CImage, double?>(nameof(LayoutHeight));

        public double? LayoutWidth
        {
            get { return GetValue(LayoutWidthProperty); }
            set { SetValue(LayoutWidthProperty, value); }
        }
        public double? LayoutHeight
        {
            get { return GetValue(LayoutHeightProperty); }
            set { SetValue(LayoutHeightProperty, value); }
        }

        public Task<Bitmap> Task { get; }
        private Bitmap WhenError { get; }
        public Bitmap Image { private set; get; }

        public CImage(Task<Bitmap> task, Bitmap whenError)
        {
            if (task is null) throw new NullReferenceException(nameof(task));
            if (whenError is null) throw new NullReferenceException(nameof(whenError));

            this.Task = task;
            this.WhenError = whenError;
        }

        public CImage(Bitmap bitmap)
        {
            if (bitmap is null) throw new NullReferenceException(nameof(bitmap));

            this.Image = bitmap;
        }

        protected internal override IEnumerable<CGeometry> Measure(
            double entireWidth, double remainWidth)
        {
            if (Image is null)
            {
                Task.Wait();
                Image = Task.IsFaulted ? WhenError : Task.Result ?? WhenError;
            }

            yield return new BitmapGeometry(
                Image,
                LayoutWidth.HasValue ? LayoutWidth.Value : Image.Size.Width,
                LayoutHeight.HasValue ? LayoutHeight.Value : Image.Size.Height);
        }
    }
}
