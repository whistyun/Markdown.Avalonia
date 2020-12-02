using Avalonia;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Remote.Protocol.Viewport;
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

        protected internal override IEnumerable<CGeometry> Measure(
            FontFamily parentFontFamily,
            double parentFontSize,
            FontStyle parentFontStyle,
            FontWeight parentFontWeight,
            IBrush parentForeground,
            IBrush parentBackground,
            bool parentUnderline,
            bool parentStrikethough,
            double entireWidth,
            double remainWidth)
        {
            var family = FontFamily ?? parentFontFamily;
            var size = FontSize.HasValue ? FontSize.Value : parentFontSize;
            var style = FontStyle.HasValue ? FontStyle.Value : parentFontStyle;
            var weight = FontWeight.HasValue ? FontWeight.Value : parentFontWeight;
            var foreground = Foreground ?? parentForeground;
            var background = Background ?? parentBackground;
            var underline = IsUnderline || parentUnderline;
            var strikethrough = IsStrikethrough || parentStrikethough;


            var infos = new List<TextGeometry>();

            var fmt = Measure(Size.Infinity, family, size, style, weight, TextWrapping.Wrap);

            if (String.IsNullOrEmpty(Text))
            {
                fmt.Text = "Ty";

                infos.Add(new TextGeometry(
                    0, fmt.Bounds.Height, false,
                    foreground, background,
                    underline, strikethrough,
                    "", fmt));

                return infos;
            }

            string[] txtChips = Regex.Split(Text, "\r\n|\r|\n");

            Tuple<string, bool>[] txtChipsWithLineBreak =
                txtChips.Select((t, i) => Tuple.Create(t, i < txtChips.Length - 1)).ToArray();

            foreach (Tuple<string, bool> txtChipEntry in txtChipsWithLineBreak)
            {
                string txtChip = txtChipEntry.Item1;
                bool lineBreak = txtChipEntry.Item2;

                if (string.IsNullOrEmpty(txtChip))
                {
                    // linebreak;
                    fmt.Text = "Ty";
                    infos.Add(TextGeometry.NewLine(fmt));

                    continue;
                }

                /*
                 * It is hacking-resolution for 'line breaking rules'.
                 * 
                 * TODO 後で、英訳する。
                 * 
                 * Avalonia(9.11)のFormattedTextでは、
                 * 矩形範囲に単一のスタイルで文字描画したときの改行位置しか計算できません。
                 * 
                 * そのため、 既に適当な文字を入力した後に、追加で別の文言を描画しようとした時、
                 * 以下のどちらの理由で改行が発生したか判断ができません。
                 * 
                 * 　理由1.余白が小さすぎるため改行が行われた
                 * 　理由2.描画領域が狭く(あるいは単語が長すぎるため)無理やり改行が行われた
                 * 
                 * 先頭にスペースを入れて改行位置を計算させることで、
                 * 理由1でも理由2でも先頭で改行が行われるようにしています。
                 * (この場合、スペース1文字を追加したために理由1に該当してしまう可能性がありますが、
                 *  スペースの横幅は小さいため、不自然には見えないと期待しています)
                 */
                string lineTxt = txtChip;

                if (entireWidth != remainWidth)
                {
                    fmt.Text = " " + lineTxt;
                    fmt.Constraint = new Size(remainWidth, Double.PositiveInfinity);

                    FormattedTextLine[] lines = fmt.GetLines().ToArray();
                    FormattedTextLine firstLine = lines[0];

                    string firstLineTxt = fmt.Text.Substring(0, firstLine.Length);

                    if (lines.Length == 1)
                    {
                        if (remainWidth < fmt.Bounds.Width)
                        {
                            // 指定条件を無視された場合(横幅が狭すぎる)は強制的に改行
                            infos.Add(TextGeometry.NewLine(fmt));
                            remainWidth = entireWidth;
                        }
                        else
                        {
                            // 1行しか無い場合は、余計なスペースを排除して寸法情報生成
                            fmt.Text = lineTxt;

                            infos.Add(new TextGeometry(
                                            fmt.Bounds.Width, fmt.Bounds.Height, lineBreak,
                                            foreground, background,
                                            underline, strikethrough,
                                            lineTxt, fmt));
                            remainWidth -= fmt.Bounds.Width;

                            continue;
                        }
                    }
                    else
                    {
                        fmt.Text = firstLineTxt;

                        infos.Add(new TextGeometry(
                                        fmt.Bounds.Width, fmt.Bounds.Height, true,
                                        foreground, background,
                                        underline, strikethrough,
                                        firstLineTxt, fmt));
                        remainWidth = entireWidth;

                        lineTxt = lineTxt.Substring(firstLineTxt.Length - 1);
                    }
                }

                fmt.Text = lineTxt;
                fmt.Constraint = new Size(entireWidth, Double.PositiveInfinity);

                int lineOffset = 0;
                FormattedTextLine[] ftlines = fmt.GetLines().ToArray();
                for (int idx = 0; idx < ftlines.Length; ++idx)
                {
                    FormattedTextLine line = ftlines[idx];

                    string chip = lineTxt.Substring(lineOffset, line.Length);

                    fmt.Text = chip;
                    double txtWid = fmt.Bounds.Width;

                    infos.Add(new TextGeometry(
                                        txtWid, line.Height, lineBreak || idx < ftlines.Length - 1,
                                        foreground, background,
                                        underline, strikethrough,
                                        chip, fmt));

                    lineOffset += line.Length;
                }

                remainWidth = entireWidth - infos.Last().Width;
            }

            return infos;
        }

        internal FormattedText Measure(
            Size constraint,
            FontFamily parentFontFamily,
            double parentFontSize,
            FontStyle parentFontStyle,
            FontWeight parentFontWeight,
            TextWrapping parentWrapping)
        {
            var typeface = new Typeface(
                    FontFamily ?? parentFontFamily,
                    FontStyle.HasValue ? FontStyle.Value : parentFontStyle,
                    FontWeight.HasValue ? FontWeight.Value : parentFontWeight);

            return new FormattedText
            {
                Constraint = constraint,
                Typeface = typeface,
                FontSize = FontSize.HasValue ? FontSize.Value : parentFontSize,
                Text = Text ?? string.Empty,
                TextAlignment = TextAlignment.Left,
                TextWrapping = parentWrapping,
            };
        }
    }
}
