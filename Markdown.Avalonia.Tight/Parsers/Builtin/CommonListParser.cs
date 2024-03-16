using ColorDocument.Avalonia;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Markdown.Avalonia.Parsers.Builtin
{
    internal class CommonListParser : AbstractListParser
    {
        private const string _commonListMaker = @"(?:[*+-]|\d+[.])";

        private static readonly Regex _commonListNested = CreateWholeListPattern(_commonListMaker, _commonListMaker);

        public CommonListParser() : base(_commonListNested)
        {
        }

        public override IEnumerable<DocumentElement>? Convert2(
            string text, Match firstMatch,
            ParseStatus status,
            IMarkdownEngine2 engine,
            out int parseTextBegin, out int parseTextEnd)
        {
            return new DocumentElement[] {
                ListEvalutor(text, firstMatch, _commonListMaker, engine, out parseTextBegin, out parseTextEnd)
            };
        }
    }
}
