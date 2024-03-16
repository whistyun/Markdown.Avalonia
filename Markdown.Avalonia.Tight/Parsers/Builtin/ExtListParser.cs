using ColorDocument.Avalonia;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Markdown.Avalonia.Parsers.Builtin
{
    internal class ExtListParser : AbstractListParser
    {
        // `alphabet order` and `roman number` must start 'a.'～'c.' and 'i,'～'iii,'.
        // This restrict is avoid to treat "Yes," as list marker.
        private const string _extFirstListMaker = @"(?:[*+=-]|\d+[.]|[a-c][.]|[i]{1,3}[,]|[A-C][.]|[I]{1,3}[,])";
        private const string _extSubseqListMaker = @"(?:[*+=-]|\d+[.]|[a-c][.]|[cdilmvx]+[,]|[A-C][.]|[CDILMVX]+[,])";


        private static readonly Regex _extListNested = CreateWholeListPattern(_extFirstListMaker, _extSubseqListMaker);

        public ExtListParser() : base(_extListNested)
        {
        }

        public override IEnumerable<DocumentElement>? Convert2(
            string text, Match firstMatch,
            ParseStatus status,
            IMarkdownEngine2 engine,
            out int parseTextBegin, out int parseTextEnd)
        {
            return new DocumentElement[] {
                ListEvalutor(text, firstMatch, _extSubseqListMaker, engine, out parseTextBegin, out parseTextEnd)
            };
        }
    }
}
