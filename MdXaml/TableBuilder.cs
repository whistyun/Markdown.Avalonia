using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows;
using System.Text.RegularExpressions;

#if MIG_FREE
namespace Markdown.Xaml
#else
namespace MdXaml
#endif
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

            Dictionary<int, TextAlignment> styleMt = styles
                    .Select((txt, idx) =>
                    {
                        var firstChar = txt[0];
                        var lastChar = txt[txt.Length - 1];

                        return
                            (firstChar == ':' && lastChar == ':') ?
                                 Tuple.Create(idx, (TextAlignment?)TextAlignment.Center) :

                            (lastChar == ':') ?
                                Tuple.Create(idx, (TextAlignment?)TextAlignment.Right) :

                            (firstChar == ':') ?
                                Tuple.Create(idx, (TextAlignment?)TextAlignment.Left) :

                                Tuple.Create(idx, (TextAlignment?)null);
                    })
                    .Where(tpl => tpl.Item2.HasValue)
                    .ToDictionary(tpl => tpl.Item1, tpl => tpl.Item2.Value);

            var styleColumnCount = styleMt.Count;

            // apply cell style to header
            var headerColumnCount = 0;
            {
                var colOffset = 0;
                foreach (var cell in Header)
                {
                    // apply text align
                    if (styleMt.TryGetValue(colOffset, out var style))
                    {
                        cell.Horizontal = style;
                    }

                    colOffset += cell.ColSpan;
                }

                headerColumnCount = colOffset;
            }


            // apply cell style to header
            var detailsRowCount = new List<int>();
            var detailsRowCountSummary = 1;
            {
                var rowSpanLife = new Dictionary<int, MdSpan>();
                for (var rowIdx = 0; rowIdx < Details.Count; ++rowIdx)
                {
                    var row = Details[rowIdx];

                    var hasAnyCell = false;
                    var colOffset = 0;

                    var rowspansColOffset = rowSpanLife
                        .Select(ent => ent.Value.ColSpan)
                        .Sum();

                    if (rowspansColOffset < detailsRowCountSummary)
                    {
                        for (var colIdx = 0; colIdx < row.Count;)
                        {
                            var cell = row[colIdx];

                            // apply text align
                            if (!cell.Horizontal.HasValue
                                        && styleMt.TryGetValue(colOffset, out var style))
                            {
                                cell.Horizontal = style;
                            }

                            int colSpan;
                            if (rowSpanLife.TryGetValue(colOffset, out var span))
                            {
                                colSpan = span.ColSpan;
                            }
                            else
                            {
                                colSpan = cell.ColSpan;
                                hasAnyCell = true;

                                if (cell.RowSpan > 1)
                                {
                                    rowSpanLife[colOffset] =
                                        new MdSpan(cell.RowSpan, cell.ColSpan);
                                }

                                ++colIdx;
                            }

                            colOffset += colSpan;
                        }
                    }

                    colOffset += rowSpanLife
                        .Where(ent => ent.Key >= colOffset)
                        .Select(ent => ent.Value.ColSpan)
                        .Sum();

                    foreach (var spanEntry in rowSpanLife.ToArray())
                    {
                        if (--spanEntry.Value.Life == 0)
                        {
                            rowSpanLife.Remove(spanEntry.Key);
                        }
                    }

                    detailsRowCount.Add(colOffset);
                    detailsRowCountSummary = Math.Max(detailsRowCountSummary, colOffset);

                    if (!hasAnyCell)
                    {
                        Details.Insert(rowIdx, new List<MdTableCell>());
                    }
                }

                while (rowSpanLife.Count > 0)
                {
                    Details.Add(new List<MdTableCell>());

                    var colOffset = 0;

                    foreach (var spanEntry in rowSpanLife.ToArray())
                    {
                        colOffset += spanEntry.Value.ColSpan;

                        if (--spanEntry.Value.Life == 0)
                        {
                            rowSpanLife.Remove(spanEntry.Key);
                        }
                    }

                    detailsRowCount.Add(colOffset);
                }
            }

            ColCount = Math.Max(Math.Max(headerColumnCount, styleColumnCount), detailsRowCountSummary);
            RowCount = Details.Count;

            while (Header.Count < ColCount)
                Header.Add(new MdTableCell(null));

            for (var rowIdx = 0; rowIdx < Details.Count; ++rowIdx)
                for (var retry = detailsRowCount[rowIdx]; retry < ColCount; ++retry)
                    Details[rowIdx].Add(new MdTableCell(null));

            //while (Header.Count < ColCount)
            //    Header.Add(new MdTableCell(""));
            //
            //foreach (var row in Details)
            //    while (row.Count < ColCount)
            //        row.Add(new MdTableCell(""));
        }   //
    }

    class MdSpan
    {
        public int Life { set; get; }
        public int ColSpan { set; get; }

        public MdSpan(int l, int c)
        {
            Life = l;
            ColSpan = c;
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

            if (txt is null) return;

            txt = ParseFormatFrom(txt);

            var sb = new StringBuilder();
            for (var i = 0; i < txt.Length; ++i)
            {
                var c = txt[i];

                if (c == '\\')
                {
                    if (++i < txt.Length)
                    {
                        if (txt[i] == 'n')
                            sb.Append("  \n"); // \n => linebreak
                        else
                            sb.Append('\\').Append(txt[i]);
                    }
                    else
                        sb.Append('\\');
                }
                else
                    sb.Append(c);
            }
            Text = sb.ToString();
        }

        private string ParseFormatFrom(string txt)
        {
            int idx = txt.IndexOf('.');

            if (idx == -1)
            {
                return txt.Trim();
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
                            RowSpan = 1;
                            ColSpan = 1;
                            Horizontal = null;
                            Vertical = null;
                            return txt.Trim();
                    }
                }
                return txt.Substring(idx + 1).Trim();
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
