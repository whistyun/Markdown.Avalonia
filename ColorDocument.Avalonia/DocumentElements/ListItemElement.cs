using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ColorDocument.Avalonia.DocumentElements
{
    public class ListItemElement : DocumentElement
    {
        private Lazy<StackPanel> _panel;
        private EnumerableEx<DocumentElement> _elements;
        private List<DocumentElement>? _prevSelection;

        public override Control Control => _panel.Value;
        public override IEnumerable<DocumentElement> Children => _elements;

        public ListItemElement(IEnumerable<DocumentElement> contents)
        {
            _elements = contents.ToEnumerable();
            _panel = new Lazy<StackPanel>(() =>
            {
                var panel = new StackPanel();
                foreach (var content in _elements)
                    panel.Children.Add(content.Control);

                return panel;
            });
        }


        public override void Select(Point from, Point to)
        {
            var selection = SelectionUtil.SelectVertical(Control, _elements, from, to);

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
            foreach (var c in _elements)
                c.UnSelect();
        }
    }
}
