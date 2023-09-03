using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Markdown.Avalonia.Html.Tables
{
    class Table
    {
        public List<LengthInfo> ColumnLengths { get; }
        public List<List<TableCell>> RowGroups { get; }
        public int ColCount { get; private set; }
        public int RowCount { get; private set; }

        public Table()
        {
            ColumnLengths = new();
            RowGroups = new();
        }

        public void Structure()
        {
            var colCntAtDetail = new List<int>();
            var maxColCntInDetails = Math.Max(1, ColumnLengths.Count);

            // The list of multi-row cells.
            // Key: Column index where the target cell is located.
            var multiRowsAtColIdx = new Dictionary<int, MdSpan>();

            for (var rowIdx = 0; rowIdx < RowGroups.Count; ++rowIdx)
            {
                List<TableCell> row = RowGroups[rowIdx];

                var colOffset = 0;

                // Setup ColspanIndex of each cells in the current row.
                for (int colIdx = 0; colIdx < row.Count;)
                {
                    int colSpan;
                    if (multiRowsAtColIdx.TryGetValue(colOffset, out var span))
                    {
                        colSpan = span.ColSpan;
                    }
                    else
                    {
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

                // Increments end of column index by the sum of the remaining multi-row cells.
                colOffset += multiRowsAtColIdx
                    .Where(ent => ent.Key >= colOffset)
                    .Sum(ent => ent.Value.ColSpan);

                // Removes multi-row cells,   複数行にまたがるセルの削除(必要なら)
                foreach (var spanEntry in multiRowsAtColIdx.ToArray())
                {
                    if (--spanEntry.Value.Life == 0)
                    {
                        multiRowsAtColIdx.Remove(spanEntry.Key);
                    }
                }

                colCntAtDetail.Add(colOffset);
                maxColCntInDetails = Math.Max(maxColCntInDetails, colOffset);
            }

            // If multi-row cells remain, insert empty rows.
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

            ColCount = maxColCntInDetails;
            RowCount = RowGroups.Count;

            // insert coldef for the shortfall

            var hasScaleWidCol = ColumnLengths.Any(c => c.Unit == LengthUnit.Percent);

            while (ColumnLengths.Count < ColCount)
            {
                if (hasScaleWidCol)
                    ColumnLengths.Add(new LengthInfo(1, LengthUnit.Auto));
                else
                    ColumnLengths.Add(new LengthInfo(1, LengthUnit.Percent));
            }

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
