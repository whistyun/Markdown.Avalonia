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
        const double _SingleLineWidth = 1;
        const double _BoldLineWidth = 2;
        const double _LineMargin = 1;

        public static readonly StyledProperty<double> SingleLineWidthProperty =
            AvaloniaProperty.Register<Rule, double>(nameof(SingleLineWidth), defaultValue: _SingleLineWidth);

        public static readonly StyledProperty<double> BoldLineWidthProperty =
            AvaloniaProperty.Register<Rule, double>(nameof(SingleLineWidth), defaultValue: _BoldLineWidth);

        public static readonly StyledProperty<double> LineMarginProperty =
            AvaloniaProperty.Register<Rule, double>(nameof(LineMargin), defaultValue: _LineMargin);

        static Rule()
        {
            AffectsRender<Rule>(
                BackgroundProperty,
                ForegroundProperty);

            AffectsMeasure<Rule>(
                SingleLineWidthProperty,
                BoldLineWidthProperty,
                LineMarginProperty);
        }


        public double SingleLineWidth
        {
            get { return GetValue(SingleLineWidthProperty); }
            set { SetValue(SingleLineWidthProperty, value); }
        }

        public double BoldLineWidth
        {
            get { return GetValue(BoldLineWidthProperty); }
            set { SetValue(BoldLineWidthProperty, value); }
        }

        public double LineMargin
        {
            get { return GetValue(LineMarginProperty); }
            set { SetValue(LineMarginProperty, value); }
        }


        public RuleType Type
        {
            get; set;
        }

        public Rule(RuleType ruleType)
        {
            this.Type = ruleType;
            this.HorizontalAlignment = HAlign.Stretch;
            this.VerticalAlignment = VAlign.Center;

            var cls = Enum.GetName(typeof(RuleType), ruleType);
            if (cls is null)
                throw new ArgumentException(nameof(ruleType));

            this.Classes.Add(cls);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return Type switch
            {
                RuleType.Single
                    => new Size(10, LineMargin * 2 + SingleLineWidth),

                RuleType.TwoLines
                    => new Size(10, LineMargin * 2 + SingleLineWidth * 3),

                RuleType.Bold
                    => new Size(10, LineMargin * 2 + BoldLineWidth),

                RuleType.BoldWithSingle
                    => new Size(10, LineMargin * 2 + SingleLineWidth * 2 + BoldLineWidth),

                _ => throw new InvalidOperationException(),
            };
        }

        public override void Render(DrawingContext context)
        {
            var brush = Foreground;
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
