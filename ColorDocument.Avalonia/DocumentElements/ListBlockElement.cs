using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ColorTextBlock.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ColorDocument.Avalonia.DocumentElements
{
    public class ListBlockElement : DocumentElement
    {
        private Lazy<Grid> _control;
        private EnumerableEx<ListItemElement> _items;

        public override Control Control => _control.Value;
        public override IEnumerable<DocumentElement> Children => _items;

        public override SelectDirection Select(Point from, Point to)
            => SelectionUtil.SelectVertical(_items, from, to);

        public override void UnSelect()
        {
            foreach (var c in _items)
                c.UnSelect();
        }

        public ListBlockElement(TextMarkerStyle marker, IEnumerable<ListItemElement> items)
        {
            _control = new Lazy<Grid>(() => CreateList(marker, items));
            _items = items.ToEnumerable();
        }

        private Grid CreateList(TextMarkerStyle marker, IEnumerable<ListItemElement> items)
        {
            var grid = new Grid();
            grid.Classes.Add(ClassNames.ListClass);
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            int index = 0;
            foreach (var item in items)
            {
                var markerTxt = new CTextBlock(marker.CreateMakerText(index));
                var itemCtrl = item.Control;

                // adjust baseline
                if (FindFirstFrom(itemCtrl) is { } controlTxt)
                    markerTxt.ObserveBaseHeightOf(controlTxt);

                grid.RowDefinitions.Add(new RowDefinition());
                grid.Children.Add(markerTxt);
                grid.Children.Add(itemCtrl);

                markerTxt.TextAlignment = TextAlignment.Right;
                markerTxt.TextWrapping = TextWrapping.NoWrap;
                markerTxt.Classes.Add(ClassNames.ListMarkerClass);
                Grid.SetRow(markerTxt, index);
                Grid.SetColumn(markerTxt, 0);

                Grid.SetRow(itemCtrl, index);
                Grid.SetColumn(itemCtrl, 1);
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
    }
}
