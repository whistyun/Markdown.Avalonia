using System.Collections.Generic;

namespace ColorTextBlock.Avalonia
{
    /// <summary>
    /// Underline decoration
    /// </summary>
    public class CUnderline : CSpan
    {
        public CUnderline() { }

        public CUnderline(IEnumerable<CInline> inlines) : base(inlines)
        {
            IsUnderline = true;
        }
    }
}
