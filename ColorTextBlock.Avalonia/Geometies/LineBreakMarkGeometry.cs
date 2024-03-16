using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using System;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace ColorTextBlock.Avalonia.Geometries
{
    internal class LineBreakMarkGeometry : TextGeometry
    {
        private bool IsDummy { get; }

        internal LineBreakMarkGeometry(
            CInline owner,
            double lineHeight) :
            base(owner, 0, lineHeight, lineHeight, TextVerticalAlignment.Base, true)
        {
            IsDummy = false;
        }

        internal LineBreakMarkGeometry(CInline owner) :
            base(owner, 0, 0, 0, TextVerticalAlignment.Base, true)
        {
            IsDummy = true;
        }

        public override void Render(DrawingContext ctx) { }

        public override TextPointer CalcuatePointerFrom(double x, double y)
        {
            throw new InvalidOperationException();
        }

        public override TextPointer CalcuatePointerFrom(int index)
        {
            return index switch
            {
                0 => GetBegin(),
                1 => GetEnd(),
                _ => throw new ArgumentOutOfRangeException(nameof(index))
            };
        }

        public override TextPointer GetBegin()
        {
            return new TextPointer(this);
        }

        public override TextPointer GetEnd()
        {
            return IsDummy ?
                new TextPointer(this) :
                new TextPointer(this, 1, Width);
        }
    }
}
