using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Media.TextFormatting.Unicode;
using Avalonia.Metadata;
using ColorTextBlock.Avalonia.Geometries;
using System;
using System.Collections.Generic;
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
                return Array.Empty<CGeometry>();
            }

            var typeface = new Typeface(FontFamily, FontStyle, FontWeight);

            if (remainWidth == entireWidth)
            {
                return Convert(
                    new TextLayout(
                            Text,
                            typeface, FontSize,
                            Foreground,
                            textWrapping: TextWrapping.Wrap,
                            maxWidth: entireWidth)
                );
            }

            var layout = new TextLayout(
                                Text,
                                typeface, FontSize,
                                Foreground,
                                textWrapping: TextWrapping.Wrap);

            IReadOnlyList<TextLine> lines = layout.TextLines;

            TextLine firstLine = lines[0];

            if (firstLine.Width < remainWidth)
            {
                return Convert(
                    lines.Count == 1 ?
                        layout :
                        new TextLayout(
                                Text,
                                typeface, FontSize,
                                Foreground,
                                textWrapping: TextWrapping.Wrap,
                                maxWidth: entireWidth)
                );
            }
            else
            {
                string firstLineText = Text.Substring(firstLine.FirstTextSourceIndex, firstLine.Length);

                var firstLineLayout = new TextLayout(
                                              firstLineText,
                                              typeface, FontSize,
                                              Foreground,
                                              textWrapping: TextWrapping.Wrap,
                                              maxWidth: remainWidth);


                var breakPosEnum = new LineBreakEnumerator(firstLineText.AsMemory());
                int breakPos = breakPosEnum.MoveNext() ?
                                    breakPosEnum.Current.PositionWrap :
                                    int.MaxValue;


                if (breakPos < firstLineLayout.TextLines[0].Length)
                {
                    // correct wrap

                    var list = Convert(
                        new TextLayout(
                                Text.Substring(firstLineLayout.TextLines[0].Length),
                                typeface, FontSize,
                                Foreground,
                                textWrapping: TextWrapping.Wrap,
                                maxWidth: entireWidth)
                    );

                    list.Insert(0, Convert(firstLineLayout.TextLines[0], true));

                    return list;
                }
                else
                {
                    // wrong wrap; first line word is too long

                    var list = Convert(
                        new TextLayout(
                                Text,
                                typeface, FontSize,
                                Foreground,
                                textWrapping: TextWrapping.Wrap,
                                maxWidth: entireWidth)
                    );

                    list.Insert(0, new LineBreakMarkGeometry(this));

                    return list;
                }
            }
        }

        private List<CGeometry> Convert(TextLayout layout)
        {
            var rslt = new List<CGeometry>();

            var textlines = layout.TextLines;
            for (int j = 0; j < textlines.Count; ++j)
            {
                var line = textlines[j];

                rslt.Add(Convert(line, j != textlines.Count - 1));
            }

            return rslt;
        }

        private CGeometry Convert(TextLine line, bool linebreak)
            => new TextLineGeometry(
                        this,
                        line,
                        TextVerticalAlignment,
                        Text.Substring(line.FirstTextSourceIndex, line.Length),
                        linebreak);
    }
}