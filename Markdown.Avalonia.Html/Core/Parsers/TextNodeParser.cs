using Avalonia;
using Avalonia.Controls.Documents;
using ColorTextBlock.Avalonia;
using HtmlAgilityPack;
using Markdown.Avalonia.Html.Core.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Markdown.Avalonia.Html.Core.Parsers
{
    public class TextNodeParser : IInlineTagParser
    {
        public IEnumerable<string> SupportTag => new[] { HtmlNode.HtmlNodeTypeNameText };

        public bool TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<CInline> generated)
        {
            if (node is HtmlTextNode textNode)
            {
                var nodeText = textNode.Text;
                var lineBreakIdx = nodeText.IndexOf("\n\n");

                if (lineBreakIdx == -1)
                {
                    generated = Replace(textNode.Text, manager);
                }
                else {
                    var preText = nodeText.Substring(0, lineBreakIdx + 1);
                    var pstText = nodeText.Substring(lineBreakIdx + 2);

                    if (preText == "\n") // empty line
                    {
                        generated = manager.Engine.ParseGamutInline(pstText);
                    }
                    else {
                        generated = Replace(preText, manager)
                                    .Concat(manager.Engine.ParseGamutInline(pstText));
                    }
                }

                return true;
            }

            generated = EnumerableExt.Empty<CInline>();
            return false;
        }

        public IEnumerable<CInline> Replace(string text, ReplaceManager manager)
            => text.StartsWith("\n") ?
                    new[] { new CRun() { Text = text.Replace('\n', ' ') } } :
                    manager.Engine.ParseGamutInline(text.Replace('\n', ' '));
    }
}
