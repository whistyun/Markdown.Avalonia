using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColorDocument.Avalonia.DocumentElements
{
    public class TableCellElement : DocumentElement
    {
        private readonly Lazy<Border> _control;
        private readonly EnumerableEx<DocumentElement> _items;
        private SelectionList? _prevSelection;

        internal int Row { get; set; }
        internal int Column { get; set; }
        public int RowSpan { set; get; }
        public int ColSpan { set; get; }
        public TextAlignment? Horizontal { set; get; }
        public VerticalAlignment? Vertical { set; get; }

        public override Control Control => _control.Value;

        public override IEnumerable<DocumentElement> Children => _items;

        public TableCellElement(DocumentElement cell)
        {
            _items = new[] { cell }.ToEnumerable();
            _control = new Lazy<Border>(CreateCell);
        }
        public TableCellElement(IEnumerable<DocumentElement> cells)
        {
            _items = cells.ToEnumerable();
            _control = new Lazy<Border>(CreateCell);
        }

        public override void Select(Point from, Point to)
        {
            var selection = SelectionUtil.SelectVertical(Control, _items, from, to);

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
            foreach (var child in _items)
                child.UnSelect();
        }

        private Border CreateCell()
        {
            if (_items.Count == 1)
            {

                return new Border() { Child = Setup(_items[0].Control) };
            }
            else
            {
                var pnl = new StackPanel() { Orientation = Orientation.Vertical };
                foreach (var cnt in _items)
                    pnl.Children.Add(Setup(cnt.Control));

                return new Border() { Child = pnl };
            }
        }

        private Control Setup(Control control)
        {
            if (Horizontal.HasValue)
            {
                control.SetCurrentValue(TextBlock.TextAlignmentProperty, Horizontal.Value);

                switch (Horizontal.Value)
                {
                    case TextAlignment.Left:
                        control.HorizontalAlignment = HorizontalAlignment.Left;
                        break;
                    case TextAlignment.Right:
                        control.HorizontalAlignment = HorizontalAlignment.Right;
                        break;
                    case TextAlignment.Center:
                        control.HorizontalAlignment = HorizontalAlignment.Center;
                        break;
                }
            }
            return control;
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
