using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColorTextBlock.Avalonia
{
    public class CStrikethrough : CSpan
    {
        public CStrikethrough(IEnumerable<CInline> inlines) : base(inlines)
        {
            IsStrikethrough = true;
        }
    }
}
