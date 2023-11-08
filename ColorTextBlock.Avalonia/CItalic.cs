using System.Collections.Generic;
using FStyle = Avalonia.Media.FontStyle;

namespace ColorTextBlock.Avalonia
{
    /// <summary>
    /// Italic decoration
    /// </summary>
    public class CItalic : CSpan
    {
        public CItalic() { }

        public CItalic(IEnumerable<CInline> inlines) : base(inlines)
        {
            FontStyle = FStyle.Italic;
        }
    }
}
