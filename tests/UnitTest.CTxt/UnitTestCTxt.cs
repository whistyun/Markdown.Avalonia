using ApprovalTests;
using ApprovalTests.Core;
using ApprovalTests.Reporters;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Rendering;
using Avalonia.VisualTree;
using ColorTextBlock.Avalonia;
using Markdown.Avalonia;
using NUnit.Framework;
using System;
using System.Linq;
using System.Reflection;
using UnitTest.Base;
using UnitTest.Base.Apps;
using UnitTest.Base.Utils;
using UnitTest.CTxt.Utils;
using UnitTest.CTxt.Xamls;

namespace UnitTest.CTxt
{
    //[UseReporter(typeof(DiffReporter))]
    public class UnitTestCTxt : UnitTestBase
    {
        public UnitTestCTxt()
        {
            Approvals.RegisterDefaultApprover((w, n, c) => new ImageFileApprover(w, n, c));
        }

        [Test]
        [RunOnUI]
        public void GivenTest1_generatesExpectedResult()
        {
            var tst1 = new Test1();
            var ctxt = (CTextBlock)tst1.Content;

            var info = new MetryHolder(ctxt, 360, 1000);

            Approvals.Verify(
                new ApprovalImageWriter(info.Image),
                Approvals.GetDefaultNamer(),
                new DiffToolReporter(DiffEngine.DiffTool.WinMerge));
        }

        [Test]
        [RunOnUI]
        public void GivenTest2_generatesExpectedResult()
        {
            var tst2 = new Test2();
            var ctxt = (CTextBlock)tst2.Content;

            var info = new MetryHolder(ctxt, 1000, 1000);

            Approvals.Verify(
                new ApprovalImageWriter(info.Image),
                Approvals.GetDefaultNamer(),
                new DiffToolReporter(DiffEngine.DiffTool.WinMerge));
        }

        [Test]
        [RunOnUI]
        public void GivenTest3_generatesExpectedResult_sub0()
        {
            var tst3 = new Test3();
            var spnl = (StackPanel)tst3.Content;

            var ctxt = (CTextBlock)spnl.Children[0];
            var info = new MetryHolder(ctxt, 1000, 1000);

            Approvals.Verify(
                new ApprovalImageWriter(info.Image),
                Approvals.GetDefaultNamer(),
                new DiffToolReporter(DiffEngine.DiffTool.WinMerge));
        }

        [Test]
        [RunOnUI]
        public void GivenTest3_generatesExpectedResult_sub1()
        {
            var tst3 = new Test3();
            var spnl = (StackPanel)tst3.Content;

            var ctxt = (CTextBlock)spnl.Children[1];
            var info = new MetryHolder(ctxt, 1000, 1000);

            Approvals.Verify(
                new ApprovalImageWriter(info.Image),
                Approvals.GetDefaultNamer(),
                new DiffToolReporter(DiffEngine.DiffTool.WinMerge));
        }

        [Test]
        [RunOnUI]
        public void GivenTest3_generatesExpectedResult_sub2()
        {
            var tst3 = new Test3();
            var spnl = (StackPanel)tst3.Content;

            var ctxt = (CTextBlock)spnl.Children[2];
            var info = new MetryHolder(ctxt, 1000, 1000);

            Approvals.Verify(
                new ApprovalImageWriter(info.Image),
                Approvals.GetDefaultNamer(),
                new DiffToolReporter(DiffEngine.DiffTool.WinMerge));
        }

        [Test]
        [RunOnUI]
        public void GivenTest3_generatesExpectedResult_sub3()
        {
            var tst3 = new Test3();
            var spnl = (StackPanel)tst3.Content;

            var ctxt = (CTextBlock)spnl.Children[3];
            var info = new MetryHolder(ctxt, 1000, 1000);

            Approvals.Verify(
                new ApprovalImageWriter(info.Image),
                Approvals.GetDefaultNamer(),
                new DiffToolReporter(DiffEngine.DiffTool.WinMerge));
        }

        [Test]
        [RunOnUI]
        public void GivenTest_drawableSomeMds()
        {
            foreach (var mdname in Util.GetTextNames().Where(nm => nm.EndsWith(".md")))
            {
                var text = Util.LoadText(mdname);
                var markdown = new Markdown.Avalonia.Markdown();
                var control = markdown.Transform(text);

                var theme = new Avalonia.Themes.Simple.SimpleTheme();
                control.Styles.Add(theme);

                control.Styles.Add(MarkdownStyle.SimpleTheme);
                control.Resources.Add("FontSizeNormal", 16d);

                var umefont = new FontFamily(new Uri("avares://UnitTest.CTxt/Assets/Fonts/ume-ugo4.ttf"), "Ume UI Gothic");
                TextElement.SetFontFamily(control, umefont);

                var info = new MetryHolder(control, 500, 10000);
            }
        }

