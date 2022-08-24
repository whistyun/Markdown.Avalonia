using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Media.TextFormatting.Unicode;
using Avalonia.Metadata;
using ColorTextBlock.Avalonia.Geometries;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace ColorTextBlock.Avalonia
{
    public class CRun : CInline
    {
        private static readonly Regex Sep = new("\r\n|\r|\n", RegexOptions.Compiled);

        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<CRun, string>(nameof(Text));

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
                yield break;
            }

            var typeface = new Typeface(FontFamily, FontStyle, FontWeight);

            double remain = remainWidth;
            foreach (var line in Sep.Split(Text))
            {
                if (line.Length == 0)
                {
                    yield return new LineBreakMarkGeometry(this);
                }

                foreach (var geometry in MeasureOverride(line, typeface, FontSize, Foreground, entireWidth, remain))
                {
                    yield return geometry;
                }
            }
        }



        public IEnumerable<CGeometry> MeasureOverride(
            string text,
            Typeface typeface,
            double fontSize,
            IBrush? foreground,
            double entireWidth,
            double remainWidth)
        {
#if DEBUG
            if (text.Contains("\r") || text.Contains("\n"))
                throw new InvalidOperationException("text contains linebreak!");
#endif
            var ftext = CreateFormattedText(text);

            if (ftext.Width < remainWidth)
            {
                return new[] { CreateGeometry(text, ftext, false) };
            }
            else
            {
                var list = new List<CGeometry>();

                using var chipList = SplitBreakPoint(text).GetEnumerator();
                chipList.MoveNext();

                var chip = chipList.Current;
                var fmtChip = CreateFormattedText(chip);

                if (fmtChip.Width > remainWidth
                    && remainWidth != entireWidth)
                {
                    list.Add(new LineBreakMarkGeometry(this));
                }

                var lineIndex = 0;
                var lineLength = 0;
                var lineWidth = 0d;

                for (; ; )
                {
                    var constraint = list.Count == 0 ? remainWidth : entireWidth;

                    if (lineWidth + fmtChip.WidthIncludingTrailingWhitespace <= constraint)
                    {
                        lineLength += chip.Length;
                        lineWidth += fmtChip.WidthIncludingTrailingWhitespace;
                    }
                    else if (lineLength > 0)
                    {
                        // push stored words

                        var line = text.Substring(lineIndex, lineLength);
                        var fline = CreateFormattedText(line);
                        list.Add(CreateGeometry(line, fline, true));

                        lineIndex += lineLength;
                        lineLength = 0;
                        lineWidth = 0;

                        continue;
                    }
                    else
                    {
                        // The width is too narrow to render a word.
                        // Break down a word into letters.

                        var layout = new TextLayout(
                                            chip,
                                            typeface, fontSize,
                                            foreground,
                                            textWrapping: TextWrapping.Wrap,
                                            maxWidth: constraint);

                        int layoutIdx = lineIndex;
                        foreach (var line in layout.TextLines)
                        {
                            var brokenWord = text.Substring(layoutIdx, line.Length);
                            var fbrokenWord = CreateFormattedText(brokenWord);
                            list.Add(CreateGeometry(brokenWord, fbrokenWord, true));
                            layoutIdx += line.Length;
                        }

                        lineIndex += chip.Length;
                    }

                    if (!chipList.MoveNext()) break;

                    chip = chipList.Current;
                    fmtChip = CreateFormattedText(chipList.Current);
                }

                if (lineIndex < text.Length)
                {
                    var line = text.Substring(lineIndex);
                    var fline = CreateFormattedText(line);
                    list.Add(CreateGeometry(line, fline, true));
                }

                return list;
            }

            FormattedText CreateFormattedText(string text)
                => new FormattedText(
                        text,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        typeface, fontSize,
                        foreground);

            SingleTextLayoutGeometry CreateGeometry(string text, FormattedText formattedText, bool linebreak)
                => new SingleTextLayoutGeometry(
                        this,
                        formattedText,
                        TextVerticalAlignment,
                        text,
                        linebreak);
        }

        private static IEnumerable<string> SplitBreakPoint(string text)
        {
            var list = new List<string>();

            int idx = 0;
            var breakposis = new LineBreakEnumerator(text.AsMemory());
            while (breakposis.MoveNext())
            {
                var word = breakposis.Current;
                list.Add(text.Substring(idx, word.PositionWrap - idx));
                idx = word.PositionWrap;
            }

            return list;
        }
    }
}