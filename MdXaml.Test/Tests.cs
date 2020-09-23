using ApprovalTests;
using ApprovalTests.Reporters;
using MdXamlTest;
using NUnit.Framework;
using System;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Markup;


#if !MIG_FREE
namespace MdXaml.Test
#else
namespace Markdown.Xaml.Test
#endif
{
    [UseReporter(typeof(DiffReporter))]
    public class Tests
    {
        static Tests()
        {
#if !MIG_FREE
            Approvals.RegisterDefaultNamerCreation(() => new ChangeOutputPathNamer("Out"));
#else
            Approvals.RegisterDefaultNamerCreation(() => new ChangeOutputPathNamer("OutMF"));
#endif
        }

        string assetPath;
        Uri baseUri;


        public Tests()
        {
            PackUriHelper.Create(new Uri("http://example.com"));

            var asm = Assembly.GetExecutingAssembly();
            assetPath = Path.GetDirectoryName(asm.Location);
            baseUri = new Uri($"pack://application:,,,/{asm.GetName().Name};Component/");
        }



        [Test]
        [Apartment(ApartmentState.STA)]
        public void Transform_givenTest1_generatesExpectedResult()
        {
            var text = Utils.LoadText("Test1.md");
            var markdown = new Markdown();
            var result = markdown.Transform(text);
            Approvals.Verify(Utils.AsXaml(result));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void Transform_givenTest2_generatesExpectedResult()
        {
            var text = Utils.LoadText("Test1.md");
            var markdown = new Markdown();
            markdown.DisabledTag = true;
            var result = markdown.Transform(text);
            Approvals.Verify(Utils.AsXaml(result));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void Transform_givenLists1_generatesExpectedResult()
        {
            var text = Utils.LoadText("Lists1.md");
            var markdown = new Markdown();
            var result = markdown.Transform(text);
            Approvals.Verify(Utils.AsXaml(result));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void Transform_givenLists2_generatesExpectedResult()
        {
            var text = Utils.LoadText("Lists2.md");
            var markdown = new Markdown();
            var result = markdown.Transform(text);
            Approvals.Verify(Utils.AsXaml(result));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void Transform_givenTables1_generatesExpectedResult()
        {
            var text = Utils.LoadText("Tables.md");
            var markdown = new Markdown();
            var result = markdown.Transform(text);
            Approvals.Verify(Utils.AsXaml(result));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void Transform_givenTables2_generatesExpectedResult()
        {
            var text = Utils.LoadText("Tables.md");
            var markdown = new Markdown();
            markdown.DisabledTag = true;
            var result = markdown.Transform(text);
            Approvals.Verify(Utils.AsXaml(result));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void Transform_givenHorizontalRules_generatesExpectedResult()
        {
            var text = Utils.LoadText("HorizontalRules.md");
            var markdown = new Markdown();
            var result = markdown.Transform(text);
            Approvals.Verify(Utils.AsXaml(result));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void Transform_givenLinksInline1_generatesExpectedResult()
        {
            var text = Utils.LoadText("Links_inline_style.md");
            var markdown = new Markdown();
            var result = markdown.Transform(text);
            Approvals.Verify(Utils.AsXaml(result));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void Transform_givenLinksInline2_generatesExpectedResult()
        {
            var text = Utils.LoadText("Links_inline_style.md");
            var markdown = new Markdown();
            markdown.DisabledTootip = true;
            var result = markdown.Transform(text);
            Approvals.Verify(Utils.AsXaml(result));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void Transform_givenTextStyles_generatesExpectedResult()
        {
            var text = Utils.LoadText("Text_style.md");
            var markdown = new Markdown();
            var result = markdown.Transform(text);
            Approvals.Verify(Utils.AsXaml(result));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void Transform_givenImages1_generatesExpectedResult()
        {
            var text = Utils.LoadText("Images.md");
            var markdown = new Markdown();
            markdown.AssetPathRoot = assetPath;
            markdown.BaseUri = baseUri;

            var result = markdown.Transform(text);
            Approvals.Verify(Utils.AsXaml(result));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void Transform_givenImages2_generatesExpectedResult()
        {
            var text = Utils.LoadText("Images.md");
            var markdown = new Markdown();
            markdown.DisabledLazyLoad = true;
            markdown.AssetPathRoot = assetPath;
            markdown.BaseUri = baseUri;

            var result = markdown.Transform(text);
            Approvals.Verify(Utils.AsXaml(result));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void Transform_givenImages3_generatesExpectedResult()
        {
            var text = Utils.LoadText("Images.md");
            var markdown = new Markdown();
            markdown.DisabledTootip = true;
            markdown.AssetPathRoot = assetPath;
            markdown.BaseUri = baseUri;

            var result = markdown.Transform(text);
            Approvals.Verify(Utils.AsXaml(result));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void Transform_givenBlockqoute_generatesExpectedResult()
        {
            var text = Utils.LoadText("Blockquite.md");
            var markdown = new Markdown();

            var result = markdown.Transform(text);
            Approvals.Verify(Utils.AsXaml(result));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void Transform_givenMixing_generatesExpectedResult()
        {
            var text = Utils.LoadText("Mixing.md");
            var markdown = new Markdown();
            markdown.AssetPathRoot = assetPath;
            markdown.BaseUri = baseUri;

            var result = markdown.Transform(text);
            Approvals.Verify(Utils.AsXaml(result));
        }


        [Test]
        [Apartment(ApartmentState.STA)]
        public void Transform_givenstring()
        {
            ResourceDictionary resources;
            using (var stream = new FileStream("IndentTest.xaml", FileMode.Open))
            {
                resources = (ResourceDictionary)XamlReader.Load(stream);
            }

            var markdownViewer = new MarkdownScrollViewer();
            markdownViewer.MarkdownStyle = null;

            foreach (var idx in Enumerable.Range(1, 4))
            {
                var jaggingMarkdown = (string)resources["Indent" + idx];
                markdownViewer.HereMarkdown = jaggingMarkdown;
                var document = markdownViewer.Document;
                Approvals.Verify(Utils.AsXaml(document));
            }
        }
    }
}