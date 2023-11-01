using System.Collections.Generic;
using Weight = Avalonia.Media.FontWeight;

namespace ColorTextBlock.Avalonia
{
    /// <summary>
    /// Bold decoration
    /// </summary>
    public class CBold : CSpan
    {
        public CBold() { }

        public CBold(IEnumerable<CInline> inlines) : base(inlines)
        {
            FontWeight = Weight.Bold;
        }
    }
}
