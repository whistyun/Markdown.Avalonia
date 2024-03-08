using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace ColorDocument.Avalonia.DocumentElements
{
    public class TextBlockElement : DocumentElement
    {
        private Lazy<TextBlock> _text;

        public string? Text => _text.Value.Text;

        public override Control Control => _text.Value;

        public override IEnumerable<DocumentElement> Children => Array.Empty<DocumentElement>();

        public TextBlockElement(IEnumerable<Inline> inlines)
        {
            _text = new Lazy<TextBlock>(() =>
            {
                var text = new TextBlock();
                if (text.Inlines is null)
                {
                    text.Inlines = new InlineCollection();
                }

                text.Inlines.AddRange(inlines);

                return text;
            });
        }

        public TextBlockElement(IEnumerable<Inline> inlines, string appendClass)
        {
            _text = new Lazy<TextBlock>(() =>
            {
                var text = new TextBlock();
                if (text.Inlines is null)
                {
                    text.Inlines = new InlineCollection();
                }
                text.Inlines.AddRange(inlines);

                text.Classes.Add(appendClass);
                return text;
            });
        }

        public TextBlockElement(IEnumerable<Inline> inlines, string appendClass, TextAlignment alignment)
        {
            _text = new Lazy<TextBlock>(() =>
            {
                var text = new TextBlock();
                if (text.Inlines is null)
                {
                    text.Inlines = new InlineCollection();
                }
                text.Inlines.AddRange(inlines);

                text.TextAlignment = alignment;
                text.Classes.Add(appendClass);
                return text;
            });
        }

        public override void Select(Point from, Point to) { }

        public override void UnSelect() { }

        public override void ConstructSelectedText(StringBuilder stringBuilder)
        {
        }
    }
}
