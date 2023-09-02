using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;

namespace Markdown.Avalonia.Html.Tables
{
    class AutoScaleColumnDefinitions : ColumnDefinitions
    {

        private readonly int _columnCount;
        private Grid _grid;
        private bool _isAutoArrangeTried = false;
        private readonly ReadOnlyCollection<LengthInfo> _baseLength;

        public AutoScaleColumnDefinitions(IEnumerable<LengthInfo> baseLength, Grid owner)
        {
            _baseLength = baseLength.ToList().AsReadOnly();
            _columnCount = _baseLength.Count;
            _grid = owner;

            // To determine the required width for each column, use `Auto`
            foreach (var def in _baseLength)
            {
                if (def.Unit == LengthUnit.Pixel)
                    Add(new ColumnDefinition(def.Value, GridUnitType.Pixel));
                else if (def.Unit == LengthUnit.Percent)
                    Add(new ColumnDefinition(1, GridUnitType.Pixel));
                else
                    Add(new ColumnDefinition(1, GridUnitType.Auto));
            }

            _grid.LayoutUpdated += _grid_LayoutUpdated;
        }


        private void _grid_LayoutUpdated(object? sender, EventArgs e)
        {
            // If auto scale is disabled, use `1 *` for each column.
            // This is default value used for grid column width in Markdown.Avalonia.

            var isEnabled = global::Markdown.Avalonia.Controls.AutoScaleColumnDefinitions.GetIsEnabled(_grid);
            if (!isEnabled)
            {
                if (_grid.ColumnDefinitions.All(def => def.Width.IsStar && def.Width.Value == 1d))
                    return;

                _isAutoArrangeTried = false;

                Clear();

                for (var i = 0; i < _columnCount; ++i)
                    Add(new ColumnDefinition(1, GridUnitType.Star));

                ReSetGridChildren();
            }
            // If column width adjustement is already done, skip the following process.
            else
            {
                if (_isAutoArrangeTried)
                    return;

                _isAutoArrangeTried = true;

                AdjustColumn();
            }
        }

        private void AdjustColumn()
        {
            double entireWidth = _grid.Bounds.Width;
            double entireColumnWidth = entireWidth / _columnCount;

            double[] widths = MeasureColumnWidths();

            int[] scaleIndics = Enumerable.Range(0, _columnCount)
                                          .Where(i => _baseLength[i].IsPercent)
                                          .ToArray();

            int[] fixedIndics = Enumerable.Range(0, _columnCount)
                                          .Where(i => !_baseLength[i].IsPercent)
                                          .ToArray();

            foreach (var i in scaleIndics)
                widths[i] = entireWidth / widths.Length / 2;

            double scaleWidth = scaleIndics.Sum(i => widths[i]);

            double wealthy = entireWidth - scaleWidth;

            var minColWid = entireWidth / widths.Length / 2;
            var allowedColWid = wealthy / fixedIndics.Length;

            // sum of the shortfall width for each columns, excluding extra widths.
            var required = fixedIndics.Sum(i => Math.Max(widths[i] - allowedColWid, 0));

            if (required < 1d)
            {
                // equality arrange
                Clear();

                for (var i = 0; i < _columnCount; ++i)
                {
                    if (_baseLength[i].IsPercent)
                        Add(new ColumnDefinition(_baseLength[i].Value, GridUnitType.Star));
                    else
                        Add(new ColumnDefinition(widths[i], GridUnitType.Pixel));
                }

                ReSetGridChildren();
            }
            else
            {
                var units = _baseLength.Select(l => l.Unit).ToArray();

                if (minColWid < allowedColWid)
                {
                    CollectWidthFrom(
                        Enumerable.Range(0, _columnCount).ToArray(),
                        widths,
                        units,
                        minColWid);
                }
                else
                {
                    CollectWidthFrom(
                        fixedIndics,
                        widths,
                        units,
                        allowedColWid);
                }

                // equality arrange
                Clear();

                for (var i = 0; i < _columnCount; ++i)
                {
                    if (units[i] == LengthUnit.Percent)
                    {
                        Add(new ColumnDefinition(_baseLength[i].Value, GridUnitType.Star));
                    }
                    else
                    {
                        Add(new ColumnDefinition(widths[i], GridUnitType.Pixel));
                    }
                }

                ReSetGridChildren();
            }
        }

        void CollectWidthFrom(int[] indics, double[] widths, LengthUnit[] units, double colWid)
        {
            var required = indics.Sum(i => Math.Max(widths[i] - colWid, 0));

            // collect from wealthy columns
            var wealthy = indics.Sum(i => Math.Max(colWid - widths[i], 0));

            bool isShortabe;
            double collect = (isShortabe = required > wealthy) ? wealthy : required;

            foreach (var i in indics)
            {
                if (widths[i] < colWid)
                {
                    widths[i] = colWid - collect * (colWid - widths[i]) / wealthy;

                    if (isShortabe)
                        units[i] = LengthUnit.Pixel;
                }
                else
                {
                    widths[i] = colWid + collect * (widths[i] - colWid) / required;
                }
            }
        }

        /// <summary>
        /// Determines the maximum requred width for each column.
        /// </summary>
        private double[] MeasureColumnWidths()
        {
            var width = new double[_columnCount];

            foreach (var element in _grid.Children.OfType<Control>())
            {
                var colidx = Grid.GetColumn(element);
                var colspan = Grid.GetColumnSpan(element);
                if (colspan == 1)
                {
                    width[colidx] = Math.Max(width[colidx], element.Bounds.Width);
                }
                else
                {
                    // If the cell spans multiple columns, check whether there is sufficient width available for each column.

                    var requreWidth = element.Bounds.Width;
                    var consumedWid = Enumerable.Range(colidx, colspan).Sum(i => width[i]);

                    if (consumedWid >= requreWidth)
                        continue;

                    // If there is insufficient width, distribute the shortfall for each column.

                    var adding = Math.Ceiling((requreWidth - consumedWid) / colspan);

                    foreach (var i in Enumerable.Range(colidx, colspan))
                    {
                        width[i] += adding;
                    }
                }

                if (width.All(d => d != 0d)) break;
            }

            for (var i = 0; i < width.Length; ++i)
                width[i] = Math.Max(width[i], 1);

            return width;
        }

        private void ReSetGridChildren()
        {
            var backup = _grid.Children.ToArray();
            _grid.Children.Clear();
            _grid.Children.AddRange(backup);
        }
    }

    readonly struct LengthInfo
    {
        public double Value { get; }
        public LengthUnit Unit { get; }
        public bool IsPixcel => Unit == LengthUnit.Pixel;
        public bool IsPercent => Unit == LengthUnit.Percent;
        public bool IsAutoFit => Unit == LengthUnit.Auto;

        public LengthInfo(double v, LengthUnit u)
        {
            Value = v;
            Unit = u;
        }
    }

    enum LengthUnit
    {
        Pixel,
        Percent,
        Auto
    }
}
