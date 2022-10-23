using Avalonia;
using Avalonia.Controls;
using ColorTextBlock.Avalonia.Geometies;
using ColorTextBlock.Avalonia.Geometries;
using System;
using System.Collections.Generic;
using System.Text;

namespace ColorTextBlock.Avalonia
{
    public class CInlineUIContainer : CInline
    {
        internal Control Content { get; }
        internal DummyGeometryForControl? Indicator { get; private set; }

        public CInlineUIContainer(Control content)
        {
            Content = content;
        }

        protected override IEnumerable<CGeometry> MeasureOverride(double entireWidth, double remainWidth)
        {
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

        public override string AsString() => String.Empty;
    }
}