using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColorDocument.Avalonia.DocumentElements
{
    public class TableBlockElement : DocumentElement
    {
        private Lazy<Border> _table;
        private TableCellElement[][] _head;
        private TableCellElement[][] _body;
        private TableCellElement[][] _foot;
        private bool _autoAdjust;
        private EnumerableEx<TableCellElement> _all;
        private SelectionList? _prevSelection;

        public override Control Control => _table.Value;
        public override IEnumerable<DocumentElement> Children => _all;

        public TableBlockElement(
            IEnumerable<IEnumerable<TableCellElement>> thead,
            IEnumerable<IEnumerable<TableCellElement>> tbody,
            IEnumerable<IEnumerable<TableCellElement>> tfoot,
            bool autoAdjust) :
                this(
                    thead.Select(ln => ln.ToArray()).ToArray(),
                    tbody.Select(ln => ln.ToArray()).ToArray(),
                    tfoot.Select(ln => ln.ToArray()).ToArray(),
                    autoAdjust)
        { }

        public TableBlockElement(
            TableCellElement[][] thead,
            TableCellElement[][] tbody,
            TableCellElement[][] tfoot,
            bool autoAdjust)
        {
            _head = thead;
            _body = tbody;
            _foot = tfoot;

            _all = _head.SelectMany(l => l)
                        .Concat(_body.SelectMany(l => l))
                        .Concat(_foot.SelectMany(l => l))
                        .ToEnumerable();

            _autoAdjust = autoAdjust;

            _table = new Lazy<Border>(CreateTable);
        }

        public override void Select(Point from, Point to)
        {
            var selection = SelectionUtil.SelectGrid(Control, _all, from, to);

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
            foreach (var child in _all)
                child.UnSelect();
        }

        private Border CreateTable()
        {
            var rowInfs = new List<RowInf>();
            CreateRows(rowInfs, _head, ClassNames.TableHeaderClass);
            CreateRows(rowInfs, _body);
            CreateRows(rowInfs, _foot, ClassNames.TableFooterClass);

            int maxColCnt = rowInfs.Max(r => r.ColumnCount);

            if (_autoAdjust)
            {
                for (int i = 0; i < rowInfs.Count; ++i)
                {
                    var rowInf = rowInfs[i];
                    while (rowInf.ColumnCount < maxColCnt)
                    {
                        var cellCtrl = new Border();
                        Grid.SetRow(cellCtrl, i);
                        Grid.SetColumn(cellCtrl, rowInf.ColumnCount);
                        rowInf.Cells.Add(cellCtrl);
                        rowInf.ColumnCount++;
                    }
                }
            }

            var grid = new Grid();
            grid.Classes.Add(ClassNames.TableClass);

            grid.RowDefinitions.AddRange(Enumerable.Range(0, rowInfs.Count).Select(_ => new RowDefinition()));
            grid.ColumnDefinitions.AddRange(Enumerable.Range(0, maxColCnt).Select(_ => new ColumnDefinition()));

            foreach (var rowInf in rowInfs)
            {
                foreach (var cell in rowInf.Cells)
                    cell.Classes.AddRange(rowInf.Classes);

                grid.Children.AddRange(rowInf.Cells);
            }

            var border = new Border();
            border.Classes.Add(ClassNames.TableClass);
            border.Child = grid;

            //var grid = new Grid();
            //grid.Classes.Add(ClassNames.TableClass);
            //var border = new Border();
            //border.Classes.Add(ClassNames.TableClass);
            //border.Child = grid;
            //int rowOffset = 0;
            //
            //int hRowOffset = rowOffset;
            //List<RowInfo> hInfs = SetupRow(grid, _head, ref rowOffset, ClassNames.TableHeaderClass);
            //int bRowOffset = rowOffset;
            //List<RowInfo> bInfs = SetupRow(grid, _body, ref rowOffset);
            //int fRowOffset = rowOffset;
            //List<RowInfo> fInfs = SetupRow(grid, _foot, ref rowOffset, ClassNames.TableFooterClass);
            //
            //int colCnt = hInfs.Concat(bInfs).Concat(fInfs).Max(i => i.ColumnCount);
            //
            //if (_autoAdjust)
            //{
            //    AdjustRow(grid, hInfs, hRowOffset, colCnt);
            //    AdjustRow(grid, bInfs, bRowOffset, colCnt);
            //    AdjustRow(grid, fInfs, fRowOffset, colCnt);
            //}
            //
            //foreach (var _ in Enumerable.Range(0, colCnt))
            //{
            //    grid.ColumnDefinitions.Add(new ColumnDefinition());
            //}

            return border;
        }

        private static List<RowInfo> SetupRow(
            Grid grid,
            TableCellElement[][] rows,
            ref int gridRowIdx,
            string? classNm = null)
        {
            // The list of multi-row cells.
            // Key: Column index where the target cell is located.
            var multiRowsAtColIdx = new Dictionary<int, MdSpan>();

            var rowInfs = new List<RowInfo>();
            var maxColCount = 0;

            int startRowInSection = gridRowIdx;
            for (var i = 0; i < rows.Length; ++gridRowIdx)
            {
                var row = rows[i];
                grid.RowDefinitions.Add(new RowDefinition());

                // Set up classes for cell in this row.
                string[] classes;
                if (classNm is not null)
                    classes = new[] { classNm };
                else
                {
                    var rowIdxInSection = gridRowIdx - startRowInSection;

                    if (rowIdxInSection == 0)
                    {
                        if (i == rows.Length - 1)
                            classes = new[] { ClassNames.TableRowOddClass, ClassNames.TableFirstRowClass, ClassNames.TableLastRowClass };
                        else
                            classes = new[] { ClassNames.TableRowOddClass, ClassNames.TableFirstRowClass };
                    }
                    else
                    {
                        var oddOrEven = rowIdxInSection % 2 == 0 ? ClassNames.TableRowOddClass : ClassNames.TableRowEvenClass;

                        if (i == rows.Length - 1)
                            classes = new[] { oddOrEven, ClassNames.TableLastRowClass };
                        else
                            classes = new[] { oddOrEven };
                    }
                }


                var rowspansColOffset = multiRowsAtColIdx.Sum(e => e.Value.ColSpan);

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
                if (rowspansColOffset == 0 || rowspansColOffset < maxColCount)
                {
                    int colIdx = 0;
                    foreach (var cell in row)
                    {
                        while (multiRowsAtColIdx.TryGetValue(colIdx, out var span))
                        {
                            colIdx += span.ColSpan;
                        }

                        var cellCtrl = cell.Control;
                        cell.Row = gridRowIdx;
                        cell.Column = colIdx;
                        Grid.SetRow(cellCtrl, gridRowIdx);
                        Grid.SetColumn(cellCtrl, colIdx);
                        if (cell.RowSpan > 1) Grid.SetRowSpan(cellCtrl, cell.RowSpan);
                        if (cell.ColSpan > 1) Grid.SetColumnSpan(cellCtrl, cell.ColSpan);
                        cellCtrl.Classes.AddRange(classes);
                        grid.Children.Add(cellCtrl);


                        if (cell.RowSpan > 1)
                        {
                            multiRowsAtColIdx[colIdx] =
                                new MdSpan(cell.RowSpan, cell.ColSpan);
                        }

                        colIdx += cell.ColSpan;
                    }

                    rowInfs.Add(new RowInfo(classes, colIdx));

                    if (maxColCount < colIdx) maxColCount = colIdx;

                    ++i;
                }
                else
                {
                    rowInfs.Add(new RowInfo(classes, rowspansColOffset));
                }

                // Removes multi-row cells,   複数行にまたがるセルの削除(必要なら)
                foreach (var spanEntry in multiRowsAtColIdx.ToArray())
                {
                    if (--spanEntry.Value.Life == 0)
                    {
                        multiRowsAtColIdx.Remove(spanEntry.Key);
                    }
                }
            }

            // if any multirow is left, insert an empty row.
            while (multiRowsAtColIdx.Count > 0)
            {
                grid.RowDefinitions.Add(new RowDefinition());

                var colOffset = 0;

                foreach (var spanEntry in multiRowsAtColIdx.OrderBy(tpl => tpl.Key))
                {
                    while (colOffset < spanEntry.Key)
                    {
                        var cellCtrl = new Border();
                        Grid.SetRow(cellCtrl, gridRowIdx);
                        Grid.SetColumn(cellCtrl, colOffset);
                        grid.Children.Add(cellCtrl);
                        colOffset++;

                    }
                    colOffset += spanEntry.Value.ColSpan;


                    if (--spanEntry.Value.Life == 0)
                    {
                        multiRowsAtColIdx.Remove(spanEntry.Key);
                    }
                }

                rowInfs.Add(new RowInfo(Array.Empty<string>(), colOffset));
                gridRowIdx++;
            }

            return rowInfs;
        }

        private static void AdjustRow(Grid grid, List<RowInfo> rowInfs, int rowOffset, int colCnt)
        {
            for (var rowIdx = 0; rowIdx < rowInfs.Count; ++rowIdx)
            {
                var rowInf = rowInfs[rowIdx];
                for (var colIdx = rowInf.ColumnCount; colIdx < colCnt; ++colIdx)
                {
                    var cellCtrl = new Border();
                    Grid.SetRow(cellCtrl, rowIdx + rowOffset);
                    Grid.SetColumn(cellCtrl, colIdx);
                    cellCtrl.Classes.AddRange(rowInf.Classes);
                    grid.Children.Insert(SearchInsPos(grid.Children, cellCtrl), cellCtrl);
                }
            }

            int SearchInsPos(IList<Control> list, Control tgt)
            {
                int min = 0, max = list.Count;

                var tgtRow = Grid.GetRow(tgt);
                var tgtCol = Grid.GetColumn(tgt);
                int mid = 0;

                while (min < max)
                {
                    mid = (min + max) / 2;

                    var ctrl = list[mid];
                    var ctrlRow = Grid.GetRow(ctrl);
                    var ctrlCol = Grid.GetColumn(ctrl);

                    if (tgtRow < ctrlRow || (tgtRow == ctrlRow && tgtCol < ctrlCol))
                    {
                        max = mid - 1;
                    }
                    else if (tgtRow > ctrlRow || (tgtRow == ctrlRow && tgtCol > ctrlCol))
                    {
                        min = mid + 1;
                    }
                    else break;
                }

                for (var i = Math.Min(Math.Min(Math.Min(min, max), mid), list.Count); i < list.Count; ++i)
                {
                    var ctrl = list[i];
                    var ctrlRow = Grid.GetRow(ctrl);
                    var ctrlCol = Grid.GetColumn(ctrl);

                    if (tgtRow < ctrlRow || (tgtRow == ctrlRow && tgtCol < ctrlCol))
                    {
                        return i;
                    }
                }

                return list.Count;
            }
        }


        private void CreateRows(List<RowInf> rowInfs, TableCellElement[][] rows, string? classNm = null)
        {
            // The list of multi-row cells.
            // Key: Column index where the target cell is located.
            var multiRowsAtColIdx = new Dictionary<int, MdSpan>();

            var maxColCount = 0;
            int detailsRowIdx = 0;
            for (int i = 0; i < rows.Length;)
            {
                var rinf = new RowInf();
                SetupClass(rinf, detailsRowIdx++, classNm);


                var rowspansColOffset = multiRowsAtColIdx.Sum(e => e.Value.ColSpan);
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
                if (rowspansColOffset == 0 || rowspansColOffset < maxColCount)
                {
                    int colIdx = 0;
                    foreach (var cell in rows[i])
                    {
                        while (multiRowsAtColIdx.TryGetValue(colIdx, out var span))
                        {
                            colIdx += span.ColSpan;
                        }

                        var cellCtrl = cell.Control;
                        cell.Row = rowInfs.Count;
                        cell.Column = colIdx;
                        Grid.SetRow(cellCtrl, rowInfs.Count);
                        Grid.SetColumn(cellCtrl, colIdx);
                        if (cell.RowSpan > 1) Grid.SetRowSpan(cellCtrl, cell.RowSpan);
                        if (cell.ColSpan > 1) Grid.SetColumnSpan(cellCtrl, cell.ColSpan);
                        rinf.Cells.Add(cellCtrl);


                        if (cell.RowSpan > 1)
                        {
                            multiRowsAtColIdx[colIdx] =
                                new MdSpan(cell.RowSpan, cell.ColSpan);
                        }

                        colIdx += cell.ColSpan;
                    }


                    foreach (var left in multiRowsAtColIdx.Where(tpl => tpl.Key >= colIdx)
                                                          .OrderBy(tpl => tpl.Key))
                    {
                        while (colIdx < left.Key)
                        {
                            var cellCtrl = new Border();
                            Grid.SetRow(cellCtrl, rowInfs.Count);
                            Grid.SetColumn(cellCtrl, colIdx);
                            rinf.Cells.Add(cellCtrl);

                            ++colIdx;
                        }
                        colIdx += left.Value.ColSpan;
                    }


                    rinf.ColumnCount = colIdx;
                    if (maxColCount < colIdx) maxColCount = colIdx;


                    ++i;
                }
                else
                {
                    rinf.ColumnCount = rowspansColOffset;
                }

                rowInfs.Add(rinf);


                // Removes multi-row cells,   複数行にまたがるセルの削除(必要なら)
                foreach (var spanEntry in multiRowsAtColIdx.ToArray())
                {
                    if (--spanEntry.Value.Life == 0)
                    {
                        multiRowsAtColIdx.Remove(spanEntry.Key);
                    }
                }
            }


            // if any multirow is left, insert an empty row.
            while (multiRowsAtColIdx.Count > 0)
            {
                var rinf = new RowInf();
                SetupClass(rinf, detailsRowIdx++, classNm);

                var colIdx = 0;
                foreach (var spanEntry in multiRowsAtColIdx.OrderBy(tpl => tpl.Key))
                {
                    while (colIdx < spanEntry.Key)
                    {
                        var cellCtrl = new Border();
                        Grid.SetRow(cellCtrl, rowInfs.Count);
                        Grid.SetColumn(cellCtrl, colIdx);
                        rinf.Cells.Add(cellCtrl);
                        colIdx++;
                    }
                    colIdx += spanEntry.Value.ColSpan;

                    if (--spanEntry.Value.Life == 0)
                    {
                        multiRowsAtColIdx.Remove(spanEntry.Key);
                    }
                }
                rinf.ColumnCount = colIdx;
                rowInfs.Add(rinf);
            }


            if (classNm is null)
            {
                rowInfs.Last().Classes.Add(ClassNames.TableLastRowClass);
            }


            static void SetupClass(RowInf rinf, int rowIndex, string? classNm)
            {
                if (classNm is not null)
                    rinf.Classes.Add(classNm);
                else if (rowIndex == 0)
                    rinf.Classes.AddRange(new[] { ClassNames.TableRowOddClass, ClassNames.TableFirstRowClass });
                else if (rowIndex % 2 == 0)
                    rinf.Classes.Add(ClassNames.TableRowOddClass);
                else
                    rinf.Classes.Add(ClassNames.TableRowEvenClass);
            }
        }

        public override void ConstructSelectedText(StringBuilder builder)
        {
            if (_prevSelection is null)
                return;

            string[,] cellTxt = new string[
                _all.Max(c => c.Row + c.RowSpan),
                _all.Max(c => c.Column + c.ColSpan)
            ];

            foreach (var para in _prevSelection.Cast<TableCellElement>())
            {
                cellTxt[para.Row, para.Column] = para.GetSelectedText().TrimEnd().Replace("\r\n", "\r").Replace('\n', '\r');
            }

            for (int i = 0; i < cellTxt.GetLength(0); i++)
            {
                var preLen = builder.Length;

                for (int j = 0; j < cellTxt.GetLength(1); j++)
                {
                    builder.Append(cellTxt[i, j] ?? "");
                    builder.Append("\t");
                }

                if (builder.Length - preLen == 0)
                    continue;

                if (builder[builder.Length - 1] != '\n')
                    builder.Append('\n');
            }
        }

        class MdSpan
        {
            public int Life { get; set; }
            public int ColSpan { get; }

            public MdSpan(int l, int c)
            {
                Life = l;
                ColSpan = c;
            }
        }


        class RowInf
        {
            public List<string> Classes { get; } = new List<string>(5);
            public List<Control> Cells { get; } = new List<Control>();
            public int ColumnCount;
        }

        class RowInfo
        {
            public string[] Classes { get; }
            public int ColumnCount { get; }

            public RowInfo(string[] classes, int colCount)
            {
                Classes = classes;
                ColumnCount = colCount;
            }
        }
    }
}
