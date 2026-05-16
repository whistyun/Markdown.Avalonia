using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ColorTextBlock.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColorDocument.Avalonia.DocumentElements
{
    public class CTextBlockElement : DocumentElement
    {
        private TextAlignment? _alignment;
        private Lazy<CTextBlock> _text;

        public string Text => _text.Value.Text;

        public override Control Control => _text.Value;

        public List<CInline> Inlines { get; }

        public override IEnumerable<DocumentElement> Children => Array.Empty<DocumentElement>();

        public CTextBlockElement(IEnumerable<CInline> inlines)
        {
            Inlines = inlines.ToList();
            _text = new Lazy<CTextBlock>(CreateTextBlock);
        }

        public CTextBlockElement(IEnumerable<CInline> inlines, string appendClass)
            : this(inlines)
        {
            Classes.Add(appendClass);
        }

        public CTextBlockElement(IEnumerable<CInline> inlines, string appendClass, TextAlignment alignment)
            : this(inlines, appendClass)
        {
            _alignment = alignment;
        }

        private CTextBlock CreateTextBlock()
        {
            var text = new CTextBlock();
            foreach (var inline in Inlines)
                text.Content.Add(inline);

            if (_alignment.HasValue)
                text.TextAlignment = _alignment.Value;

            ApplyEffects(text);
            return text;
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
