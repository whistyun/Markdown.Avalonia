using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ColorTextBlock.Avalonia.Geometries;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace ColorTextBlock.Avalonia.Geometies
{
    internal class DummyGeometryForControl : CGeometry
    {
        public Control Control { get; }

        public DummyGeometryForControl(Control control, TextVerticalAlignment alignment) :
            base(
                control.DesiredSize.Width,
                control.DesiredSize.Height,
                control.DesiredSize.Height,
                alignment,
                false)
        {
            Control = control;
        }

        public override void Render(DrawingContext ctx)
        {
        }

        public override bool TryMoveNext(TextPointer current, out TextPointer next)
        {
            if (current == GetBegin())
            {
                next = GetEnd();
                return true;
            }
            else
            {
                next = null;
                return false;
            }
        }

        public override bool TryMovePrev(TextPointer current, out TextPointer prev)
        {
            if (current == GetEnd())
            {
                prev = GetBegin();
                return true;
            }
            else
            {
                prev = null;
                return false;
            }
        }

        public override TextPointer CalcuatePointerFrom(double x, double y)
        {
            if (x < Left + Width / 2)
            {
                return GetBegin();
            }
            else
            {
                return GetEnd();
            }
        }

        public override TextPointer GetBegin()
        {
            return new TextPointer(this, 0, Left, Top, Height);
        }

        public override TextPointer GetEnd()
        {
            return new TextPointer(this, 1, Left, Top, Height);
        }
    }
}