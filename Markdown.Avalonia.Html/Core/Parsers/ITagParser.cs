using ColorTextBlock.Avalonia;
using ColorDocument.Avalonia;
using HtmlAgilityPack;
using System.Collections.Generic;

namespace Markdown.Avalonia.Html.Core.Parsers
{
    public interface ITagParserBase
    {
        IEnumerable<string> SupportTag { get; }
    }

    public interface IInlineTagParser : ITagParserBase
    {
        bool TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<CInline> generated);
    }

    public interface IBlockTagParser : ITagParserBase
    {
        bool TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<DocumentElement> generated);
    }

    public interface IHasPriority
    {
        int Priority { get; }
    }

    public static class HasPriority
    {
        public const int DefaultPriority = 10000;

        public static int GetPriority(this ITagParserBase parser)
            => parser is IHasPriority prop ? prop.Priority : HasPriority.DefaultPriority;
    }
}
