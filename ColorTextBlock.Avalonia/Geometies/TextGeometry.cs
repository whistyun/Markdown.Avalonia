using Avalonia;
using Avalonia.Media;

namespace ColorTextBlock.Avalonia.Geometries
{
    public class TextGeometry : CGeometry
    {
        public string Text { get; }

        private IBrush _TemporaryForeground;
        public IBrush TemporaryForeground
        {
            get => _TemporaryForeground;
            set
            {
                _TemporaryForeground = value;
            }
        }

        private IBrush _TemporaryBackground;
        public IBrush TemporaryBackground
        {
            get => _TemporaryBackground;
            set
            {
                _TemporaryBackground = value;
            }
        }

        private CInline Owner;

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

        public bool IsLineBreakMarker => Format is null && Width == 0;

        private FormattedText Format;

        internal TextGeometry(
            double width, double height,
            TextVerticalAlignment alignment,
            bool linebreak,
            string text, FormattedText format) :
            base(width, height, height, alignment, linebreak)
        {
            this.Text = text;
            this.Format = format;
        }

        internal TextGeometry(
            double width, double height,
            bool linebreak,
            CInline owner,
            string text, FormattedText format) :
            base(width, height, height, owner.TextVerticalAlignment, linebreak)
        {
            this.Text = text;
            this.Format = format;
            this.Owner = owner;
        }

        internal static TextGeometry NewLine()
        {
            return new TextGeometry(
                0, 0,
                TextVerticalAlignment.Descent,
                true,
                "", null);
        }

        internal static TextGeometry NewLine(FormattedText format)
        {
            return new TextGeometry(
                0, format.Bounds.Height,
                TextVerticalAlignment.Descent,
                true,
                "", null);
        }

        public override void Render(DrawingContext ctx)
        {
            if (IsLineBreakMarker) return;

            var foreground = _TemporaryForeground ?? Foreground;
            var background = _TemporaryBackground ?? Background;
            if (background != null)
            {
                ctx.FillRectangle(background, new Rect(Left, Top, Width, Height));
            }

            var pen = new Pen(foreground);


            Format.Text = Text;
            ctx.DrawText(foreground, new Point(Left, Top), Format);

            if (IsUnderline)
            {
                ctx.DrawLine(pen,
                    new Point(Left, Top + Height),
                    new Point(Left + Width, Top + Height));
            }

            if (IsStrikethrough)
            {
                ctx.DrawLine(pen,
                    new Point(Left, Top + Height / 2),
                    new Point(Left + Width, Top + Height / 2));
            }
        }
    }
}
