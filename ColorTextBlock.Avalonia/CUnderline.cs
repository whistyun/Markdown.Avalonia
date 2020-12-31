using System.Collections.Generic;

namespace ColorTextBlock.Avalonia
{
    public class CUnderline : CSpan
    {
        public CUnderline() { }

        public CUnderline(IEnumerable<CInline> inlines) : base(inlines)
        {
            IsUnderline = true;
        }
    }
}
