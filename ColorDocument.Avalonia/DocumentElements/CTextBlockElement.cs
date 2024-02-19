using Avalonia;
using Avalonia.Controls;
using ColorTextBlock.Avalonia;
using System;
using System.Collections.Generic;

namespace ColorDocument.Avalonia.DocumentElements
{
    public class CTextBlockElement : DocumentElement
    {
        private Lazy<CTextBlock> _text;

        public string Text => _text.Value.Text;

        public override Control Control => _text.Value;

        public override IEnumerable<DocumentElement> Children => Array.Empty<DocumentElement>();

        public CTextBlockElement(CTextBlock block)
        {
            _text = new Lazy<CTextBlock>(() => block);
        }

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


        public override SelectDirection Select(Point from, Point to)
        {
            var text = _text.Value;

            var fromPoint = text.CalcuatePointerFrom(from.X, from.Y);
            var toPoint = text.CalcuatePointerFrom(to.X, to.Y);

            text.Selection = new Selection(fromPoint, toPoint);

            return fromPoint <= toPoint ? SelectDirection.Forward : SelectDirection.Backward;
        }

        public override void UnSelect()
        {
            _text.Value.Selection = null;
        }
    }
}
