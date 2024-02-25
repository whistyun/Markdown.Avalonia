using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ColorDocument.Avalonia.DocumentElements
{
    /// <summary>
    /// The top document element.
    /// </summary>
    public class DocumentRootElement : DocumentElement
    {
        private Lazy<StackPanel> _block;
        private EnumerableEx<DocumentElement> _children;
        private List<DocumentElement>? _prevSelection;

        public override Control Control => _block.Value;
        public override IEnumerable<DocumentElement> Children => _children;

        public DocumentRootElement(IEnumerable<DocumentElement> child)
        {
            _block = new Lazy<StackPanel>(Create);
            _children = child.ToEnumerable();
        }

        private StackPanel Create()
        {
            var panel = new StackPanel();
            panel.Orientation = Orientation.Vertical;
            foreach (var child in _children)
                panel.Children.Add(child.Control);

            return panel;
        }

        public override void Select(Point from, Point to)
        {
            var selection = SelectionUtil.SelectVertical(Control, _children, from, to);

            if (_prevSelection is not null)
            {
                foreach (var ps in _prevSelection)
                {
                    if (!selection.Any(cs => ReferenceEquals(cs, ps)))
                    {
                        ps.UnSelect();
                    }
                }
            }

            _prevSelection = selection;
        }

        public override void UnSelect()
        {
            foreach (var child in _children)
                child.UnSelect();
        }
    }
}
