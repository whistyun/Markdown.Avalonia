using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using ColorDocument.Avalonia;
using ColorDocument.Avalonia.DocumentElements;
using ColorTextBlock.Avalonia;
using NUnit.Framework;
using System;
using UnitTest.Base.Utils;

namespace UnitTest.Md
{
    public class DocumentGroupElementTests
    {
        [Test]
        [RunOnUI]
        public void ClassTest()
        {
            var line = new CTextBlockElement(new[] { new CRun { Text = "x" } });
            var group = new DocumentGroupElement(new[] { line });
            group.Classes.Add("test");

            var panel = group.Control;
            Assert.That(panel.Classes, Does.Contain("test"));
        }

        [Test]
        [RunOnUI]
        public void HorizontalAlignmentTest()
        {
            var line = new CTextBlockElement(new[] { new CRun { Text = "x" } });
            var group = new DocumentGroupElement(new[] { line });
            group.HorizontalAlignment = HorizontalAlignment.Center;

            var panel = (StackPanel)group.Control;
            var txt = (CTextBlock)panel.Children[0];

            Assert.That(panel.HorizontalAlignment, Is.EqualTo(HorizontalAlignment.Center));
            Assert.That(txt.TextAlignment, Is.EqualTo(TextAlignment.Center));
        }

        [Test]
        [RunOnUI]
        public void BorderdClassTest()
        {
            var bordered = new BorderedDocumentGroupElement(Array.Empty<DocumentElement>());
            bordered.Classes.Add("test");

            var border = bordered.Control;
            var innerPanel = ((Border)bordered.Control).Child;
            if (innerPanel is null)
                throw new NullReferenceException(nameof(innerPanel));

            Assert.That(innerPanel.Classes, Does.Contain("test"));
            Assert.That(border.Classes, Does.Contain("test"));
        }

        [Test]
        [RunOnUI]
        public void BorderedHorizontalAlignmentTest()
        {
            var line = new CTextBlockElement(new[] { new CRun { Text = "x" } });
            var bordered = new BorderedDocumentGroupElement(new[] { line });
            bordered.HorizontalAlignment = HorizontalAlignment.Right;

            var border = (Border)bordered.Control;
            Assert.That(border.HorizontalAlignment, Is.EqualTo(HorizontalAlignment.Right));

            var innerPanel = border.Child as StackPanel;
            if (innerPanel is null)
                throw new NullReferenceException(nameof(innerPanel));

            var txt = (CTextBlock)innerPanel.Children[0];
            Assert.That(innerPanel.HorizontalAlignment, Is.EqualTo(HorizontalAlignment.Right));
            Assert.That(txt.TextAlignment, Is.EqualTo(TextAlignment.Right));
        }
    }
}
