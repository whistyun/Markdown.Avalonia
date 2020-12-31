using System.Collections.Generic;

namespace ColorTextBlock.Avalonia
{
    public class CStrikethrough : CSpan
    {
        public CStrikethrough() { }

        public CStrikethrough(IEnumerable<CInline> inlines) : base(inlines)
        {
            IsStrikethrough = true;
        }
    }
}
