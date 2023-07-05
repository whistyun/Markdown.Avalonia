using Avalonia;
using Avalonia.Controls;
using ColorTextBlock.Avalonia;
using HtmlAgilityPack;
using System.Collections.Generic;

namespace Markdown.Avalonia.Html.Core.Parsers
{
    public class ProgressParser : IInlineTagParser
    {
        public IEnumerable<string> SupportTag => new[] { "progress", "meter" };

        bool ITagParser.TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<StyledElement> generated)
        {
            var rtn = TryReplace(node, manager, out var list);
            generated = list;
            return rtn;
        }

        public bool TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<CInline> generated)
        {
            var bar = new ProgressBar()
            {
                Value = TryParse(node.Attributes["value"]?.Value, 1),
                Minimum = TryParse(node.Attributes["min"]?.Value, 0),
                Maximum = TryParse(node.Attributes["max"]?.Value, 1),
                Width = 50,
                Height = 12,
            };
            generated = new[] { new CInlineUIContainer(bar) };
            return true;
        }

        private static int TryParse(string? txt, int def)
        {
            if (txt is null) return def;
            return int.TryParse(txt, out var v) ? v : def;
        }
    }
}
