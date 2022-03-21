using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using System.Linq;

namespace ColorTextBlock.Avalonia.Geometries
{
    internal abstract class TextGeometry : CGeometry
    {
        private CInline Owner;

        private IBrush _TemporaryForeground;
        public IBrush TemporaryForeground
        {
            get => _TemporaryForeground;
            set => _TemporaryForeground = value;
        }

        private IBrush _TemporaryBackground;
        public IBrush TemporaryBackground
        {
            get => _TemporaryBackground;
            set => _TemporaryBackground = value;
        }

        public IBrush Foreground
        {
            get => Owner?.Foreground;
        }
        public IBrush Background
        {
            get => Owner?.Background;
        }
        public bool IsUnderline
        {
            get => Owner is null ? false : Owner.IsUnderline;
        }
        public bool IsStrikethrough
        {
            get => Owner is null ? false : Owner.IsStrikethrough;
        }

        internal TextGeometry(
            CInline owner,
            double width, double height, double lineHeight,
            TextVerticalAlignment alignment,
            bool linebreak) :
            base(width, height, lineHeight, alignment, linebreak)
        {
            Owner = owner;
        }
    }

    internal class LineBreakMarkGeometry : TextGeometry
    {
        internal LineBreakMarkGeometry(
            CInline owner,
            TextLayout layout) :
            base(owner, 0, layout.Bounds.Height, layout.Bounds.Height, TextVerticalAlignment.Base, true)
        {

        }
        internal LineBreakMarkGeometry(CInline owner) :
            base(owner, 0, 0, 0, TextVerticalAlignment.Base, true)
        {
        }

        public override void Render(DrawingContext ctx) { }
    }

    internal class SingleTextLayoutGeometry : TextGeometry
    {
        private string Text { get; }
        private TextLayoutCreator Creator { get; }
        private TextLayout Layout { set; get; }
        private IBrush LayoutForeground { set; get; }

        internal SingleTextLayoutGeometry(
            CInline owner,
            TextLayout layout,
            TextLayoutCreator creator,
            TextVerticalAlignment align,
            string text,
            bool linebreak) :
            base(owner, layout.Bounds.Width, layout.Bounds.Height, layout.Bounds.Height, align, linebreak)
        {
            Creator = creator;
            Layout = layout;
            LayoutForeground = owner.Foreground;
            Text = text;
        }

        public override void Render(DrawingContext ctx)
        {
            var foreground = TemporaryForeground ?? Foreground;
            var background = TemporaryBackground ?? Background;

            if (LayoutForeground != foreground)
            {
                LayoutForeground = foreground;
                Layout = Creator(Text, LayoutForeground);
            }

            if (background != null)
            {
                ctx.FillRectangle(background, new Rect(Left, Top, Width, Height));
            }

            Layout.Draw(ctx, new Point(Left, Top));

            var pen = new Pen(foreground);
            if (IsUnderline)
            {
                ctx.DrawLine(pen,
                    new Point(Left, Top + Height),
                    new Point(Left + Width, Top + Height));
            }

            if (IsStrikethrough)
            {
                ctx.DrawLine(pen,
                    new Point(Left, +Top + Height / 2),
                    new Point(Left + Width, Top + Height / 2));
            }
        }
    }

    internal delegate TextLayout TextLayoutCreator(string text, IBrush foreground);
}
