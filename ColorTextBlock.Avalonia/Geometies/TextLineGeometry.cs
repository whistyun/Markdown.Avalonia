using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace ColorTextBlock.Avalonia.Geometries
{
    internal class TextLineGeometry : TextGeometry
    {
        public string Text { get; }
        public Typeface Typeface { get; }
        public double FontSize { get; }
        public TextLine Line { get; private set; }
        public IBrush? LayoutForeground { get; private set; }

        internal TextLineGeometry(
            CInline owner,
            string text,
            TextLine tline,
            bool linebreak) :
            base(owner, tline.WidthIncludingTrailingWhitespace, tline.Height, tline.Baseline, owner.TextVerticalAlignment, linebreak)
        {
            Text = text;
            Typeface = owner.Typeface;
            FontSize = owner.FontSize;
            Line = tline;
            LayoutForeground = owner.Foreground;
        }

        internal TextLineGeometry(
                TextLineGeometry baseGeometry,
                bool linebreak) :
            base(baseGeometry.Owner,
                 baseGeometry.Width, baseGeometry.Height, baseGeometry.BaseHeight,
                 baseGeometry.TextVerticalAlignment,
                 linebreak)
        {
            Text = baseGeometry.Text;
            Typeface = baseGeometry.Typeface;
            FontSize = baseGeometry.FontSize;
            Line = baseGeometry.Line;
            LayoutForeground = baseGeometry.LayoutForeground;
        }

        public override void Render(DrawingContext ctx)
        {
            var foreground = TemporaryForeground ?? Foreground;
            var background = TemporaryBackground ?? Background;

            if (LayoutForeground != foreground)
            {
                LayoutForeground = foreground;
                var layout = new TextLayout(Text, Typeface, FontSize, foreground);
                Line = layout.TextLines.First();
            }

            if (background != null)
            {
                ctx.FillRectangle(background, new Rect(Left, Top, Width, Height));
            }

            Line.Draw(ctx, new Point(Left, Top));

            if (IsUnderline)
            {
                var ypos = Math.Round(Top + Height);
                ctx.DrawLine(new Pen(foreground, 2),
                    new Point(Left, ypos),
                    new Point(Left + Width, ypos));
            }

            if (IsStrikethrough)
            {
                var ypos = Math.Round(Top + Height / 2);
                ctx.DrawLine(new Pen(foreground, 2),
                    new Point(Left, ypos),
                    new Point(Left + Width, ypos));
            }
        }

        public override bool TryMoveNext(
            TextPointer current,
#if NETCOREAPP3_0_OR_GREATER
            [MaybeNullWhen(false)]
            out TextPointer? next
#else
            out TextPointer next
#endif
            )
        {
            if (!Object.ReferenceEquals(current.Last, this))
                throw new ArgumentException();

            var hit = new CharacterHit(current.InternalIndex);
            var nxt = Line.GetNextCaretCharacterHit(hit);

            if (hit == nxt)
            {
                next = null;
                return false;
            }

            var dst = Line.GetDistanceFromCharacterHit(nxt);
            next = new TextPointer(this, Line, nxt, dst + Left, Top, Height);
            return true;
        }

        public override bool TryMovePrev(
            TextPointer current,
#if NETCOREAPP3_0_OR_GREATER
            [MaybeNullWhen(false)]
            out TextPointer? prev
#else
            out TextPointer prev
#endif
            )
        {
            var hit = new CharacterHit(current.InternalIndex);
            var prv = Line.GetPreviousCaretCharacterHit(hit);

            if (hit == prv)
            {
                prev = null;
                return false;
            }

            var dst = Line.GetDistanceFromCharacterHit(prv);
            prev = new TextPointer(this, Line, prv, dst + Left, Top, Height);
            return true;
        }

        public override TextPointer CalcuatePointerFrom(double x, double y)
        {
            var hit = Line.GetCharacterHitFromDistance(x - Left);
            var dst = Line.GetDistanceFromCharacterHit(hit);

            return new TextPointer(this, Line, hit, dst + Left, Top, Height);
        }

        public override TextPointer GetBegin()
        {
            var hit = Line.GetCharacterHitFromDistance(0);
            var dst = Line.GetDistanceFromCharacterHit(hit);

            return new TextPointer(this, Line, hit, dst + Left, Top, Height);
        }

        public override TextPointer GetEnd()
        {
            var hit = Line.GetCharacterHitFromDistance(Double.MaxValue);
            var dst = Line.GetDistanceFromCharacterHit(hit);

            return new TextPointer(this, Line, hit, dst + Left, Top, Height);
        }
    }
}
