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
            var inlines = engine.ParseGamutInline(header.Trim());
            var element = new HeaderElement(inlines, level);
            return new[] { element };
        }
    }
}
