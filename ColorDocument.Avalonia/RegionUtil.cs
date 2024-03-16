using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace ColorDocument.Avalonia
{
    internal static class RegionUtil
    {
        public static Rect? GetRectInDoc(this Control control, Layoutable anchor)
        {
            if (!LayoutInformation.GetPreviousArrangeBounds(control).HasValue)
                return null;

            double driftX = 0;
            double driftY = 0;

            StyledElement? c;
            for (c = control.Parent;
                    c is not null
                    && c is Layoutable layoutable
                    && !ReferenceEquals(anchor, layoutable);
                    c = c.Parent)
            {
                driftX += layoutable.Bounds.X;
                driftY += layoutable.Bounds.Y;
            }

            return new Rect(
                        control.Bounds.X + driftX,
                        control.Bounds.Y + driftY,
                        control.Bounds.Width,
                        control.Bounds.Height);
        }



        public static EnumerableEx<DocumentElementWithBound> GetRectInDoc<T>(this EnumerableEx<T> controls, Layoutable anchor)
            where T : DocumentElement
        {
            var rs = new DocumentElementWithBound[controls.Count];
            for (var i = 0; i < rs.Length; ++i)
            {
                var doc = controls[i];
                var rect = doc.Control.GetRectInDoc(anchor);
                if (rect.HasValue)
                {
                    rs[i] = new DocumentElementWithBound(doc, rect.Value);
                }
            }

            return new EnumerableExAry<DocumentElementWithBound>(rs);
        }

    }

    internal struct DocumentElementWithBound
    {
        public DocumentElement Element { get; }
        public Rect Rect { get; }

        public DocumentElementWithBound(DocumentElement c, Rect r)
        {
            Element = c;
            Rect = r;
        }
    }
}
