using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using System.Linq;

namespace ColorTextBlock.Avalonia.Geometries
{
    internal abstract class TextGeometry : CGeometry
    {
        internal CInline Owner { get; }

        private IBrush? _TemporaryForeground;
        public IBrush? TemporaryForeground
        {
            get => _TemporaryForeground;
            set => _TemporaryForeground = value;
        }

        private IBrush? _TemporaryBackground;
        public IBrush? TemporaryBackground
        {
            get => _TemporaryBackground;
            set => _TemporaryBackground = value;
        }

        public IBrush? Foreground
        {
            get => Owner?.Foreground;
        }
        public IBrush? Background
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
            double lineHeight) :
            base(owner, 0, lineHeight, lineHeight, TextVerticalAlignment.Base, true)
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
        private FormattedText Layout { set; get; }
        private IBrush? LayoutForeground { set; get; }

        internal SingleTextLayoutGeometry(
            CInline owner,
            FormattedText tline,
            TextVerticalAlignment align,
            string text,
            bool linebreak) :
            base(owner, tline.WidthIncludingTrailingWhitespace, tline.Height, tline.Height, align, linebreak)
        {
            Layout = tline;
            LayoutForeground = owner.Foreground;
            Text = text;
        }

        internal SingleTextLayoutGeometry(
                SingleTextLayoutGeometry baseGeometry,
                bool linebreak) :
            base(baseGeometry.Owner,
                 baseGeometry.Width, baseGeometry.Height, baseGeometry.Height,
                 baseGeometry.TextVerticalAlignment,
                 linebreak)
        {
            Layout = baseGeometry.Layout;
            LayoutForeground = baseGeometry.LayoutForeground;
            Text = baseGeometry.Text;
        }

        public override void Render(DrawingContext ctx)
        {
            var foreground = TemporaryForeground ?? Foreground;
            var background = TemporaryBackground ?? Background;

            if (LayoutForeground != foreground)
            {
                LayoutForeground = foreground;
                Layout.SetForegroundBrush(LayoutForeground, 0, Text.Length);
            }

            if (background != null)
            {
                ctx.FillRectangle(background, new Rect(Left, Top, Width, Height));
            }

            ctx.DrawText(Layout, new Point(Left, Top));

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
}
