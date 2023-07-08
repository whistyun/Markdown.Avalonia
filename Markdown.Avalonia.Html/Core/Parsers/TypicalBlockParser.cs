using Avalonia;
using Avalonia.Controls;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;

namespace Markdown.Avalonia.Html.Core.Parsers
{
    public class TypicalBlockParser : IBlockTagParser
    {
        private const string _resource = "Markdown.Avalonia.Html.Core.Parsers.TypicalBlockParser.tsv";
        private TypicalParseInfo _parser;

        public IEnumerable<string> SupportTag => new[] { _parser.HtmlTag };

        public TypicalBlockParser(TypicalParseInfo parser)
        {
            _parser = parser;
        }

        bool ITagParser.TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<StyledElement> generated)
        {
            var rtn = _parser.TryReplace(node, manager, out var list);
            generated = list;
            return rtn;
        }

        public bool TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<Control> generated)
        {
            var rtn = _parser.TryReplace(node, manager, out var list);
            generated = list.Cast<Control>();
            return rtn;
        }

        public static IEnumerable<TypicalBlockParser> Load()
        {
            foreach (var info in TypicalParseInfo.Load(_resource))
            {
                yield return new TypicalBlockParser(info);
            }
        }
    }
}
