using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ColorTextBlock.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColorDocument.Avalonia.DocumentElements
{
    public class ListBlockElement : DocumentElement
    {
        private Lazy<Grid> _control;
        private EnumerableEx<ListItemElement> _items;
        private SelectionList? _prevSelection;

        public override Control Control => _control.Value;
        public override IEnumerable<DocumentElement> Children => _items;

        public ListBlockElement(TextMarkerStyle marker, IEnumerable<ListItemElement> items)
        {
            _control = new Lazy<Grid>(() => CreateList(marker));
            _items = items.ToEnumerable();
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
            foreach (var c in _items)
                c.UnSelect();
        }

        private Grid CreateList(TextMarkerStyle marker)
        {
            var grid = new Grid();
            grid.Classes.Add(ClassNames.ListClass);
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            int index = 0;
            foreach (var item in _items)
            {
                var markerTxt = new CTextBlock(marker.CreateMakerText(index));
                var itemCtrl = item.Control;

                item.MarkerText = markerTxt.Text;

                // adjust baseline
                if (FindFirstFrom(itemCtrl) is { } controlTxt)
                    markerTxt.ObserveBaseHeightOf(controlTxt);

                grid.RowDefinitions.Add(new RowDefinition());

                markerTxt.TextAlignment = TextAlignment.Right;
                markerTxt.TextWrapping = TextWrapping.NoWrap;
                markerTxt.Classes.Add(ClassNames.ListMarkerClass);
                Grid.SetRow(markerTxt, index);
                Grid.SetColumn(markerTxt, 0);
                grid.Children.Add(markerTxt);

                Grid.SetRow(itemCtrl, index);
                Grid.SetColumn(itemCtrl, 1);
                grid.Children.Add(itemCtrl);

                ++index;
            }

            return grid;

            static CTextBlock? FindFirstFrom(Control ctrl)
            {
                if (ctrl is Panel pnl)
                {
                    foreach (var chld in pnl.Children)
                    {
                        var res = FindFirstFrom(chld);
                        if (res != null) return res;
                    }
                }
                if (ctrl is CTextBlock ctxt)
                {
                    return ctxt;
                }
                return null;
            }
        }

        public override void ConstructSelectedText(StringBuilder builder)
        {
            if (_prevSelection is null)
                return;

            foreach (var para in _prevSelection.Cast<ListItemElement>())
            {
                builder.Append(para.MarkerText).Append(' ');

                var listElmTxt = para.GetSelectedText().Replace("\r\n", "\n").Replace('\r', '\n');
                builder.Append(listElmTxt);

                if (!listElmTxt.EndsWith("\n"))
                    builder.Append('\n');
            }
        }
    }
}
