using Avalonia;
using Avalonia.Controls;
using HtmlAgilityPack;
using Markdown.Avalonia.Controls;
using System.Collections.Generic;

namespace Markdown.Avalonia.Html.Core.Parsers
{
    public class HorizontalRuleParser : IBlockTagParser
    {
        public IEnumerable<string> SupportTag => new[] { "hr" };

        bool ITagParser.TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<StyledElement> generated)
        {
            var rtn = TryReplace(node, manager, out var list);
            generated = list;
            return rtn;
        }

        public bool TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<Control> generated)
        {
            var rule = new Rule(RuleType.Single);
            rule.Classes.Add(Tags.TagRuleSingle.GetClass());

            generated = new[] { rule };
            return true;
        }
    }
}
