using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace ColorTextBlock.Avalonia.Geometries
{
    class DecolatedGeometry : CGeometry
    {
        public CSpan Owner { get; }
        public CGeometry Target { get; }
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
                Target.OnMouseEnter?.Invoke();
            };
            set => _OnMouseEnter = value;
        }
        public override Action OnMouseLeave
        {
            get => () =>
            {
                _OnMouseLeave?.Invoke();
                Target.OnMouseLeave?.Invoke();
            };
            set => _OnMouseLeave = value;
        }
        public override Action OnMousePressed
        {
            get => () =>
            {
                _OnMousePressed?.Invoke();
                Target.OnMousePressed?.Invoke();
            };
            set => _OnMousePressed = value;
        }
        public override Action OnMouseReleased
        {
            get => () =>
            {
                _OnMouseReleased?.Invoke();
                Target.OnMouseReleased?.Invoke();
            };
            set => _OnMouseReleased = value;
        }
        public override Action OnClick
        {
            get => () =>
            {
                _OnClick?.Invoke();
                Target.OnClick?.Invoke();
            };
            set => _OnClick = value;
        }

        public DecolatedGeometry(
            CSpan owner,
            CGeometry target,
            Border decorate)
            : base(
                  target.Width + decorate.DesiredSize.Width,
                  target.Height + decorate.DesiredSize.Height,
                  target.LineBreak)
        {
            this.Owner = owner;
            this.Target = target;
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

            // I'm not sure it is correct.
            Target.Left = Left + Decorate.BorderThickness.Left + Decorate.Padding.Left;
            Target.Top = Top + Decorate.BorderThickness.Left + Decorate.Padding.Left;
            Target.Render(ctx);
        }
    }
}
