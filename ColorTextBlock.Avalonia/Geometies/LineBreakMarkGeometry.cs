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
        internal LineBreakMarkGeometry(
            CInline owner,
            double lineHeight) :
            base(owner, 0, lineHeight, lineHeight, TextVerticalAlignment.Base, true)
        {

        }
        internal LineBreakMarkGeometry(CInline owner) :
            base(owner, 0, 0, 0, TextVerticalAlignment.Base, true)
        {
        }

        public override void Render(DrawingContext ctx) { }

        public override bool TryMoveNext(TextPointer current, out TextPointer next)
        {
            next = null;
            return false;
        }

        public override bool TryMovePrev(TextPointer current, out TextPointer prev)
        {
            prev = null;
            return false;
        }

        public override TextPointer CalcuatePointerFrom(double x, double y)
        {
            throw new InvalidOperationException();
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
