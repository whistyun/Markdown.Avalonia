using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace Markdown.Avalonia.Tables
{
    class MdTableCell : ITableCell
    {
        public int ColumnIndex { set; get; }

        public string RawText { get; }

        public string Text => RawText;

        public int RowSpan => 1;

        public int ColSpan => 1;

        public TextAlignment? Horizontal { get; set; }

        public VerticalAlignment? Vertical { get; set; }

        public MdTableCell(string txt)
        {
            RawText = txt;
        }
    }
}
