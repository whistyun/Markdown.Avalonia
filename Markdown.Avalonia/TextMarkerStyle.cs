using System;
using System.Collections.Generic;
using System.Text;

namespace Markdown.Avalonia
{
    public enum TextMarkerStyle
    {
        Box,
        Circle,
        Decimal,
        Disc,

        LowerLatin,
        LowerRoman,

        UpperLatin,
        UpperRoman,

        Square,
    }

    public static class MarkdownStyleExt
    {
        public static string CreateMakerText(this TextMarkerStyle textMarker, int index)
        {
            switch (textMarker)
            {
                default:
                    throw new InvalidOperationException("sorry library manager forget to modify about listmerker.");

                case TextMarkerStyle.Disc:
                    return "•";

                case TextMarkerStyle.Box:
                    return "▪";

                case TextMarkerStyle.Circle:
                    return "○";

                case TextMarkerStyle.Square:
                    return "❏";

                case TextMarkerStyle.Decimal:
                    return (index + 1).ToString() + ".";

                case TextMarkerStyle.LowerLatin:
                    return NumberToOrder.ToLatin(index + 1).ToLower() + ".";

                case TextMarkerStyle.UpperLatin:
                    return NumberToOrder.ToLatin(index + 1) + ".";

                case TextMarkerStyle.LowerRoman:
                    return NumberToOrder.ToRoman(index + 1).ToLower() + ".";

                case TextMarkerStyle.UpperRoman:
                    return NumberToOrder.ToRoman(index + 1) + ".";
            }
        }
    }
}
