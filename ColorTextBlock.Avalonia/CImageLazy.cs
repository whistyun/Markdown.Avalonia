using Avalonia.Media;
using Avalonia.Media.Imaging;
using ColorTextBlock.Avalonia.Geometries;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColorTextBlock.Avalonia
{
    public class CImageLazy : CInline
    {
        public Bitmap Result { get; private set; }
        public Task<Bitmap> Task { get; }
        private Bitmap WhenError { get; }

        public CImageLazy(Task<Bitmap> task, Bitmap whenError)
        {
            this.Task = task;
            this.WhenError = whenError;
        }

        protected internal override IEnumerable<CGeometry> Measure(
            FontFamily parentFontFamily, double parentFontSize, FontStyle parentFontStyle, FontWeight parentFontWeight,
            IBrush parentForeground, IBrush parentBackground,
            bool parentUnderline, bool parentStrikethough,
            double entireWidth, double remainWidth)
        {
            if (Result is null)
            {
                Task.Wait();
                Result = Task.IsFaulted ? WhenError : Task.Result ?? WhenError;
            }

            yield return new BitmapGeometry(Result);
        }
    }
}
