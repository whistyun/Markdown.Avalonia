using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using System;
using System.Collections.Generic;

namespace ColorDocument.Avalonia.DocumentElements
{
    /// <summary>
    /// The top document element.
    /// </summary>
    public class DocumentRootElement : DocumentElement
    {
        private Lazy<StackPanel> _block;
        private EnumerableEx<DocumentElement> _children;

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

        public override SelectDirection Select(Point from, Point to)
            => SelectionUtil.SelectVertical(_children, from, to);

        public override void UnSelect()
        {
            foreach (var child in _children)
                child.UnSelect();
        }
    }
}
