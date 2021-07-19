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
                Console.WriteLine("????"); // TODO: Check
                yield break;
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
            string entireText = Text;

            if (remainWidth != entireWidth)
            {
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
                var lastStart = midlayout.TextLines.Last().TextRange.Start;

                // TODO split linebreak
                var midTxt = entireText.Substring(0, lastStart);
                var lstTxt = entireText.Substring(lastStart);
                yield return NewGeometry2(midTxt, lstTxt != "", entireWidth);
                yield return NewGeometry(lstTxt, false);
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
