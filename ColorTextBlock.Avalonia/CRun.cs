using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Metadata;
using ColorTextBlock.Avalonia.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ColorTextBlock.Avalonia
{
    public class CRun : CInline
    {
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
            var creator = new LayoutCreateor(
                FontFamily,
                FontStyle,
                FontWeight,
                FontSize);


            SingleTextLayoutGeometry NewGeometry(string text, bool linebreak)
                => new SingleTextLayoutGeometry(this, creator.Create(text, Foreground), creator.Create, TextVerticalAlignment, text, linebreak);

            SingleTextLayoutGeometry NewGeometry2(string text, bool linebreak, double width)
                => new SingleTextLayoutGeometry(this, creator.Create(text, Foreground, width), creator.WithConstraint(width), TextVerticalAlignment, text, linebreak);

            if (String.IsNullOrEmpty(Text))
            {
                yield break;
            }

            string entireText = Text;

            if (remainWidth != entireWidth)
            {
                /*
                 * It is hacking-resolution for 'line breaking rules'.
                 * 
                 * insert one space in the head to detect the line break position
                 *   |                        |                 |                        |
                 *   | xxxxxx xxxxxx          |   rather than   | xxxxxx xxxxxx internat |
                 *   | internationalization   |                 | ionalization           |
                 *   |                        |                 |                        |
                 */

                var firstTxtLen =
                        creator.Create(" " + entireText, Foreground, remainWidth)
                            .TextLines.First().TextRange.Length;

                firstTxtLen = Math.Max(firstTxtLen - 1, 0);

                if (firstTxtLen > 0)
                {
                    var firstText = entireText.Substring(0, firstTxtLen);
                    entireText = entireText.Substring(firstTxtLen);
                    yield return NewGeometry(firstText, entireText != "");
                }
                else
                {
                    yield return new LineBreakMarkGeometry(this);
                }

                if (String.IsNullOrEmpty(entireText))
                    yield break;
            }

            var midlayout = creator.Create(entireText, Foreground, entireWidth);

            if (midlayout.TextLines.Count >= 2)
            {
                //var lastStart = midlayout.TextLines.Last().TextRange.Start;
                //
                //var midTxt = entireText.Substring(0, lastStart);
                //var lstTxt = entireText.Substring(lastStart);
                //yield return NewGeometry2(midTxt, true, entireWidth);
                //yield return NewGeometry(lstTxt, false);

                var ranges = midlayout.TextLines.Select(ln => ln.TextRange).ToArray();
                var lastRange = ranges[ranges.Length - 1];

                foreach (var range in ranges)
                {
                    var line = entireText.Substring(range.Start, range.Length);
                    yield return NewGeometry(line, !range.Equals(lastRange));
                }
            }
            else
            {
                yield return new SingleTextLayoutGeometry(
                        this,
                        midlayout,
                        creator.Create,
                        TextVerticalAlignment,
                        entireText,
                        false);
            }
        }
    }

    class LayoutCreateor
    {
        public Typeface Typeface { get; }
        public double FontSize { get; }

        public LayoutCreateor(
            FontFamily fontFamily,
            FontStyle fontStyle,
            FontWeight fontWeight,
            double fontSize)
        {
            Typeface = new Typeface(
                    fontFamily,
                    fontStyle,
                    fontWeight);
            FontSize = fontSize;
        }

        public TextLayoutCreator WithConstraint(double width)
        => (text, foreground) => Create(text, foreground, width);

        public TextLayout Create(
                string text,
                IBrush foreground)
        => Create(text, foreground, Double.PositiveInfinity);

        public TextLayout Create(
                string text,
                IBrush foreground,
                double width)
        {
            return new TextLayout(
                text ?? string.Empty,
                Typeface,
                FontSize,
                foreground,
                textWrapping: TextWrapping.Wrap,
                maxWidth: width,
                maxHeight: double.PositiveInfinity);
        }
    }
}
