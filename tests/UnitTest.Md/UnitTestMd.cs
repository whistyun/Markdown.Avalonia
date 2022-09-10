using ApprovalTests;
using ApprovalTests.Reporters;
using Avalonia.Controls;
using NUnit.Framework;
using System.Linq;
using UnitTest.Base;
using UnitTest.Base.Utils;

namespace UnitTest.Md
{
    [UseReporter(typeof(DiffReporter))]
    public class UnitTestMd : UnitTestBase
    {
        [Test]
        [RunOnUI]
        public void Transform_givenTest1_generatesExpectedResult()
        {
            var text = Util.LoadText("Test1.md");
            var markdown = new Markdown.Avalonia.Markdown();
            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenLists1_generatesExpectedResult()
        {
            var text = Util.LoadText("Lists1.md");
            var markdown = new Markdown.Avalonia.Markdown();
            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenLists2_generatesExpectedResult()
        {
            var text = Util.LoadText("Lists2.md");
            var markdown = new Markdown.Avalonia.Markdown();
            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenLists3_generatesExpectedResult()
        {
            var text = Util.LoadText("Lists3.md");
            var markdown = new Markdown.Avalonia.Markdown();
            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenTables1_generatesExpectedResult()
        {
            var text = Util.LoadText("Tables.md");
            var markdown = new Markdown.Avalonia.Markdown();
            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenHorizontalRules_generatesExpectedResult()
        {
            var text = Util.LoadText("HorizontalRules.md");
            var markdown = new Markdown.Avalonia.Markdown();
            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenLinksInline_generatesExpectedResult()
        {
            var text = Util.LoadText("Links_inline_style.md");
            var markdown = new Markdown.Avalonia.Markdown();
            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenTextStyles_generatesExpectedResult()
        {
            var text = Util.LoadText("Text_style.md");
            var markdown = new Markdown.Avalonia.Markdown();
            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenImages_generatesExpectedResult()
        {
            var text = Util.LoadText("Images.md");
            var markdown = new Markdown.Avalonia.Markdown() { AssetPathRoot = AssetPath };

            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenBlockqoute_generatesExpectedResult()
        {
            var text = Util.LoadText("Blockquite.md");
            var markdown = new Markdown.Avalonia.Markdown();

            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenMixing_generatesExpectedResult()
        {
            var text = Util.LoadText("Mixing.md");
            var markdown = new Markdown.Avalonia.Markdown() { AssetPathRoot = AssetPath };

            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenMixing2_generatesExpectedResult()
        {
            var text = Util.LoadText("Mixing2.md");
            var markdown = new Markdown.Avalonia.Markdown() { AssetPathRoot = AssetPath };

            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenCodes_generatesExpectedResult()
        {
            var text = Util.LoadText("Codes.md");
            var markdown = new Markdown.Avalonia.Markdown() { AssetPathRoot = AssetPath };

            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenContainer_generatesExpectedResult()
        {
            var text = Util.LoadText("ContainerBlock.md");
            var markdown = new Markdown.Avalonia.Markdown() { AssetPathRoot = AssetPath };

            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenEmoji()
        {
            var text = Util.LoadText("Emoji.md");
            var markdown = new Markdown.Avalonia.Markdown() { AssetPathRoot = AssetPath };

            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void CheckSwitch()
        {
            var markdown = new Markdown.Avalonia.Markdown
            {
                ContainerBlockHandler = new Markdown.Avalonia.ContainerSwitch() {
                    { "test", new EmptyBorder("TestBorder1")},
                    { "test2", new EmptyBorder("TestBorder2")},
                }
            };

            {
                var control1_1 = markdown.Transform(TextUtil.HereDoc(@"
                    ::: test{}
                    some text
                    :::
                "));
                Assert.AreEqual(1, Util.FindControlsByClassName<Border>(control1_1, "TestBorder1").Count());
            }

            {
                var control1_2 = markdown.Transform(TextUtil.HereDoc(@"
                    ::: test             []
                    some text
                    :::
                "));
                Assert.AreEqual(1, Util.FindControlsByClassName<Border>(control1_2, "TestBorder1").Count());
            }

            {
                var control2 = markdown.Transform(TextUtil.HereDoc(@"
                    :::    test2    ()
                    some text
                    :::
                "));
                Assert.AreEqual(1, Util.FindControlsByClassName<Border>(control2, "TestBorder2").Count());
            }

        }


        class EmptyBorder : Markdown.Avalonia.Utils.IContainerBlockHandler
        {
            public string ClassName { private set; get; }

            public EmptyBorder(string classNm)
            {
                ClassName = classNm;
            }

            public Avalonia.Controls.Border ProvideControl(string assetPathRoot, string blockName, string lines)
            {
                var border = new Avalonia.Controls.Border();
                border.Classes.Add(ClassName);

                return border;
            }
        }
    }

}