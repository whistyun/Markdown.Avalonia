using ColorDocument.Avalonia;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Markdown.Avalonia.Parsers.Builtin
{
    internal class CommonListParser : AbstractListParser
    {
        private const string _commonListMaker = @"(?:[*+-]|\d+[.])";

        private static readonly Regex _startNoIndentCommonSublistMarker = new(@"\A" + _commonListMaker, RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

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
            return new DocumentElement[] { ListEvalutor(firstMatch, _startNoIndentCommonSublistMarker, engine, out parseTextBegin, out parseTextEnd) };
        }
    }
}
