using ColorTextBlock.Avalonia;
using System.Collections.Generic;

namespace ColorDocument.Avalonia.DocumentElements
{
    public class HeaderElement : CTextBlockElement
    {
        public int Level { get; }

        public HeaderElement(IEnumerable<CInline> inlines, int level) :
            base(inlines, level switch
            {
                1 => ClassNames.Heading1Class,
                2 => ClassNames.Heading2Class,
                3 => ClassNames.Heading3Class,
                4 => ClassNames.Heading4Class,
                5 => ClassNames.Heading5Class,
                _ => ClassNames.Heading6Class,
            })
        {
            Level = level switch
            {
                1 => 1,
                2 => 2,
                3 => 3,
                4 => 4,
                5 => 5,
                _ => 6,
            };
        }
    }
}
