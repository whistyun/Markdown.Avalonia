using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using System;
using System.Collections.Generic;

namespace ColorDocument.Avalonia.DocumentElements
{
    /// <summary>
    /// The document element for expression of blockquote.
    /// </summary>
    // 引用を表現するためのドキュメント要素
    public class BlockquoteElement : DocumentElement
    {
        private Lazy<Border> _block;
        private EnumerableEx<DocumentElement> _children;

        public override Control Control => _block.Value;
        public override IEnumerable<DocumentElement> Children => _children;

        public BlockquoteElement(IEnumerable<DocumentElement> child)
        {
            _block = new Lazy<Border>(Create);
            _children = child.ToEnumerable();
        }

        private Border Create()
        {
            var panel = new StackPanel();
            panel.Orientation = Orientation.Vertical;
            panel.Classes.Add(ClassNames.BlockquoteClass);
            foreach (var child in Children)
                panel.Children.Add(child.Control);

            var border = new Border();
            border.Classes.Add(ClassNames.BlockquoteClass);
            border.Child = panel;

            return border;
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
