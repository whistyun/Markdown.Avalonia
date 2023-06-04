using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ColorTextBlock.Avalonia.Geometries
{
    public class DecoratorGeometry : CGeometry
    {
        public CSpan Owner { get; }
        public CGeometry[] Targets { get; }
        public Border Decorate { get; }

        private Action<Control>? _OnMouseEnter;
        private Action<Control>? _OnMouseLeave;
        private Action<Control>? _OnMousePressed;
        private Action<Control>? _OnMouseReleased;
        private Action<Control>? _OnClick;

        public override Action<Control>? OnMouseEnter
        {
            get => ctrl =>
            {
                _OnMouseEnter?.Invoke(ctrl);
                foreach (var target in Targets)
                    target.OnMouseEnter?.Invoke(ctrl);
            };
            set => _OnMouseEnter = value;
        }
        public override Action<Control>? OnMouseLeave
        {
            get => ctrl =>
            {
                _OnMouseLeave?.Invoke(ctrl);
                foreach (var target in Targets)
                    target.OnMouseLeave?.Invoke(ctrl);
            };
            set => _OnMouseLeave = value;
        }
        public override Action<Control>? OnMousePressed
        {
            get => ctrl =>
            {
                _OnMousePressed?.Invoke(ctrl);
                foreach (var target in Targets)
                    target.OnMousePressed?.Invoke(ctrl);
            };
            set => _OnMousePressed = value;
        }
        public override Action<Control>? OnMouseReleased
        {
            get => ctrl =>
            {
                _OnMouseReleased?.Invoke(ctrl);
                foreach (var target in Targets)
                    target.OnMouseReleased?.Invoke(ctrl);
            };
            set => _OnMouseReleased = value;
        }
        public override Action<Control>? OnClick
        {
            get => ctrl =>
            {
                _OnClick?.Invoke(ctrl);
                foreach (var target in Targets)
                    target.OnClick?.Invoke(ctrl);
            };
            set => _OnClick = value;
        }

        internal static DecoratorGeometry New(
            CSpan owner,
            IEnumerable<CGeometry> oneline,
            Border decorate)
        {
            double width = 0;
            double height = 0;

            double descentHeightTop = 0;
            double descentHeightBtm = 0;

            double baseHeight = 0;
            double baseHeight2 = 0;

            void Max(ref double v1, double v2) => v1 = Math.Max(v1, v2);

            foreach (var one in oneline)
            {
                width += one.Width;

                switch (one.TextVerticalAlignment)
                {
                    case TextVerticalAlignment.Base:
                        Max(ref baseHeight, one.BaseHeight);

                        Max(ref descentHeightTop, one.BaseHeight);
                        Max(ref descentHeightBtm, one.Height - one.BaseHeight);
                        break;

                    case TextVerticalAlignment.Top:
                        Max(ref baseHeight, one.BaseHeight);
                        Max(ref height, one.Height);
                        break;

                    case TextVerticalAlignment.Center:
                        Max(ref baseHeight, one.Height / 2);
                        Max(ref height, one.Height);
                        break;

                    case TextVerticalAlignment.Bottom:
                        Max(ref baseHeight2, one.BaseHeight);
                        Max(ref height, one.Height);
                        break;

                    default:
                        throw new InvalidOperationException("sorry library manager forget to modify.");
                }

            }

            Max(ref height, descentHeightTop + descentHeightBtm);

            baseHeight = baseHeight != 0 ? baseHeight : baseHeight2;

            return new DecoratorGeometry(
                width + decorate.DesiredSize.Width,
                height + decorate.DesiredSize.Height,
                baseHeight + decorate.Margin.Top + decorate.BorderThickness.Top + decorate.Padding.Top,
                owner,
                oneline.ToArray(),
                decorate);
        }

        private DecoratorGeometry(
            double w, double h, double lh,
            CSpan owner,
            CGeometry[] targets,
            Border decorate) : base(
                w, h, lh,
                owner.TextVerticalAlignment,
                targets[targets.Length - 1].LineBreak)
        {
            this.Owner = owner;
            this.Targets = targets;
            this.Decorate = decorate;
        }

        public override void Render(DrawingContext ctx)
        {
            using (ctx.PushTransform(Matrix.CreateTranslation(Left + Decorate.Margin.Left, Top + Decorate.Margin.Top)))
            {
                Decorate.Background = Owner.Background;
                Decorate.Arrange(new Rect(0, 0, Width, Height));
                Decorate.Render(ctx);

            }

            var left = Left + Decorate.BorderThickness.Left + Decorate.Padding.Left + Decorate.Margin.Left;

            var top = Top + Decorate.BorderThickness.Top + Decorate.Padding.Top + Decorate.Margin.Top;
            var btm = Top + Height - Decorate.BorderThickness.Bottom - Decorate.Padding.Bottom - Decorate.Margin.Bottom;

            foreach (var target in Targets)
            {
                target.Left = left;

                switch (target.TextVerticalAlignment)
                {
                    case TextVerticalAlignment.Top:
                        target.Top = top;
                        break;

                    case TextVerticalAlignment.Center:
                        target.Top = (top + btm - target.Height) / 2;
                        break;

                    case TextVerticalAlignment.Bottom:
                        target.Top = btm - target.Height;
                        break;

                    case TextVerticalAlignment.Base:
                        target.Top = Top + BaseHeight - target.BaseHeight;
                        break;
                }

                target.Render(ctx);

                left += target.Width;
            }
        }
    }
}
