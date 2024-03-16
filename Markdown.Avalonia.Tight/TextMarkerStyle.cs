using ColorDocument.Avalonia.DocumentElements;
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
        public static ColorDocument.Avalonia.DocumentElements.TextMarkerStyle Change(this TextMarkerStyle style)
        {
            return style switch
            {
                TextMarkerStyle.Box => ColorDocument.Avalonia.DocumentElements.TextMarkerStyle.Box,
                TextMarkerStyle.Circle => ColorDocument.Avalonia.DocumentElements.TextMarkerStyle.Circle,
                TextMarkerStyle.Decimal => ColorDocument.Avalonia.DocumentElements.TextMarkerStyle.Decimal,
                TextMarkerStyle.Disc => ColorDocument.Avalonia.DocumentElements.TextMarkerStyle.Disc,
                TextMarkerStyle.LowerLatin => ColorDocument.Avalonia.DocumentElements.TextMarkerStyle.LowerLatin,
                TextMarkerStyle.LowerRoman => ColorDocument.Avalonia.DocumentElements.TextMarkerStyle.LowerRoman,
                TextMarkerStyle.UpperLatin => ColorDocument.Avalonia.DocumentElements.TextMarkerStyle.UpperLatin,
                TextMarkerStyle.UpperRoman => ColorDocument.Avalonia.DocumentElements.TextMarkerStyle.UpperRoman,
                TextMarkerStyle.Square => ColorDocument.Avalonia.DocumentElements.TextMarkerStyle.Square,
            };
        }

        public static string CreateMakerText(this TextMarkerStyle textMarker, int index)
            => textMarker.Change().CreateMakerText(index);
    }
}
