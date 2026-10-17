using Avalonia;
using Avalonia.Controls;
using ColorDocument.Avalonia;
using ColorDocument.Avalonia.DocumentElements;
using HtmlAgilityPack;
using Markdown.Avalonia.Controls;
using System.Collections.Generic;

namespace Markdown.Avalonia.Html.Core.Parsers
{
    public class HorizontalRuleParser : IBlockTagParser
    {
        public IEnumerable<string> SupportTag => new[] { "hr" };

        public bool TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<DocumentElement> generated)
        {
            var rule = new Rule(RuleType.Single);
            rule.Classes.Add(Tags.TagRuleSingle.GetClass());

            generated = new DocumentElement[] { new UnBlockElement(rule) };
            return true;
        }
    }
}
