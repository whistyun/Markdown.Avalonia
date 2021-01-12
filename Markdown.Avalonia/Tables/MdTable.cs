using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Markdown.Avalonia.Tables
{
    class MdTable : ITable
    {
        public List<ITableCell> Header { get; }
        public List<List<ITableCell>> Details { get; }
        public int ColCount { get; }
        public int RowCount { get; }

        public MdTable(
            string[] header,
            string[] styles,
            IList<string[]> details)
        {
            RowCount = details.Count;
            ColCount = Math.Max(header.Length, Math.Max(styles.Length, details.Max(dtl => dtl.Length)));

            // style
            List<TextAlignment> aligns = styles.Select(stl =>
            {
                char f = stl[0];
                char l = stl[stl.Length - 1];
                return (f == ':' && l == ':') ? TextAlignment.Center :
                        l == ':' ? TextAlignment.Right :
                        TextAlignment.Left;
            }).ToList();
            while (aligns.Count < ColCount) aligns.Add(TextAlignment.Left);

            List<ITableCell> MkCells(string[] line)
            {
                ITableCell MkCell(string txt, int idx)
                    => new MdTableCell(txt) { ColumnIndex = idx, Horizontal = aligns[idx] };

                var lst = line.Select(MkCell).ToList();
                while (lst.Count < ColCount) lst.Add(MkCell("", Header.Count));
                return lst;
            }

            // header
            Header = MkCells(header);

            // details
            Details = details.Select(MkCells).ToList();
        }
    }
}
