using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Avalonia.Media;
using Avalonia.Layout;

namespace Markdown.Avalonia
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

            // column-idx vs text-alignment
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
                foreach (MdTableCell cell in Header)
                {
                    cell.ColumnIndex = colOffset;

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
            var colCntAtDetail = new List<int>();
            var maxColCntInDetails = 1;
            {
                var multiRowsAtColIdx = new Dictionary<int, MdSpan>();
                for (var rowIdx = 0; rowIdx < Details.Count; ++rowIdx)
                {
                    List<MdTableCell> row = Details[rowIdx];

                    var hasAnyCell = false;
                    var colOffset = 0;

                    var rowspansColOffset = multiRowsAtColIdx
                        .Select(ent => ent.Value.ColSpan)
                        .Sum();

                    /*
                     * In this row, is space exists to insert cell?
                     * 
                     * eg. has space
                     *    __________________________________
                     *    | 2x1 cell | 1x1 cell | 1x1 cell |
                     * -> |          |‾‾‾‾‾‾‾‾‾‾|‾‾‾‾‾‾‾‾‾‾|
                     *    ‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾
                     *    
                     * eg. has no space: multi-rows occupy all space in this row.
                     *    __________________________________
                     *    | 2x1 cell |      2x2 cell        |
                     * -> |          |                      |
                     *    ‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾
                     * 
                     */
                    if (rowspansColOffset < maxColCntInDetails)
                    {
                        int colIdx;
                        for (colIdx = 0; colIdx < row.Count;)
                        {
                            int colSpan;
                            if (multiRowsAtColIdx.TryGetValue(colOffset, out var span))
                            {
                                colSpan = span.ColSpan;
                            }
                            else
                            {
                                hasAnyCell = true;

                                var cell = row[colIdx];
                                cell.ColumnIndex = colOffset;

                                // apply text align
                                if (!cell.Horizontal.HasValue
                                            && styleMt.TryGetValue(colOffset, out var style))
                                {
                                    cell.Horizontal = style;
                                }

                                colSpan = cell.ColSpan;

                                if (cell.RowSpan > 1)
                                {
                                    multiRowsAtColIdx[colOffset] =
                                        new MdSpan(cell.RowSpan, cell.ColSpan);
                                }

                                ++colIdx;
                            }

                            colOffset += colSpan;
                        }

                        foreach (var left in multiRowsAtColIdx.Where(tpl => tpl.Key >= colOffset)
                                                              .OrderBy(tpl => tpl.Key))
                        {
                            while (colOffset < left.Key)
                            {
                                var cell = new MdTableCell(null);
                                cell.ColumnIndex = colOffset++;
                                row.Add(cell);

                            }
                            colOffset += left.Value.ColSpan;
                        }
                    }

                    colOffset += multiRowsAtColIdx
                        .Where(ent => ent.Key >= colOffset)
                        .Select(ent => ent.Value.ColSpan)
                        .Sum();

                    foreach (var spanEntry in multiRowsAtColIdx.ToArray())
                    {
                        if (--spanEntry.Value.Life == 0)
                        {
                            multiRowsAtColIdx.Remove(spanEntry.Key);
                        }
                    }

                    colCntAtDetail.Add(colOffset);
                    maxColCntInDetails = Math.Max(maxColCntInDetails, colOffset);

                    if (!hasAnyCell)
                    {
                        Details.Insert(rowIdx, new List<MdTableCell>());
                    }
                }

                // if any multirow is left, insert an empty row.
                while (multiRowsAtColIdx.Count > 0)
                {
                    var row = new List<MdTableCell>();
                    Details.Add(row);

                    var colOffset = 0;

                    foreach (var spanEntry in multiRowsAtColIdx.OrderBy(tpl => tpl.Key))
                    {
                        while (colOffset < spanEntry.Key)
                        {
                            var cell = new MdTableCell(null);
                            cell.ColumnIndex = colOffset++;
                            row.Add(cell);

                        }

                        colOffset += spanEntry.Value.ColSpan;

                        if (--spanEntry.Value.Life == 0)
                        {
                            multiRowsAtColIdx.Remove(spanEntry.Key);
                        }
                    }

                    colCntAtDetail.Add(colOffset);
                }
            }

            ColCount = Math.Max(Math.Max(headerColumnCount, styleColumnCount), maxColCntInDetails);
            RowCount = Details.Count;

            // insert cell for the shortfall

            for (var retry = Header.Sum(cell => cell.ColSpan); retry < ColCount; ++retry)
            {
                var cell = new MdTableCell(null);
                cell.ColumnIndex = retry;
                Header.Add(cell);
            }

            for (var rowIdx = 0; rowIdx < Details.Count; ++rowIdx)
            {
                for (var retry = colCntAtDetail[rowIdx]; retry < ColCount; ++retry)
                {
                    var cell = new MdTableCell(null);
                    cell.ColumnIndex = retry;
                    Details[rowIdx].Add(cell);
                }
            }
        }
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
        public int ColumnIndex { set; get; }

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
