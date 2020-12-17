using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;

namespace ColorTextBlock.Avalonia.Geometries
{
    class TextGeometry : CGeometry
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

        private FormattedText Format;

        public TextGeometry(
            double width, double height, bool linebreak,
            CInline owner,
            string text, FormattedText format) : base(width, height, linebreak)
        {
            this.Text = text;
            this.Format = format;

            this.Owner = owner;
        }

        public static TextGeometry NewLine()
        {
            return new TextGeometry(
                0, 0, true,
                null,
                "", null);
        }

        public static TextGeometry NewLine(FormattedText format)
        {
            return new TextGeometry(
                0, format.Bounds.Height, true,
                null,
                "", format);
        }

        public override void Render(DrawingContext ctx)
        {
            if (Format is null && Width == 0 && Height == 0)
                return;

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
