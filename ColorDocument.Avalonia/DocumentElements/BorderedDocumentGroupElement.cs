using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using System;
using System.Collections.Generic;
using System.Text;

namespace ColorDocument.Avalonia.DocumentElements
{
    public class BorderedDocumentGroupElement : DocumentElement
    {
        private readonly DocumentGroupElement _inner;
        private readonly Lazy<Border> _border;

        public override Control Control => _border.Value;
        public override IEnumerable<DocumentElement> Children => _inner.Children;

        public BorderedDocumentGroupElement(IEnumerable<DocumentElement> inner)
        {
            if (inner is null)
                throw new ArgumentNullException(nameof(inner));

            _inner = new DocumentGroupElement(inner);
            _border = new Lazy<Border>(Create);
        }

        private Border Create()
        {
            var border = new Border();
            ApplyEffects(border);
            border.Child = _inner.Control;
            return border;
        }

        protected override void OnClassAdded(string className)
            => _inner.Classes.Add(className);

        protected override void OnHorizontalAlignmentChanged(HorizontalAlignment alignment)
            => _inner.HorizontalAlignment = alignment;

        public override void Select(Point from, Point to)
        {
            _inner.Select(from, to);
        }

        public override void UnSelect()
        {
            _inner.UnSelect();
        }

        public override void ConstructSelectedText(StringBuilder builder)
        {
            _inner.ConstructSelectedText(builder);
        }
    }
}
