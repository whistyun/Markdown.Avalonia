using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using System.Collections.Generic;
using System.Text;

namespace ColorDocument.Avalonia
{
    public abstract class DocumentElement
    {
        private ISelectionRenderHelper? _helper;

        public abstract Control Control { get; }
        public abstract IEnumerable<DocumentElement> Children { get; }

        public ISelectionRenderHelper? Helper
        {
            get => _helper;
            set
            {
                _helper = value;
                foreach (var child in Children)
                    child.Helper = value;
            }
        }

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

    public interface ISelectionRenderHelper
    {
        void Register(Control control);
        void Unregister(Control control);
    }
}
