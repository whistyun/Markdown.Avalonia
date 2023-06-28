using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Media;
using Avalonia.Svg;
using ShimSkiaSharp;

namespace Markdown.Avalonia.Svg
{
    /// <summary>
    /// An <see cref="IImage"/> that uses a <see cref="SvgSource"/> for content.
    /// </summary>
    internal class VectorImage : IImage
    {
        /// <summary>
        /// Gets or sets the <see cref="SvgSource"/> content.
        /// </summary>
        public SvgSource? Source { get; set; }

        /// <inheritdoc/>
        public Size Size =>
            Source?.Picture is { } ? new Size(Source.Picture.CullRect.Width, Source.Picture.CullRect.Height) : default;

        private SKPicture? _previousPicture = null;
        private AvaloniaPicture? _avaloniaPicture = null;

        /// <inheritdoc/>
        void IImage.Draw(
            DrawingContext context,
            Rect sourceRect,
            Rect destRect)
        {
            var source = Source;
            if (source?.Picture is null)
            {
                _previousPicture = null;
                _avaloniaPicture?.Dispose();
                _avaloniaPicture = null;
                return;
            }

            if (Size.Width <= 0 || Size.Height <= 0)
            {
                return;
            }

            var bounds = source.Picture.CullRect;
            var scaleMatrix = Matrix.CreateScale(
                destRect.Width / sourceRect.Width,
                destRect.Height / sourceRect.Height);
            var translateMatrix = Matrix.CreateTranslation(
                -sourceRect.X + destRect.X - bounds.Left,
                -sourceRect.Y + destRect.Y - bounds.Top);
            using (context.PushClip(destRect))
            using (context.PushTransform(translateMatrix))
            using (context.PushTransform(scaleMatrix))
            {
                try
                {
                    if (_avaloniaPicture is null || source.Picture != _previousPicture)
                    {
                        _previousPicture = source.Picture;
                        _avaloniaPicture?.Dispose();
                        _avaloniaPicture = AvaloniaPicture.Record(source.Picture);
                    }

                    if (_avaloniaPicture is { })
                    {
                        _avaloniaPicture.Draw(context);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{ex.Message}");
                    Debug.WriteLine($"{ex.StackTrace}");
                }
            }
        }
    }
}