using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using ColorTextBlock.Avalonia.Geometries;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace ColorTextBlock.Avalonia.Geometies
{
    internal class DummyGeometryForControl : CGeometry
    {
        public Control Control { get; }

        public DummyGeometryForControl(CInlineUIContainer owner, Control control, TextVerticalAlignment alignment) :
            base(
                owner,
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

        public override PhysicalTextPointer CalcuatePointerFrom(double x, double y)
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

        public override PhysicalTextPointer CalcuatePointerFrom(int index)
        {
            return index switch
            {
                0 => GetBegin(),
                1 => GetEnd(),
                _ => throw new ArgumentOutOfRangeException(nameof(index))
            };
        }

        public override PhysicalTextPointer GetBegin()
        {
            return new PhysicalTextPointer(this);
        }

        public override PhysicalTextPointer GetEnd()
        {
            return new PhysicalTextPointer(this, 1, Width);
        }
    }
}