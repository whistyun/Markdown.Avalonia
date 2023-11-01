using System.Collections.Generic;

namespace ColorTextBlock.Avalonia
{
    /// <summary>
    /// Strikethrough decoration
    /// </summary>
    public class CStrikethrough : CSpan
    {
        public CStrikethrough() { }

        public CStrikethrough(IEnumerable<CInline> inlines) : base(inlines)
        {
            IsStrikethrough = true;
        }
    }
}
