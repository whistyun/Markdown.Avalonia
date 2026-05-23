using ApprovalTests;
using ApprovalTests.Reporters;
using ColorDocument.Avalonia;
using ColorDocument.Avalonia.DocumentElements;
using ColorTextBlock.Avalonia;
using NUnit.Framework;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnitTest.Base;
using UnitTest.Base.Utils;
using UnitTest.CTxt.Utils;

namespace UnitTest.CTxt
{
    internal class TextSelecting : UnitTestBase
    {
        private const string _shortText1 = "The quick brown fox jumps ";
        private const string _shortText2 = "over the lazy dog";
        private const string _test1Text = _shortText1 + _shortText2;

        private const string _longText1 = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.";
        private const string _longText2 = "Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";
        private const string longTest = _longText1 + _longText2;

        public TextSelecting()
        {
            Approvals.RegisterDefaultApprover((w, n, c) => new ImageFileApprover(w, n, c));
        }

        private static CTextBlock CreateShortTextBlock()
        {
            var ctxt = new CTextBlock();
            ctxt.Content.Add(new CRun() { Text = _shortText1 });
            ctxt.Content.Add(new CRun() { Text = _shortText2 });
            return ctxt;
        }

        public static DocumentRootElement CreateDocument()
        {
            var paras = new List<DocumentElement>() {
                new CTextBlockElement([new CRun(){Text= _longText1 }]),
                new CTextBlockElement([new CRun(){Text= _longText2 }]),
            };

            return new DocumentRootElement(paras);
        }

        [Test]
        [RunOnUI]
        public void CopyTextTestSinle()
        {
            var ctxt = CreateShortTextBlock();

            var info = new MetryHolder(ctxt, 180, 1000);
            info.ToString();

            for (int begin = 0; begin < _test1Text.Length - 1; ++begin)
            {
                for (int end = begin + 1; end <= _test1Text.Length; ++end)
                {
                    ctxt.Select(begin, end);

                    var expected = _test1Text.Substring(begin, end - begin);
                    var actual = ctxt.GetSelectedText();

                    Assert.That(actual, Is.EqualTo(expected), $"Failed for begin={begin} end={end}");
                }
            }
        }


        [Test]
        [RunOnUI]
        public void CopyTextTestDoc()
        {
            var doc = CreateDocument();

            var info = new MetryHolder(doc.Control, 200, 1000);
            info.ToString();
        }

        [Test]
        [RunOnUI]
        public void RenderingTest()
        {
            var ctxt = CreateShortTextBlock();

            var info = new MetryHolder(ctxt, 180, 1000);

            for (int begin = 0; begin < _test1Text.Length - 1; ++begin)
            {
                for (int end = begin + 1; end <= _test1Text.Length; ++end)
                {
                    ctxt.Select(begin, end);
                    info.ReDraw();

                    Approvals.Verify(
                         new ApprovalImageWriter(Path.Combine("Select", begin.ToString("D2")), info.Image, end.ToString("D2")),
                         Approvals.GetDefaultNamer(),
                         new DiffToolReporter(DiffEngine.DiffTool.WinMerge));
                }
            }
        }

    }
}
