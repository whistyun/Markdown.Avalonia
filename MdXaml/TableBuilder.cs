using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows;

namespace MdXaml
{
    class MdTable
    {
        public List<MdTableCell> Header { get; }
        public List<List<MdTableCell>> Details { get; }
        public int ColCount { get; }
        public int RowCount { get; }


        public MdTable(
            string[] header,
            string[] styles,
            IList<string[]> details)
        {
            Header = header.Select(txt =>
            {
                var cell = new MdTableCell(txt);
                // Ignore Header Row-span
                cell.RowSpan = 1;
                return cell;
            }).ToList();

            Details = details.Select(row =>
                row.Select(txt => new MdTableCell(txt)).ToList()
            ).ToList();

            var styleMt = new OverrunList<Nullable<TextAlignment>>(
                styles.Select(txt =>
                {
                    var firstChar = txt[0];
                    var lastChar = txt[txt.Length - 1];

                    return
                        (firstChar == ':' && lastChar == ':') ?
                            (Nullable<TextAlignment>)TextAlignment.Center :

                        (lastChar == ':') ?
                            (Nullable<TextAlignment>)TextAlignment.Right :

                        (firstChar == ':') ?
                            (Nullable<TextAlignment>)TextAlignment.Left :

                            null;
                }),
                () => null
            );

            // apply cell style to header
            var colOffset = 0;
            foreach (var cell in Header)
            {
                var style = styleMt[colOffset];
                if (style.HasValue) cell.Horizontal = style;

                colOffset += cell.ColSpan;
            }

            ColCount = Header.Sum(cell => cell.ColSpan);

            var rowSpanLife = new OverrunList<Span>(() => new Span(-1, 1));
            // apply cell style to header
            foreach (var row in Details)
            {
                colOffset = 0;

                foreach (var cell in row)
                {
                    var span = rowSpanLife[colOffset];

                    int colSpan;
                    if (span.Life <= 0)
                    {
                        colSpan = cell.ColSpan;

                        var style = styleMt[colOffset];
                        if (style.HasValue && !cell.Horizontal.HasValue)
                            cell.Horizontal = style;

                        if (cell.RowSpan > 1)
                        {
                            rowSpanLife[colOffset] =
                                new Span(cell.RowSpan, cell.ColSpan);
                        }
                    }
                    else
                    {
                        colSpan = span.ColSpan;

                        var style = styleMt[colOffset + colSpan];
                        if (style.HasValue && !cell.Horizontal.HasValue)
                            cell.Horizontal = style;

                    }

                    for (var j = 0; j < colSpan; ++j)
                    {
                        rowSpanLife[colOffset++].Life--;
                    }
                }

                ColCount = Math.Max(ColCount, colOffset);
            }

            RowCount = Details.Count;

            //while (Header.Count < ColCount)
            //    Header.Add(new MdTableCell(""));
            //
            //foreach (var row in Details)
            //    while (row.Count < ColCount)
            //        row.Add(new MdTableCell(""));
        }   //
    }

    class Span
    {
        public int Life { set; get; }
        public int ColSpan { set; get; }

        public Span(int l, int c)
        {
            Life = l;
            ColSpan = c;
        }
    }

    class OverrunList<T>
    {
        Func<T> IndexOutVal;

        List<T> list = new List<T>();

        public OverrunList(Func<T> indexOutVal)
        {
            IndexOutVal = indexOutVal;
        }

        public OverrunList(IEnumerable<T> lst, Func<T> indexOutVal) : this(indexOutVal)
        {
            list = lst.ToList();
        }

        public T this[int idx]
        {
            set
            {
                while (idx >= list.Count)
                    list.Add(IndexOutVal());

                list[idx] = value;
            }
            get
            {
                return idx < list.Count ? list[idx] : IndexOutVal();
            }
        }

        public IEnumerable<T> Enumerable
        {
            get { return list; }
        }
    }

    class MdTableCell
    {
        public string RawText { get; }
        public string Text { get; }
        public int RowSpan { set; get; }
        public int ColSpan { set; get; }
        public TextAlignment? Horizontal { set; get; }
        public VerticalAlignment? Vertical { set; get; }

        public MdTableCell(string txt)
        {
            RawText = txt;
            RowSpan = 1;
            ColSpan = 1;
            Horizontal = null;
            Vertical = null;

            int idx = txt.IndexOf('.');

            if (idx == -1)
            {
                Text = txt.Trim();
            }
            else
            {

                var styleTxt = txt.Substring(0, idx);

                for (var i = 0; i < styleTxt.Length; ++i)
                {
                    var c = styleTxt[i];

                    switch (c)
                    {
                        case '/': // /2 rowspan
                            ++i;
                            var numTxt = ContinueToNum(styleTxt, ref i);
                            if (numTxt.Length == 0) goto default;
                            RowSpan = Int32.Parse(numTxt);

                            break;

                        case '\\': // \2 colspan
                            ++i;
                            numTxt = ContinueToNum(styleTxt, ref i);
                            if (numTxt.Length == 0) goto default;
                            ColSpan = Int32.Parse(numTxt);
                            break;

                        case '<': // < left align
                            Horizontal = TextAlignment.Left;
                            break;

                        case '>': // > right align
                            Horizontal = TextAlignment.Right;
                            break;

                        case '=': // = center align 
                            Horizontal = TextAlignment.Center;
                            break;

                        case '^': // ^ top align
                            Vertical = VerticalAlignment.Top;
                            break;

                        case '~': // ~ bottom align
                            Vertical = VerticalAlignment.Bottom;
                            break;

                        default:
                            Text = txt.Trim();
                            RowSpan = 1;
                            ColSpan = 1;
                            Horizontal = null;
                            Vertical = null;
                            return;
                    }
                }
                Text = txt.Substring(idx + 1).Trim();
            }
        }

        private static string ContinueToNum(string charSource, ref int idx)
        {
            var builder = new StringBuilder();

            for (; idx < charSource.Length; ++idx)
            {
                var c = charSource[idx];

                if ('0' <= c && c <= '9')
                    builder.Append(c);

                else break;
            }
            --idx;
            return builder.ToString();
        }
    }
}
