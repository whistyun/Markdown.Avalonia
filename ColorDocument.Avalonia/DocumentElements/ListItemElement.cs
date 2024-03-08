using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColorDocument.Avalonia.DocumentElements
{
    public class ListItemElement : DocumentElement
    {
        private Lazy<StackPanel> _panel;
        private EnumerableEx<DocumentElement> _elements;
        private SelectionList? _prevSelection;

        internal string MarkerText { get; set; }

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

        public override void ConstructSelectedText(StringBuilder builder)
        {
            if (_prevSelection is null)
                return;

            var preLen = builder.Length;

            foreach (var para in _prevSelection)
            {
                para.ConstructSelectedText(builder);

                if (preLen == builder.Length)
                    continue;

                if (builder[builder.Length - 1] != '\n')
                    builder.Append('\n');
            }
        }
    }
}
