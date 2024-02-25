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
            CRun owner,
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

        public override TextPointer CalcuatePointerFrom(double x, double y)
        {
            var relX = x - Left;

            if (relX < 0) return GetBegin();
            if (relX >= Width) return GetEnd();

            var hit = Line.GetCharacterHitFromDistance(relX);
            var dst = Line.GetDistanceFromCharacterHit(hit);

            return new TextPointer((CRun)Owner, this, hit, dst, false);
        }
        public override TextPointer CalcuatePointerFrom(int index)
        {
            var hit = new CharacterHit(Line.FirstTextSourceIndex + index);
            var dst = Line.GetDistanceFromCharacterHit(hit);

            return new TextPointer((CRun)Owner, this, hit, dst, false);
        }

        public override TextPointer GetBegin()
        {
            var hit = Line.GetCharacterHitFromDistance(0);

            return new TextPointer((CRun)Owner, this, hit, false);
        }

        public override TextPointer GetEnd()
        {
            var hit = Line.GetCharacterHitFromDistance(Double.MaxValue);

            return new TextPointer((CRun)Owner, this, hit, Width, true);
        }
    }
}
