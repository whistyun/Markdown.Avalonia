using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using System.Collections.Generic;

namespace ColorDocument.Avalonia
{
    public abstract class DocumentElement
    {
        public abstract Control Control { get; }

        public abstract IEnumerable<DocumentElement> Children { get; }

        public Rect GetRect(Layoutable anchor) => Control.GetRectInDoc(anchor).GetValueOrDefault();
        public abstract void Select(Point from, Point to);
        public abstract void UnSelect();
    }
}
