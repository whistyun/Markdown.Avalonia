using Avalonia;
using Avalonia.Controls;
using Avalonia.Remote.Protocol.Viewport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markdown.Avalonia.Controls
{
    public class AutoScaleColumnDefinitions : ColumnDefinitions
    {
        public static readonly AttachedProperty<bool> IsEnabledProperty =
            AvaloniaProperty.RegisterAttached<AutoScaleColumnDefinitions, Grid, bool>("IsEnabled", false);

        public static bool GetIsEnabled(Control control)
            => control.GetValue(IsEnabledProperty);

        public static void SetIsEnabled(Control control, bool value)
            => control.SetValue(IsEnabledProperty, value);


        private int _columnCount;
        private Grid _grid;
        private bool _isAutoArrangeTried = false;

        public AutoScaleColumnDefinitions(int columnCount, Grid owner)
        {
            _columnCount = columnCount;
            _grid = owner;

            for (var i = 0; i < _columnCount; ++i)
                Add(new ColumnDefinition(1, GridUnitType.Auto));

            _grid.LayoutUpdated += _grid_LayoutUpdated;
        }

        private void _grid_LayoutUpdated(object? sender, EventArgs e)
        {
            // If auto scale is disabled, use `1 *` for each column.
            // This is default value used for grid column width in Markdown.Avalonia.
            if (!GetIsEnabled(_grid))
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
            if (_isAutoArrangeTried)
                return;

            _isAutoArrangeTried = true;


            /// Determines the maximum requred width for each column.

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
                    var requreWidth = element.Bounds.Width;
                    var consumedWid = Enumerable.Range(colidx, colspan).Sum(i => width[i]);

                    if (consumedWid >= requreWidth)
                        continue;

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

            // arrange width at every columns

            var allowedWidth = _grid.Bounds.Width;
            var allowedColWid = allowedWidth / width.Length;

            Clear();

            var required = width.Sum(w => Math.Max(w - allowedColWid, 0));

            if (required < 1d)
            {
                // equality arrange
                for (var i = 0; i < _columnCount; ++i)
                    Add(new ColumnDefinition(1, GridUnitType.Star));
            }
            else
            {
                // collect from wealthy columns
                var wealthy = width.Sum(w => Math.Max(allowedColWid - w, 0));

                var hold = Enumerable.Repeat(allowedColWid, width.Length).ToArray();
                var unit = Enumerable.Repeat(GridUnitType.Star, width.Length).ToArray();

                bool isShortabe;
                double collect = (isShortabe = required > wealthy) ? wealthy : required;

                for (var i = 0; i < width.Length; ++i)
                {
                    if (width[i] < allowedColWid)
                    {
                        hold[i] -= collect * (allowedColWid - width[i]) / wealthy;

                        if (isShortabe)
                            unit[i] = GridUnitType.Pixel;
                    }
                    else
                    {
                        hold[i] += collect * (width[i] - allowedColWid) / required;
                    }
                }

                for (var i = 0; i < hold.Length; ++i)
                    Add(new ColumnDefinition(hold[i], unit[i]));
            }

            ReSetGridChildren();
        }

        private void ReSetGridChildren()
        {
            var backup = _grid.Children.ToArray();
            _grid.Children.Clear();
            _grid.Children.AddRange(backup);
        }
    }
}
