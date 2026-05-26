using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.Linq;
using System.Reflection;

namespace UnitTest.CTxt
{
    class MetryHolder : AvaloniaObject
    {
        private static readonly Vector Dpi = new(250, 250);

        private readonly RenderTargetBitmap _bitmap;
        private readonly Control _ctxt;

        public Bitmap Image => _bitmap;

        //public MetryHolder(CTextBlock ctxt, int width = 400, int height = 1000)
        //{
        //    var reqSz = new Size(width, height);
        //
        //    ctxt.Measure(reqSz);
        //    ctxt.Arrange(new Rect(0, 0, width, ctxt.DesiredSize.Height == 0 ? height : ctxt.DesiredSize.Height));
        //    ctxt.Measure(reqSz);
        //
        //    var newReqSz = new Size(
        //        ctxt.DesiredSize.Width == 0 ? reqSz.Width : ctxt.DesiredSize.Width,
        //        ctxt.DesiredSize.Height == 0 ? reqSz.Height : ctxt.DesiredSize.Height);
        //    ctxt.Arrange(new Rect(0, 0, newReqSz.Width, newReqSz.Height));
        //
        //    var bitmap = new RenderTargetBitmap(PixelSize.FromSizeWithDpi(newReqSz, Dpi), Dpi);
        //
        //    using (var icontext = bitmap.CreateDrawingContext(null))
        //    using (var context = new DrawingContext(icontext))
        //    {
        //        ctxt.Render(context);
        //    }
        //
        //    Image = bitmap;
        //}

        public MetryHolder(Control ctxt, int width = 400, int height = 1000)
        {
            var reqSz = new Size(width, height);
            ctxt.Measure(reqSz);

            var dSize = ctxt.DesiredSize;
            ctxt.Arrange(new Rect(0, 0, width, dSize.Height == 0 ? height : dSize.Height));
            ctxt.Measure(reqSz);

            dSize = ctxt.DesiredSize;
            var newReqSz = new Size(
                dSize.Width == 0 ? reqSz.Width : dSize.Width,
                dSize.Height == 0 ? reqSz.Height : dSize.Height);

            ctxt.Arrange(new Rect(0, 0, newReqSz.Width, newReqSz.Height));

            _bitmap = new RenderTargetBitmap(PixelSize.FromSizeWithDpi(newReqSz, Dpi), Dpi);
            _ctxt = ctxt;

            ReDraw();
        }

        private void RenderHelper(Visual vis, DrawingContext ctx)
        {
            var sz = new Rect(vis.Bounds.Size);
            var bnd = vis.Bounds;

            using (ctx.PushTransform(Matrix.CreateTranslation(vis.Bounds.Position)))
            //using (ctx.PushOpacity(vis.Opacity))
            using (vis.OpacityMask != null ? ctx.PushOpacityMask(vis.OpacityMask, sz) : default)
            {
                vis.Render(ctx);

                var childrenProp = typeof(Visual).GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
                                            .Where(fld => fld.Name == "VisualChildren")
                                            .First();

                var visualChildrenObj = childrenProp.GetValue(vis)
                                        ?? throw new NullReferenceException("Failed to get a value: VisualChildren");

                var children = (IAvaloniaList<Visual>)visualChildrenObj;
                foreach (var child in children)
                    RenderHelper(child, ctx);
            }
        }

        public void ReDraw()
        {
            using (var context = _bitmap.CreateDrawingContext())
            {
                RenderHelper(_ctxt, context);
            }
        }
    }
}
