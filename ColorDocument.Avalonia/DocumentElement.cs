using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using System.Collections.Generic;
using System.Text;

namespace ColorDocument.Avalonia
{
    public abstract class DocumentElement
    {
        public abstract Control Control { get; }

        public abstract IEnumerable<DocumentElement> Children { get; }

        public Rect GetRect(Layoutable anchor) => Control.GetRectInDoc(anchor).GetValueOrDefault();
        public abstract void Select(Point from, Point to);
        public abstract void UnSelect();

        public virtual string GetSelectedText()
        {
            var builder = new StringBuilder();
            ConstructSelectedText(builder);
            return builder.ToString();
        }

        public abstract void ConstructSelectedText(StringBuilder stringBuilder);

    }
}
