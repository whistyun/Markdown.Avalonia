using Markdown.Avalonia;
using System;

namespace ColorDocument.Avalonia.DocumentElements
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
        /// <param name="index">Zero-based item index within the list.</param>
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
                    return index.ToString() + ".";

                case TextMarkerStyle.LowerLatin:
                    return NumberToOrder.ToLatin((int)index).ToLower() + ".";

                case TextMarkerStyle.UpperLatin:
                    return NumberToOrder.ToLatin((int)index) + ".";

                case TextMarkerStyle.LowerRoman:
                    return NumberToOrder.ToRoman((int)index).ToLower() + ".";

                case TextMarkerStyle.UpperRoman:
                    return NumberToOrder.ToRoman((int)index) + ".";
            }
        }
    }
}
