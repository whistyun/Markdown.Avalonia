using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace ColorDocument.Avalonia.DocumentElements
{
    public class UnBlockElement : DocumentElement
    {
        private Control _control;

        public override Control Control => _control;

        public override IEnumerable<DocumentElement> Children => Array.Empty<DocumentElement>();

        public UnBlockElement(Control control)
        {
            _control = control;
        }

        public override void Select(Point from, Point to) { }

        public override void UnSelect() { }

        public override void ConstructSelectedText(StringBuilder stringBuilder)
        {
        }
    }
}

