using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using ColorTextBlock.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColorDocument.Avalonia.DocumentElements
{
    /// <summary>
    /// The document element for stacking child elements vertically.
    /// </summary>
    public class DocumentGroupElement : DocumentElement
    {
        private readonly Lazy<StackPanel> _stack;
        private readonly EnumerableEx<DocumentElement> _children;
        private SelectionList? _prevSelection;

        public override Control Control => _stack.Value;
        public override IEnumerable<DocumentElement> Children => _children;

        public DocumentGroupElement(IEnumerable<DocumentElement> children)
        {
            _stack = new Lazy<StackPanel>(Create);
            _children = children.ToEnumerable();
        }

        private StackPanel Create()
        {
            var panel = new StackPanel();
            panel.Orientation = Orientation.Vertical;
            ApplyEffects(panel);

            foreach (var child in _children)
            {
                var childControl = child.Control;
                panel.Children.Add(childControl);

                if (HorizontalAlignment.HasValue)
                {
                    switch (childControl)
                    {
                        case TextBlock tb:
                            tb.TextAlignment = Map(HorizontalAlignment.Value);
                            break;
                        case CTextBlock ctb:
                            ctb.TextAlignment = Map(HorizontalAlignment.Value);
                            break;
                        default:
                            childControl.HorizontalAlignment = HorizontalAlignment.Value;
                            break;
                    }
                }
            }

            return panel;

            static TextAlignment Map(HorizontalAlignment ha) => ha switch
            {
                global::Avalonia.Layout.HorizontalAlignment.Left => TextAlignment.Left,
                global::Avalonia.Layout.HorizontalAlignment.Center => TextAlignment.Center,
                global::Avalonia.Layout.HorizontalAlignment.Right => TextAlignment.Right,
                _ => TextAlignment.Left,
            };
        }

        public override void Select(Point from, Point to)
        {
            var selection = SelectionUtil.SelectVertical(Control, _children, from, to);

            if (_prevSelection is not null)
            {
                foreach (var prev in _prevSelection)
                {
                    if (!selection.Any(current => ReferenceEquals(current, prev)))
                    {
                        prev.UnSelect();
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

        public override void ConstructSelectedText(StringBuilder builder)
        {
            if (_prevSelection is null)
                return;

            var preLen = builder.Length;

            foreach (var element in _prevSelection)
            {
                element.ConstructSelectedText(builder);

                if (preLen == builder.Length)
                    continue;

                if (builder[builder.Length - 1] != '\n')
                    builder.Append('\n');
            }
        }
    }
}
