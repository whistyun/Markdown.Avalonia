using Avalonia;
using Avalonia.Controls;
using ColorDocument.Avalonia.DocumentElements;
using Markdown.Avalonia.Utils;
using System.Collections.Generic;

namespace ColorDocument.Avalonia
{
    public abstract class DocumentElement
    {
        public abstract Control Control { get; }

        public abstract IEnumerable<DocumentElement> Children { get; }

        public Rect GetRect() => Control.GetRectInDoc().GetValueOrDefault();
        public abstract SelectDirection Select(Point from, Point to);
        public abstract void UnSelect();
    }
}
