using Avalonia.Media;
using ColorTextBlock.Avalonia.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FStyle = Avalonia.Media.FontStyle;

namespace ColorTextBlock.Avalonia
{
    public class CHyperlink : CSpan
    {
        public Action<string> Command { get; set; }
        public string CommandParameter { get; set; }

        public CHyperlink(IEnumerable<CInline> inlines) : base(inlines)
        {
            IsUnderline = true;
            Foreground = new SolidColorBrush(Colors.Blue);
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
            var metrics = base.Measure(
                parentFontFamily,
                parentFontSize,
                parentFontStyle,
                parentFontWeight,
                parentForeground,
                parentBackground,
                parentUnderline,
                parentStrikethough,
                entireWidth,
                remainWidth);

            foreach (CGeometry metry in metrics)
            {
                metry.OnClick = () => Command?.Invoke(CommandParameter);
                if (metry is TextGeometry tmetry)
                {
                }


                yield return metry;
            }
        }
    }
}
