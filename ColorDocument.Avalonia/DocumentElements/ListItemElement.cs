using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using ColorDocument.Avalonia;
using System;
using System.Collections.Generic;
using System.Text;

namespace ColorDocument.Avalonia.DocumentElements
{
    public class ListItemElement : DocumentElement
    {
        private readonly DocumentGroupElement _stack;

        internal string MarkerText { get; set; } = string.Empty;

        public override Control Control => _stack.Control;
        public override IEnumerable<DocumentElement> Children => _stack.Children;

        public ListItemElement(IEnumerable<DocumentElement> contents)
        {
            _stack = new DocumentGroupElement(contents);
        }

        protected override void OnClassAdded(string className)
            => _stack.Classes.Add(className);

        protected override void OnHorizontalAlignmentChanged(HorizontalAlignment alignment)
            => _stack.HorizontalAlignment = alignment;

        public override void Select(Point from, Point to)
        {
            _stack.Select(from, to);
        }

        public override void UnSelect()
        {
            _stack.UnSelect();
        }

        public override void ConstructSelectedText(StringBuilder builder)
        {
            _stack.ConstructSelectedText(builder);
        }
    }
}
