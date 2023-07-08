using ApprovalTests;
using ApprovalTests.Reporters;
using Avalonia.Controls;
using Markdown.Avalonia;
using Markdown.Avalonia.Html;
using NUnit.Framework;
using System.Linq;
using UnitTest.Base;
using UnitTest.Base.Utils;
using UnitTest.MdHtml.Test;

using MarkdownEngine = Markdown.Avalonia.Markdown;

namespace UnitTest.MdHtml
{
    [UseReporter(typeof(DiffReporter))]
    public class UnitTest : UnitTestBase
    {
        public MarkdownEngine Engine
        {
            get
            {
                var plugins = new MdAvPlugins();
                plugins.Plugins.Add(new HtmlPlugin());

                return new MarkdownEngine()
                {
                    Plugins = plugins
                };
            }
        }

        [Test]
        [RunOnUI]
        public void Button()
        {
            var html = Utils.ReadHtml();

            var doc = Engine.Transform(html);

            var xaml = Utils.AsXaml(doc);

            Approvals.Verify(xaml);
        }

        [Test]
        [RunOnUI]
        public void CodeBlock()
        {
            var html = Utils.ReadHtml();

            var doc = Engine.Transform(html);

            var xaml = Utils.AsXaml(doc);

            Approvals.Verify(xaml);
        }

        [Test]
        [RunOnUI]
        public void InlineCode()
        {
            var html = Utils.ReadHtml();

            var doc = Engine.Transform(html);

            var xaml = Utils.AsXaml(doc);

            Approvals.Verify(xaml);
        }

        [Test]
        [RunOnUI]
        public void Input()
        {
            var html = Utils.ReadHtml();

            var doc = Engine.Transform(html);

            var xaml = Utils.AsXaml(doc);

            Approvals.Verify(xaml);
        }

        [Test]
        [RunOnUI]
        public void List()
        {
            var html = Utils.ReadHtml();

            var doc = Engine.Transform(html);

            var xaml = Utils.AsXaml(doc);

            Approvals.Verify(xaml);
        }

        [Test]
        [RunOnUI]
        public void Progres()
        {
            var html = Utils.ReadHtml();

            var doc = Engine.Transform(html);

            var xaml = Utils.AsXaml(doc);

            Approvals.Verify(xaml);
        }

        [Test]
        [RunOnUI]
        public void Details()
        {
            var html = Utils.ReadHtml();

            var doc = Engine.Transform(html);

            var xaml = Utils.AsXaml(doc);

            Approvals.Verify(xaml);
        }

        [Test]
        [RunOnUI]
        public void TypicalBlock()
        {
            var html = Utils.ReadHtml();

            var doc = Engine.Transform(html);

            var xaml = Utils.AsXaml(doc);

            Approvals.Verify(xaml);
        }

        [Test]
        [RunOnUI]
        public void TypicalInline()
        {
            var html = Utils.ReadHtml();

            var doc = Engine.Transform(html);

            var xaml = Utils.AsXaml(doc);

            Approvals.Verify(xaml);
        }

        [Test]
        [RunOnUI]
        public void Mixing()
        {
            var html = Utils.ReadHtml();

            var doc = Engine.Transform(html);

            var xaml = Utils.AsXaml(doc);

            Approvals.Verify(xaml);
        }
    }
}
