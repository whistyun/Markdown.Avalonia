using Avalonia;
using Avalonia.Media.Imaging;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System;
using System.IO;
using System.Threading;
using UnitTest.CTxt.Xamls;
using MdAvBench.Apps;
using Avalonia.Threading;
using MdAvBench.Xamls;

namespace MdAvBench
{
    [SimpleJob]
    public class BenchmarkOfCTextBlock
    {
        private RenderTargetBitmap _bitmapCTB;
        private RenderTargetBitmap _bitmapTB;

        public BenchmarkOfCTextBlock()
        {
            App.Start();

            while (!App.ApplicationStarted)
                Thread.Sleep(100);

            while (App.MainWindow is null)
                Thread.Sleep(100);

            Dispatcher.UIThread.Invoke(() =>
            {
                var dpi = new Vector(96, 96);
                var size = new Size(400, 400);
                _bitmapCTB = new RenderTargetBitmap(PixelSize.FromSizeWithDpi(size, dpi), dpi);
                _bitmapTB = new RenderTargetBitmap(PixelSize.FromSizeWithDpi(size, dpi), dpi);

                App.MainWindow.Width = 400;
            });
        }

        [Benchmark]
        public void DispatcherDelay()
        {
            Dispatcher.UIThread.Invoke(() => { });
        }

        [Benchmark]
        public void RenderByCTextBlock()
        {
            CTextBlockData? control = null;

            Dispatcher.UIThread.Invoke(() =>
            {
                control = new CTextBlockData();
                App.MainWindow.Content = control;
            });

            while (control is null || !control.IsLoaded) ;

            Dispatcher.UIThread.Invoke(() =>
            {
                _bitmapCTB.Render(control);
            });
        }

        [Benchmark]
        public void RenderByTextBlock()
        {
            TextBlockData? control = null;

            Dispatcher.UIThread.Invoke(() =>
            {
                control = new TextBlockData();
                App.MainWindow.Content = control;
            });

            while (control is null || !control.IsLoaded) ;

            Dispatcher.UIThread.Invoke(() =>
            {
                _bitmapTB.Render(control);
            });
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            string path;

            path = $@"D:\debugs\renderingCTB_{DateTime.Now.Ticks}.png";
            if (!File.Exists(path))
                using (var strm = File.OpenWrite(path))
                    _bitmapCTB.Save(strm);

            path = $@"D:\debugs\renderingTB_{DateTime.Now.Ticks}.png";
            if (!File.Exists(path))
                using (var strm = File.OpenWrite(path))
                    _bitmapTB.Save(strm);
        }
    }
}
