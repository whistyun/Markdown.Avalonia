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

        public static readonly StyledProperty<double?> RelativeWidthProperty =
            AvaloniaProperty.Register<CImage, double?>(nameof(RelativeWidth));

        /// <summary>
        /// Determine wheither image auto fitting or protrude outside Control
        /// when image is too width to be rendered in control.
        /// If you set 'true', Image is fitted to control width.
        /// </summary>
        public static readonly StyledProperty<bool> FittingWhenProtrudeProperty =
            AvaloniaProperty.Register<CImage, bool>(nameof(FittingWhenProtrude), defaultValue: true);

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

        public double? RelativeWidth
        {
            get { return GetValue(RelativeWidthProperty); }
            set { SetValue(RelativeWidthProperty, value); }
        }

        public bool FittingWhenProtrude
        {
            get { return GetValue(FittingWhenProtrudeProperty); }
            set { SetValue(FittingWhenProtrudeProperty, value); }
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

            double imageWidth = Image.Size.Width;
            double imageHeight = Image.Size.Height;

            if (RelativeWidth.HasValue)
            {
                var aspect = imageHeight / imageWidth;
                imageWidth = RelativeWidth.Value * entireWidth;
                imageHeight = aspect * imageWidth;
            }

            if (LayoutWidth.HasValue)
            {
                imageWidth = LayoutWidth.Value;
            }

            if (LayoutHeight.HasValue)
            {
                imageHeight = LayoutHeight.Value;
            }

            if (imageWidth > remainWidth)
            {
                if (entireWidth != remainWidth)
                {
                    yield return TextGeometry.NewLine();
                }

                if (FittingWhenProtrude && imageWidth > entireWidth)
                {
                    var aspect = imageHeight / imageWidth;
                    imageWidth = entireWidth;
                    imageHeight = aspect * imageWidth;
                }
            }

            yield return new BitmapGeometry(Image, imageWidth, imageHeight);
        }
    }
}
