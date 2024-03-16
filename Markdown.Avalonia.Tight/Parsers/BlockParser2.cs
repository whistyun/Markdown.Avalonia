using Avalonia.Controls;
using ColorDocument.Avalonia;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Markdown.Avalonia.Parsers
{
    public abstract class BlockParser2 : BlockParser
    {
        public BlockParser2(Regex pattern, string name) : base(pattern, name)
        {
        }

        public abstract IEnumerable<DocumentElement>? Convert2(string text, Match firstMatch, ParseStatus status, IMarkdownEngine2 engine, out int parseTextBegin, out int parseTextEnd);

        public override IEnumerable<Control>? Convert(string text, Match firstMatch, ParseStatus status, IMarkdownEngine engine, out int parseTextBegin, out int parseTextEnd)
        {
            if (Convert2(text, firstMatch, status, engine.Upgrade(), out parseTextBegin, out parseTextEnd) is { } docs)
            {
                return docs.Select(d => d.Control);
            }
            return null;
        }
    }
}
