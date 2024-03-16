using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ColorTextBlock.Avalonia;
using System;
using System.Collections.Generic;
using System.Text;

namespace ColorDocument.Avalonia.DocumentElements
{
    public class CTextBlockElement : DocumentElement
    {
        private Lazy<CTextBlock> _text;

        public string Text => _text.Value.Text;

        public override Control Control => _text.Value;

        public override IEnumerable<DocumentElement> Children => Array.Empty<DocumentElement>();

        public CTextBlockElement(IEnumerable<CInline> inlines)
        {
            _text = new Lazy<CTextBlock>(() =>
            {
                var text = new CTextBlock();
                foreach (var inline in inlines)
                    text.Content.Add(inline);
                return text;
            });
        }
        public CTextBlockElement(IEnumerable<CInline> inlines, string appendClass)
        {
            _text = new Lazy<CTextBlock>(() =>
            {
                var text = new CTextBlock();
                foreach (var inline in inlines)
                    text.Content.Add(inline);

                text.Classes.Add(appendClass);
                return text;
            });
        }

        public CTextBlockElement(IEnumerable<CInline> inlines, string appendClass, TextAlignment alignment)
        {
            _text = new Lazy<CTextBlock>(() =>
            {
                var text = new CTextBlock();
                foreach (var inline in inlines)
                    text.Content.Add(inline);

                text.TextAlignment = alignment;
                text.Classes.Add(appendClass);
                return text;
            });
        }


        public override void Select(Point from, Point to)
        {
            var text = _text.Value;

            var fromPoint = text.CalcuatePointerFrom(from.X, from.Y);
            var toPoint = text.CalcuatePointerFrom(to.X, to.Y);
            text.Select(fromPoint, toPoint);
        }

        public override void UnSelect()
        {
            _text.Value.ClearSelection();
        }

        public override void ConstructSelectedText(StringBuilder builder)
        {
            builder.Append(_text.Value.GetSelectedText());
        }
    }
}
