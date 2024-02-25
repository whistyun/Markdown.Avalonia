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

        internal List<CGeometry>? Geometries { private set; get; }


        protected override IEnumerable<CGeometry> MeasureOverride(
            double entireWidth,
            double remainWidth)
        {
            if (String.IsNullOrEmpty(Text))
            {
                Geometries = null;
                return Array.Empty<CGeometry>();
            }

            Geometries = PrivateMeasureOverride(entireWidth, remainWidth);
            return Geometries;
        }

        protected List<CGeometry> PrivateMeasureOverride(
            double entireWidth,
            double remainWidth)
        {
            if (remainWidth == entireWidth)
            {
                return Create(Text, entireWidth);
            }

            var layout = new TextLayout(
                                Text,
                                Typeface, FontSize,
                                Foreground,
                                textWrapping: TextWrapping.Wrap);

            TextLine firstLine = layout.TextLines[0];

            if (firstLine.Width < remainWidth)
            {
                return layout.TextLines.Count == 1 ?
                    Create(Text, layout) :
                    Create(Text, entireWidth);
            }
            else
            {
                string firstLineText = Text.Substring(firstLine.FirstTextSourceIndex, firstLine.Length);

                var firstLineLayout = new TextLayout(
                                              firstLineText,
                                              Typeface, FontSize,
                                              Foreground,
                                              textWrapping: TextWrapping.Wrap,
                                              maxWidth: remainWidth);


                var breakPosEnum = new LineBreakEnumerator(firstLineText.AsMemory().Span);
                int breakPos = breakPosEnum.MoveNext(out var lnbrk) ?
                                    lnbrk.PositionWrap :
                                    int.MaxValue;


                if (breakPos < firstLineLayout.TextLines[0].Length)
                {
                    // correct wrap

                    var secondalyText = Text.Substring(firstLineLayout.TextLines[0].Length);

                    var list = Create(secondalyText, entireWidth);

                    list.Insert(0, new TextLineGeometry(this, firstLineText, firstLineLayout.TextLines[0], true));

                    return list;
                }
                else
                {
                    // wrong wrap; first line word is too long

                    var list = Create(Text, entireWidth);

                    list.Insert(0, new LineBreakMarkGeometry(this));

                    return list;
                }
            }
        }

        private List<CGeometry> Create(string text, double maxWidth)
        => Create(
            text,
            new TextLayout(
                    text,
                    Typeface, FontSize,
                    Foreground,
                    textWrapping: TextWrapping.Wrap,
                    maxWidth: maxWidth));

        private List<CGeometry> Create(string text, TextLayout layout)
        {
            var rslt = new List<CGeometry>();

            var textlines = layout.TextLines;
            for (int j = 0; j < textlines.Count; ++j)
            {
                var line = textlines[j];
                var chip = text.Substring(line.FirstTextSourceIndex, line.Length);

                var linebreak = j != textlines.Count - 1;

                rslt.Add(new TextLineGeometry(this, chip, line, linebreak));
            }

            return rslt;
        }

        public override string AsString() => Text;
    }
}