using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Weight = Avalonia.Media.FontWeight;

namespace ColorTextBlock.Avalonia
{
    public class CBold : CSpan
    {
        public CBold(IEnumerable<CInline> inlines) : base(inlines)
        {
            FontWeight = Weight.Bold;
        }
    }
}
