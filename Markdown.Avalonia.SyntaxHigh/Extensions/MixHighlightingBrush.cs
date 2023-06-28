using Avalonia.Media;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Rendering;
using System;

namespace Markdown.Avalonia.SyntaxHigh.Extensions
{
    class MixHighlightingBrush : HighlightingBrush
    {
        private readonly HighlightingBrush _baseBrush;
        private readonly Color _fore;

        public MixHighlightingBrush(HighlightingBrush baseBrush, Color fore)
        {
            this._baseBrush = baseBrush;
            this._fore = fore;
        }

        public override IBrush GetBrush(ITextRunConstructionContext context)
        {
            var originalBrush = _baseBrush.GetBrush(context);

            return (originalBrush is ISolidColorBrush sbrsh) ?
                 new SolidColorBrush(WrapColor(sbrsh.Color)) :
                 originalBrush;
        }

        public override Color? GetColor(ITextRunConstructionContext context)
        {
            if (_baseBrush.GetBrush(context) is ISolidColorBrush sbrsh)
            {
                return WrapColor(sbrsh.Color);
            }
            else
            {
                var colorN = this._baseBrush.GetColor(context);
                return colorN.HasValue ? WrapColor(colorN.Value) : colorN;
            }
        }

        private Color WrapColor(Color color)
        {
            if (color.A == 0) return color;

            var foreMax = Math.Max(_fore.R, Math.Max(_fore.G, _fore.B));
            var tgtHsv = new HSV(color);

            int newValue = tgtHsv.Value + foreMax;
            int newSaturation = tgtHsv.Saturation;
            if (newValue > 255)
            {
                var newSaturation2 = newSaturation - (newValue - 255);
                newValue = 255;

                var sChRtLm = (color.R >= color.G && color.R >= color.B) ? 0.95f * 0.7f :
                              (color.G >= color.R && color.G >= color.B) ? 0.95f :
                                                                           0.95f * 0.5f;

                var sChRt = Math.Max(sChRtLm, newSaturation2 / (float)newSaturation);
                if (Single.IsInfinity(sChRt)) sChRt = sChRtLm;

                newSaturation = (int)(newSaturation * sChRt);
            }

            tgtHsv.Value = (byte)newValue;
            tgtHsv.Saturation = (byte)newSaturation;

            var newColor = tgtHsv.ToColor();
            return newColor;
        }
    }
}
