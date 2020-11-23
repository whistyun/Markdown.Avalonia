using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColorTextBlock.Avalonia
{
    public class CUnderline : CSpan
    {
        public CUnderline(IEnumerable<CInline> inlines) : base(inlines)
        {
            IsUnderline = true;
        }
    }
}
