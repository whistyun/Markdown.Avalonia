using Avalonia;
using Avalonia.Controls;
using ColorTextBlock.Avalonia.Geometies;
using ColorTextBlock.Avalonia.Geometries;
using System;
using System.Collections.Generic;
using System.Text;

namespace ColorTextBlock.Avalonia
{
    /// <summary>
    /// Places a control as an inline element.
    /// </summary>
    public class CInlineUIContainer : CInline
    {
        /// <summary>
        /// A displayed control
        /// </summary>
        public Control? Content { get; set; }
        internal DummyGeometryForControl? Indicator { get; private set; }

        public CInlineUIContainer(Control content)
        {
            Content = content;
        }

        protected override IEnumerable<CGeometry> MeasureOverride(double entireWidth, double remainWidth)
        {
            if (Content is null)
            {
                Indicator = null;
                return new CGeometry[0];
            }

            Content.Measure(new Size(remainWidth, Double.PositiveInfinity));

            if (Content.DesiredSize.Width > remainWidth)
            {
                Content.Measure(new Size(entireWidth, Double.PositiveInfinity));
                Indicator = new DummyGeometryForControl(Content, TextVerticalAlignment);
                return new CGeometry[] { new LineBreakMarkGeometry(this), Indicator };
            }
            else
            {
                Indicator = new DummyGeometryForControl(Content, TextVerticalAlignment);
                return new[] { Indicator };
            }
        }

        /// <inheritdoc/>
        public override string AsString() => String.Empty;
    }
}