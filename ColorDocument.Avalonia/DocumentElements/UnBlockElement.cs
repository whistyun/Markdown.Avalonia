using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;

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

        public override SelectDirection Select(Point from, Point to)
        {
            if (Math.Abs(from.X - to.X) > Math.Abs(from.Y - to.Y))
            {
                return from.X < to.X ? SelectDirection.Forward : SelectDirection.Backward;
            }
            else
            {
                return from.Y < to.Y ? SelectDirection.Forward : SelectDirection.Backward;
            }
        }

        public override void UnSelect()
        {
        }
    }
}

