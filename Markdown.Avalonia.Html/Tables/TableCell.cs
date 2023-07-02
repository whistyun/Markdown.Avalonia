using System;
using System.Text;
using Avalonia.Media;
using Avalonia.Layout;
using System.Collections.Generic;
using Avalonia.Controls;
using System.Linq;

namespace Markdown.Avalonia.Html.Tables
{
    class TableCell
    {
        public int ColumnIndex { set; get; }

        public Border Content { get; set; }
        public int RowSpan { set; get; }
        public int ColSpan { set; get; }
        public TextAlignment? Horizontal { set; get; }
        public VerticalAlignment? Vertical { set; get; }

        public TableCell()
        {
            RowSpan = 1;
            ColSpan = 1;
            Horizontal = null;
            Vertical = null;
            Content = new();
        }

        public TableCell(IEnumerable<Control> controls) : this()
        {
            var ctrls = controls.ToArray();
            switch (ctrls.Length)
            {
                case 0:
                    break;

                case 1:
                    Content.Child = ctrls[0];
                    break;

                default:
                    var panel = new StackPanel() { Orientation = Orientation.Vertical };
                    panel.Children.AddRange(ctrls);

                    Content.Child = panel;
                    break;
            }
        }
    }
}
