using Avalonia;
using Avalonia.Controls;
using ColorDocument.Avalonia;
using ColorDocument.Avalonia.DocumentElements;
using HtmlAgilityPack;
using Markdown.Avalonia.Html.Core.Utils;
using Markdown.Avalonia.SyntaxHigh;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markdown.Avalonia.Html.Core.Parsers
{
    public class CodeBlockParser : IBlockTagParser
    {
        SyntaxHighlightProvider _provider;

        public CodeBlockParser(SyntaxHighlight syntax)
        {
            _provider = new SyntaxHighlightProvider(syntax.Aliases);
        }


        public IEnumerable<string> SupportTag => new[] { "pre" };

        public bool TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<DocumentElement> generated)
        {
            generated = EnumerableExt.Empty<DocumentElement>();

            var codeElements = node.ChildNodes.CollectTag("code");
            if (codeElements.Count != 0)
            {
                var rslt = new List<DocumentElement>();

                foreach (var codeElement in codeElements)
                {
                    // "language-**", "lang-**", "**" or "sourceCode **"
                    var classVal = codeElement.Attributes["class"]?.Value;

                    var langCode = ParseLangCode(classVal);
                    rslt.Add(new UnBlockElement(DocUtils.CreateCodeBlock(langCode, codeElement.InnerText, manager, _provider)));
                }

                generated = rslt;
                return rslt.Count > 0;
            }
            else if (node.ChildNodes.TryCastTextNode(out var textNodes))
            {
                var buff = new StringBuilder();
                foreach (var textNode in textNodes)
                    buff.Append(textNode.InnerText);

                generated = new DocumentElement[] { new UnBlockElement(DocUtils.CreateCodeBlock(null, buff.ToString(), manager, _provider)) };
                return true;
            }
            else return false;
        }

        private static string ParseLangCode(string? classVal)
        {
            if (classVal is null) return "";

            // "language-**", "lang-**", "**" or "sourceCode **"
            var indics = Enumerable.Range(0, classVal.Length)
                                   .Reverse()
                                   .Where(i => !Char.IsLetterOrDigit(classVal[i]));

            return classVal.Substring(indics.Any() ? indics.First() + 1 : 0);
        }
    }
}