        /*
         * On Github Action, this test don't pass.
         * But on my environment, this test pass.
         * Because of environment dependent, I erase this test case.
         */
        //[Test]
        [RunOnUI]
        public void GivenTestXXX_generatesExpectedResult()
        {
            var text = Util.LoadText("MainWindow.md");

            var markdown = new Markdown.Avalonia.Markdown();
            var control = markdown.Transform(text);

            var theme = new Avalonia.Themes.Simple.SimpleTheme();
            control.Styles.Add(theme);

            control.Styles.Add(MarkdownStyle.SimpleTheme);
            control.Resources.Add("FontSizeNormal", 16d);

            var umefont = new FontFamily(new Uri("avares://UnitTest.CTxt/Assets/Fonts/ume-ugo4.ttf"), "Ume UI Gothic");
            TextElement.SetFontFamily(control, umefont);

            var info = new MetryHolder(control, 500, 10000);

            Approvals.Verify(
                new ApprovalImageWriter(info.Image),
                Approvals.GetDefaultNamer(),
                new DiffToolReporter(DiffEngine.DiffTool.WinMerge));
        }

        [Test]
        [RunOnUI]
        public void GivenTest4_generatesExpectedResult()
        {
            var tst4 = new Test4();
            var ctxt = (CTextBlock)tst4.Content;

            var info = new MetryHolder(ctxt, 1000, 1000);

            Approvals.Verify(
                new ApprovalImageWriter(info.Image),
                Approvals.GetDefaultNamer(),
                new DiffToolReporter(DiffEngine.DiffTool.WinMerge));
        }

        [Test]
        [RunOnUI]
        public void GivenTest5_generatesExpectedResult()
        {
            var tst5 = new Test5();
            var ctxt = (CTextBlock)tst5.Content;

            var info = new MetryHolder(ctxt, 1000, 1000);

            Approvals.Verify(
                new ApprovalImageWriter(info.Image),
                Approvals.GetDefaultNamer(),
                new DiffToolReporter(DiffEngine.DiffTool.WinMerge));
        }

        [Test]
        [RunOnUI]
        public void GivenTest6_generatesExpectedResult()
        {
            var tst6 = new Test6();
            var ctxt = (CTextBlock)tst6.Content;

            var info = new MetryHolder(ctxt, 1000, 1000);

            Approvals.Verify(
                new ApprovalImageWriter(info.Image),
                Approvals.GetDefaultNamer(),
                new DiffToolReporter(DiffEngine.DiffTool.WinMerge));
        }

        [Test]
        [RunOnUI]
        public void GivenTest7_generatesExpectedResult()
        {
            var tst6 = new Test7();
            var ctxt = (StackPanel)tst6.Content;

            var info = new MetryHolder(ctxt, 480, 1000);

            Approvals.Verify(
                new ApprovalImageWriter(info.Image),
                Approvals.GetDefaultNamer(),
                new DiffToolReporter(DiffEngine.DiffTool.WinMerge));
        }
    }

    class MetryHolder : AvaloniaObject
    {
        private static readonly Vector Dpi = new(250, 250);

        public Bitmap Image { get; set; }

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
            ctxt.Arrange(new Rect(0, 0, width, ctxt.DesiredSize.Height == 0 ? height : ctxt.DesiredSize.Height));
            ctxt.Measure(reqSz);

            var newReqSz = new Size(
                ctxt.DesiredSize.Width == 0 ? reqSz.Width : ctxt.DesiredSize.Width,
                ctxt.DesiredSize.Height == 0 ? reqSz.Height : ctxt.DesiredSize.Height);
            ctxt.Arrange(new Rect(0, 0, newReqSz.Width, newReqSz.Height));

            var bitmap = new RenderTargetBitmap(PixelSize.FromSizeWithDpi(newReqSz, Dpi), Dpi);

            using (var context = bitmap.CreateDrawingContext())
            {
                RenderHelper(ctxt, context);
            }
            Image = bitmap;
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

                var children = (IAvaloniaList<Visual>)childrenProp.GetValue(vis);
                foreach (var child in children)
                    RenderHelper(child, ctx);
            }
        }
    }
}