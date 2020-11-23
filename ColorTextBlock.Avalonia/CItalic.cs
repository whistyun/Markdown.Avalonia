using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FStyle = Avalonia.Media.FontStyle;

namespace ColorTextBlock.Avalonia
{
    public class CItalic : CSpan
    {
        public CItalic(IEnumerable<CInline> inlines) : base(inlines)
        {
            FontStyle = FStyle.Italic;
        }
    }
}
