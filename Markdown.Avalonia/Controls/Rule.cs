using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using HAlign = Avalonia.Layout.HorizontalAlignment;
using VAlign = Avalonia.Layout.VerticalAlignment;

namespace Markdown.Avalonia.Controls
{
    public class Rule : UserControl
    {
        const double SingleLineWidth = 1;
        const double BoldLineWidth = 2;
        const double LineMargin = 1;

        public RuleType Type
        {
            get; set;
        }

        public Rule(RuleType ruleType)
        {
            this.Type = ruleType;
            this.HorizontalAlignment = HAlign.Stretch;
            this.VerticalAlignment = VAlign.Center;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            switch (Type)
            {
                case RuleType.Single:
                    return new Size(10, LineMargin * 2 + SingleLineWidth);

                case RuleType.TwoLines:
                    return new Size(10, LineMargin * 2 + SingleLineWidth * 3);

                case RuleType.Bold:
                    return new Size(10, LineMargin * 2 + BoldLineWidth);

                case RuleType.BoldWithSingle:
                    return new Size(10, LineMargin * 2 + SingleLineWidth * 2 + BoldLineWidth);

                default:
                    throw new InvalidOperationException();
            }
        }

        public override void Render(DrawingContext context)
        {
            var brush = new SolidColorBrush(Colors.Black);
            var single = new Pen(brush, SingleLineWidth);
            var bold = new Pen(brush, BoldLineWidth);

            var width = Bounds.Width;

            switch (Type)
            {
                case RuleType.Single:
                    context.DrawLine(
                        single,
                        new Point(0d, LineMargin + SingleLineWidth / 2),
                        new Point(width, LineMargin + SingleLineWidth / 2));
                    break;

                case RuleType.TwoLines:
                    context.DrawLine(
                        single,
                        new Point(0d, LineMargin + SingleLineWidth / 2),
                        new Point(width, LineMargin + SingleLineWidth / 2));

                    context.DrawLine(
                        single,
                        new Point(0d, LineMargin * 2 + SingleLineWidth * 3 / 2),
                        new Point(width, LineMargin * 2 + SingleLineWidth * 3 / 2));

                    break;

                case RuleType.Bold:
                    context.DrawLine(
                        bold,
                        new Point(0d, LineMargin + BoldLineWidth / 2),
                        new Point(width, LineMargin + BoldLineWidth / 2));

                    break;

                case RuleType.BoldWithSingle:
                    context.DrawLine(
                        bold,
                        new Point(0d, LineMargin + BoldLineWidth / 2),
                        new Point(width, LineMargin + BoldLineWidth / 2));

                    context.DrawLine(
                        single,
                        new Point(0d, LineMargin * 2 + BoldLineWidth + SingleLineWidth / 2),
                        new Point(width, LineMargin * 2 + BoldLineWidth + SingleLineWidth / 2));

                    break;


                default:
                    throw new InvalidOperationException();
            }
        }

    }

    public enum RuleType
    {
        Single,
        TwoLines,
        Bold,
        BoldWithSingle,
    }
}
