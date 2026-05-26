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
using UnitTest.CTxt.RenderingXamls;

namespace UnitTest.CTxt
{
    //[UseReporter(typeof(DiffReporter))]
    public class Rendering : UnitTestBase
    {
        public Rendering()
        {
            Approvals.RegisterDefaultApprover((w, n, c) => new ImageFileApprover(w, n, c));
        }

        [Test]
        [RunOnUI]
        public void GivenTest1_generatesExpectedResult()
        {
            var tst1 = new Test1();
            var ctxt = (CTextBlock)tst1.Content!;

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
            var ctxt = (CTextBlock)tst2.Content!;

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
            var spnl = (StackPanel)tst3.Content!;

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
            var spnl = (StackPanel)tst3.Content!;

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
            var spnl = (StackPanel)tst3.Content!;

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
            var spnl = (StackPanel)tst3.Content!;

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
            var ctxt = (CTextBlock)tst4.Content!;

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
            var ctxt = (CTextBlock)tst5.Content!;

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
            var ctxt = (CTextBlock)tst6.Content!;

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
            var ctxt = (StackPanel)tst6.Content!;

            var info = new MetryHolder(ctxt, 480, 1000);

            Approvals.Verify(
                new ApprovalImageWriter(info.Image),
                Approvals.GetDefaultNamer(),
                new DiffToolReporter(DiffEngine.DiffTool.WinMerge));
        }

        [Test]
        [TestCase(500)]
        [TestCase(510)]
        [TestCase(520)]
        [TestCase(530)]
        [TestCase(540)]
        [TestCase(550)]
        [TestCase(560)]
        [TestCase(570)]
        [TestCase(580)]
        [TestCase(590)]
        [TestCase(600)]
        [TestCase(610)]
        [TestCase(620)]
        [TestCase(630)]
        [TestCase(640)]
        [TestCase(650)]
        [TestCase(660)]
        [TestCase(670)]
        [TestCase(680)]
        [TestCase(690)]
        [TestCase(700)]
        [TestCase(710)]
        [TestCase(720)]
        [TestCase(730)]
        [TestCase(740)]
        [TestCase(750)]
        [TestCase(760)]
        [TestCase(770)]
        [TestCase(780)]
        [TestCase(790)]
        [TestCase(800)]
        [TestCase(810)]
        [TestCase(820)]
        [TestCase(830)]
        [TestCase(840)]
        [TestCase(850)]
        [TestCase(860)]
        [TestCase(870)]
        [TestCase(880)]
        [TestCase(890)]
        [TestCase(900)]
        [TestCase(910)]
        [TestCase(920)]
        [TestCase(930)]
        [TestCase(940)]
        [TestCase(950)]
        [TestCase(960)]
        [TestCase(970)]
        [TestCase(980)]
        [TestCase(990)]
        [TestCase(1000)]
        [RunOnUI]
        public void GivenTest99_generatesExpectedResult(int width)
        {
            var tst99 = new Test99();
            var ctxt = (CTextBlock)tst99.Content!;

            var info = new MetryHolder(ctxt, width, 1000);

            Approvals.Verify(
                new ApprovalImageWriter("99", info.Image, width.ToString()),
                Approvals.GetDefaultNamer(),
                new DiffToolReporter(DiffEngine.DiffTool.WinMerge));
        }
    }
}