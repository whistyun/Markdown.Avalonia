using ApprovalTests;
using ApprovalTests.Core;
using ApprovalTests.Reporters;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ColorTextBlock.Avalonia;
using NUnit.Framework;
using UnitTest.Base;
using UnitTest.Base.Utils;
using UnitTest.CTxt.Utils;
using UnitTest.CTxt.Xamls;

namespace UnitTest.CTxt
{
    [UseReporter(typeof(DiffReporter))]
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

            var info = new MetryHolder(ctxt, 390, 1000);

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
    }

    class MetryHolder : AvaloniaObject
    {
        private static readonly Vector Dpi = new Vector(250, 250);

        public Bitmap Image { get; set; }

        public MetryHolder(CTextBlock ctxt, int width = 400, int height = 1000)
        {
            var reqSz = new Size(width, height);

            ctxt.Measure(reqSz);
            ctxt.Arrange(new Rect(0, 0, width, ctxt.DesiredSize.Height));
            ctxt.Measure(reqSz);

            var bitmap = new RenderTargetBitmap(PixelSize.FromSizeWithDpi(ctxt.DesiredSize, Dpi), Dpi);

            using (var icontext = bitmap.CreateDrawingContext(null))
            using (var context = new DrawingContext(icontext))
            {
                ctxt.Render(context);
            }

            Image = bitmap;
        }
    }
}