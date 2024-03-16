using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ColorTextBlock.Avalonia;
using System;
using System.Collections.Generic;
using System.Text;

namespace ColorDocument.Avalonia.DocumentElements
{
    public class NoteBlockElement : DocumentElement
    {
        private CTextBlockElement _child;
        private TextAlignment? _indiAlignment;
        private Lazy<Border> _block;

        public override Control Control => _block.Value;

        public override IEnumerable<DocumentElement> Children => new[] { _child };

        public NoteBlockElement(IEnumerable<CInline> content, TextAlignment? indiAlignment)
        {
            _child = new CTextBlockElement(content);
            _indiAlignment = indiAlignment;

            _block = new Lazy<Border>(Create);
        }

        private Border Create()
        {
            var note = (CTextBlock)_child.Control;

            note.Classes.Add(ClassNames.NoteClass);
            if (_indiAlignment.HasValue)
            {
                note.TextAlignment = _indiAlignment.Value;
            }

            var result = new Border();
            result.Classes.Add(ClassNames.NoteClass);
            result.Child = note;

            return result;
        }

        public override void Select(Point from, Point to)
            => _child.Select(from, to);

        public override void UnSelect()
        {
            _child.UnSelect();
        }

        public override void ConstructSelectedText(StringBuilder builder)
        {
            _child.ConstructSelectedText(builder);
        }
    }
}
