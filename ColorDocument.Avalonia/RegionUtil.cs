using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using ColorDocument.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Markdown.Avalonia.Utils
{
    internal static class RegionUtil
    {
        public static Rect? GetRectInDoc(this Control control)
        {
            var baseRect = LayoutInformation.GetPreviousArrangeBounds(control);
            if (!baseRect.HasValue)
                return null;

            double driftX = 0;
            double driftY = 0;

            for (StyledElement? c = control.Parent;
                    c is not null
                    && c is Layoutable layoutable
                    && !(layoutable is ScrollViewer);
                    c = c.Parent)
            {
                var ar = LayoutInformation.GetPreviousArrangeBounds(layoutable);
                if (ar.HasValue)
                {
                    driftX += ar.Value.Left;
                    driftY += ar.Value.Top;
                }
                else return null;
            }

            return new Rect(
                        baseRect.Value.X + driftX,
                        baseRect.Value.Y + driftY,
                        baseRect.Value.Width,
                        baseRect.Value.Height);
        }



        public static EnumerableEx<DocumentElementWithBound> GetRectInDoc<T>(this EnumerableEx<T> controls)
            where T : DocumentElement
        {
            var rs = new DocumentElementWithBound[controls.Count];
            for (var i = 0; i < rs.Length; ++i)
            {
                var doc = controls[i];
                var rect = doc.Control.GetRectInDoc();
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
