using ApprovalTests;
using ApprovalTests.Reporters;
using Markdown.Avalonia;
using NUnit.Framework;
using UnitTest.Base;
using UnitTest.Base.Utils;

namespace UnitTest.Md
{
    [UseReporter(typeof(DiffReporter))]
    public class UnitTestFlexMd : UnitTestBase
    {
        private FlexibleMarkdown MkMarkdown()
        {
            var md = new FlexibleMarkdown
            {
                EnableNoteBlock = true,
                EnableTableExtension = true,
                EnableTextDecorationExtension = true,
                EnableListMarkerExtension = true,
                EnableParagraphAlignment = true,
                EnableSaveSpaces = true,
                EnableSaveLineBreak = true
            };
            return md;
        }

        [Test]
        [RunOnUI]
        public void Transform_givenTest1_generatesExpectedResult()
        {
            var text = Util.LoadText("Test1.md");
            var markdown = MkMarkdown();
            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenLists1_generatesExpectedResult()
        {
            var text = Util.LoadText("Lists1.md");
            var markdown = MkMarkdown();
            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenLists2_generatesExpectedResult()
        {
            var text = Util.LoadText("Lists2.md");
            var markdown = MkMarkdown();
            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenLists3_generatesExpectedResult()
        {
            var text = Util.LoadText("Lists3.md");
            var markdown = MkMarkdown();
            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenTables1_generatesExpectedResult()
        {
            var text = Util.LoadText("Tables.md");
            var markdown = MkMarkdown();
            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenHorizontalRules_generatesExpectedResult()
        {
            var text = Util.LoadText("HorizontalRules.md");
            var markdown = MkMarkdown();
            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenLinksInline_generatesExpectedResult()
        {
            var text = Util.LoadText("Links_inline_style.md");
            var markdown = MkMarkdown();
            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenTextStyles_generatesExpectedResult()
        {
            var text = Util.LoadText("Text_style.md");
            var markdown = MkMarkdown();
            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenImages_generatesExpectedResult()
        {
            var text = Util.LoadText("Images.md");
            var markdown = MkMarkdown();
            markdown.AssetPathRoot = AssetPath;

            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenBlockqoute_generatesExpectedResult()
        {
            var text = Util.LoadText("Blockquite.md");
            var markdown = MkMarkdown();

            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenMixing_generatesExpectedResult()
        {
            var text = Util.LoadText("Mixing.md");
            var markdown = MkMarkdown();
            markdown.AssetPathRoot = AssetPath;

            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenEmoji()
        {
            var text = Util.LoadText("Emoji.md");
            var markdown = MkMarkdown();
            markdown.AssetPathRoot = AssetPath;

            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }
    }

}