using ColorDocument.Avalonia;
using ColorDocument.Avalonia.DocumentElements;
using ColorTextBlock.Avalonia;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Markdown.Avalonia.Parsers.Builtin
{
    internal abstract class AbstractHeaderParser : BlockParser2
    {
        protected AbstractHeaderParser(Regex pattern, string name) : base(pattern, name)
        {
        }

        protected IEnumerable<DocumentElement> Create(int level, string header, IMarkdownEngine2 engine)
        {
            var clsNm = level switch
            {
                1 => Markdown.Heading1Class,
                2 => Markdown.Heading2Class,
                3 => Markdown.Heading3Class,
                4 => Markdown.Heading4Class,
                5 => Markdown.Heading5Class,
                _ => Markdown.Heading6Class,
            };


            var inlines = engine.ParseGamutInline(header.Trim());
            var element = new CTextBlockElement(inlines, clsNm);
            return new[] { element };
        }
    }
}
