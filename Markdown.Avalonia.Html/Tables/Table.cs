using System;
using System.Collections.Generic;
using System.Linq;

namespace Markdown.Avalonia.Html.Tables
{
    class Table
    {
        public List<List<TableCell>> RowGroups { get; }
        public int ColCount { get; private set; }
        public int RowCount { get; private set; }

        public Table()
        {
            RowGroups = new();
        }

        public void Structure()
        {
            var colCntAtDetail = new List<int>();
            var maxColCntInDetails = 1;
            {
                var multiRowsAtColIdx = new Dictionary<int, MdSpan>();
                for (var rowIdx = 0; rowIdx < RowGroups.Count; ++rowIdx)
                {
                    List<TableCell> row = RowGroups[rowIdx];

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

                                var cell = (TableCell)row[colIdx];
                                cell.ColumnIndex = colOffset;

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
                                var cell = new TableCell();
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
                        RowGroups.Insert(rowIdx, new List<TableCell>());
                    }
                }

                // if any multirow is left, insert an empty row.
                while (multiRowsAtColIdx.Count > 0)
                {
                    var row = new List<TableCell>();
                    RowGroups.Add(row);

                    var colOffset = 0;

                    foreach (var spanEntry in multiRowsAtColIdx.OrderBy(tpl => tpl.Key))
                    {
                        while (colOffset < spanEntry.Key)
                        {
                            var cell = new TableCell();
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

            ColCount = maxColCntInDetails;
            RowCount = RowGroups.Count;

            // insert cell for the shortfall

            for (var rowIdx = 0; rowIdx < RowGroups.Count; ++rowIdx)
            {
                for (var retry = colCntAtDetail[rowIdx]; retry < ColCount; ++retry)
                {
                    var cell = new TableCell();
                    cell.ColumnIndex = retry;
                    RowGroups[rowIdx].Add(cell);
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
    }
}
