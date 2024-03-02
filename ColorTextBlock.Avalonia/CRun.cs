using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Media.TextFormatting.Unicode;
using Avalonia.Metadata;
using ColorTextBlock.Avalonia.Geometries;
using System;
using System.Collections.Generic;

namespace ColorTextBlock.Avalonia
{
    /// <summary>
    /// Expression of a text
    /// </summary>
    public class CRun : CInline
    {
        /// <summary>
        /// THe content of the eleemnt
        /// </summary>
        /// <seealso cref="Content"/>
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<CRun, string>(nameof(Text));

        /// <summary>
        /// THe content of the eleemnt
        /// </summary>
        [Content]
        public string Text
        {
            get { return GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        protected override IEnumerable<CGeometry> MeasureOverride(
            double entireWidth,
            double remainWidth)
        {
            if (String.IsNullOrEmpty(Text))
            {
                return Array.Empty<CGeometry>();
            }

            return PrivateMeasureOverride(entireWidth, remainWidth);
        }

        protected List<CGeometry> PrivateMeasureOverride(
            double entireWidth,
            double remainWidth)
        {
            var runProps = CreateTextRunProperties(Foreground);
            var paraProps = CreateTextParagraphProperties(runProps);
            var source = new SimpleTextSource(Text.AsMemory(), runProps);

            if (remainWidth == entireWidth)
            {
                return Create(entireWidth);
            }

            var firstLine = TextFormatter.Current.FormatLine(source, 0, double.PositiveInfinity, paraProps);
            if (firstLine is null)
            {
                return new List<CGeometry>();
            }

            if (firstLine.Width < remainWidth)
            {
                if (firstLine.Length == Text.Length)
                {
                    return new List<CGeometry>() { new TextLineGeometry(this, source, firstLine, false) };
                }

                return CreateLines(firstLine);
            }
            else
            {
                var firstLineSource = source.Subsource(firstLine.FirstTextSourceIndex, firstLine.Length);
                var firstLineRemain = TextFormatter.Current.FormatLine(firstLineSource, 0, remainWidth, paraProps)!;

                var breakPosEnum = new LineBreakEnumerator(Text.AsMemory().Slice(firstLine.FirstTextSourceIndex, firstLine.Length).Span);
                int breakPos = breakPosEnum.MoveNext(out var lnbrk) ? lnbrk.PositionWrap : int.MaxValue;


                if (breakPos < firstLineRemain.Length)
                {
                    // correct wrap

                    return CreateLines(firstLineRemain);
                }
                else
                {
                    // wrong wrap; first line word is too long

                    var list = Create(entireWidth);
                    list.Insert(0, new LineBreakMarkGeometry(this));
                    return list;
                }
            }

            List<CGeometry> CreateLines(TextLine firstLine)
            {
                var lines = new List<CGeometry>();

                TextLine prev = firstLine;

                var length = firstLine.Length;
                while (length < Text.Length)
                {
                    var line = TextFormatter.Current.FormatLine(source, length, entireWidth, paraProps, prev.TextLineBreak);
                    if (line is null)
                        break;

                    lines.Add(new TextLineGeometry(this, source, prev, true));
                    prev = line;

                    length += line.Length;
                }

                lines.Add(new TextLineGeometry(this, source, prev, false));

                return lines;
            }

            List<CGeometry> Create(double maxWidth)
            {
                var layout = new TextLayout(
                        source,
                        paraProps,
                        maxWidth: maxWidth);

                var rslt = new List<CGeometry>();

                for (int j = 0; j < layout.TextLines.Count; ++j)
                {
                    var line = layout.TextLines[j];
                    var linebreak = j != layout.TextLines.Count - 1;

                    rslt.Add(new TextLineGeometry(this, source, line, linebreak));
                }

                return rslt;
            }
        }


        internal TextParagraphProperties CreateTextParagraphProperties(TextRunProperties runProps)
            => new GenericTextParagraphProperties(
                        FlowDirection.LeftToRight,
                        TextAlignment.Left, true, false,
                        runProps,
                        TextWrapping.Wrap,
                        double.NaN,
                        0,
                        0);

        internal TextRunProperties CreateTextRunProperties(IBrush? foreground)
            => new GenericTextRunProperties(Typeface, FontSize, foregroundBrush: foreground);

        public override string AsString() => Text;
    }


    readonly struct SimpleTextSource : ITextSource
    {
        private readonly ReadOnlyMemory<char> _text;
        private readonly TextRunProperties _props;

        public TextRunProperties RunProperties => _props;

        public SimpleTextSource(ReadOnlyMemory<char> text, TextRunProperties props)
        {
            _text = text;
            _props = props;
        }

        public TextRun? GetTextRun(int textSourceIndex)
        {
            return new TextCharacters(_text.Slice(textSourceIndex), _props);
        }

        public SimpleTextSource Subsource(int start, int length)
            => new SimpleTextSource(_text.Slice(start, length), _props);

        public string Substring(int start, int length)
            => _text.Slice(start, length).ToString();

        public string Substring(int start)
            => _text.Slice(start).ToString();

        public SimpleTextSource ChangeForeground(IBrush? foreground)
        {
            var runProps = new GenericTextRunProperties(_props.Typeface, _props.FontRenderingEmSize, foregroundBrush: foreground);
            return new SimpleTextSource(_text, runProps);
        }

        public override string ToString() => _text.ToString();
    }
}