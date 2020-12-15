using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColorTextBlock.Avalonia.Geometries
{
    class DecoratorGeometry : CGeometry
    {
        public CSpan Owner { get; }
        public CGeometry[] Targets { get; }
        public Border Decorate { get; }

        private Action _OnMouseEnter;
        private Action _OnMouseLeave;
        private Action _OnMousePressed;
        private Action _OnMouseReleased;
        private Action _OnClick;

        public override Action OnMouseEnter
        {
            get => () =>
            {
                _OnMouseEnter?.Invoke();
                foreach (var target in Targets)
                    target.OnMouseEnter?.Invoke();
            };
            set => _OnMouseEnter = value;
        }
        public override Action OnMouseLeave
        {
            get => () =>
            {
                _OnMouseLeave?.Invoke();
                foreach (var target in Targets)
                    target.OnMouseLeave?.Invoke();
            };
            set => _OnMouseLeave = value;
        }
        public override Action OnMousePressed
        {
            get => () =>
            {
                _OnMousePressed?.Invoke();
                foreach (var target in Targets)
                    target.OnMousePressed?.Invoke();
            };
            set => _OnMousePressed = value;
        }
        public override Action OnMouseReleased
        {
            get => () =>
            {
                _OnMouseReleased?.Invoke();
                foreach (var target in Targets)
                    target.OnMouseReleased?.Invoke();
            };
            set => _OnMouseReleased = value;
        }
        public override Action OnClick
        {
            get => () =>
            {
                _OnClick?.Invoke();
                foreach (var target in Targets)
                    target.OnClick?.Invoke();
            };
            set => _OnClick = value;
        }

        public DecoratorGeometry(
            CSpan owner,
            IEnumerable<CGeometry> targets,
            Border decorate) : this(owner, targets.ToArray(), decorate)
        { }

        public DecoratorGeometry(
            CSpan owner,
            CGeometry[] targets,
            Border decorate)
            : base(
                  targets.Sum(t => t.Width) + decorate.DesiredSize.Width,
                  targets.Max(t => t.Height) + decorate.DesiredSize.Height,
                  targets.Last().LineBreak)
        {
            this.Owner = owner;
            this.Targets = targets;
            this.Decorate = decorate;
        }

        public override void Render(DrawingContext ctx)
        {
            using (ctx.PushPreTransform(Matrix.CreateTranslation(Left, Top)))
            {
                Decorate.Background = Owner.Background;
                Decorate.Arrange(new Rect(0, 0, Width, Height));
                Decorate.Render(ctx);

            }

            var left = Left + Decorate.BorderThickness.Left + Decorate.Padding.Left;
            var top = Top + Height - Decorate.BorderThickness.Bottom + Decorate.Padding.Bottom;

            foreach (var target in Targets)
            {
                // I'm not sure it is correct.
                target.Left = left;
                target.Top = top - target.Height;
                target.Render(ctx);

                left += target.Width;
            }
        }
    }
}
