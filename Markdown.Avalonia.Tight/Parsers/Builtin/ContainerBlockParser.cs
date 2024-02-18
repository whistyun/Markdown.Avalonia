using Avalonia.Controls;
using ColorDocument.Avalonia;
using ColorDocument.Avalonia.DocumentElements;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Markdown.Avalonia.Parsers.Builtin
{
    internal class ContainerBlockParser : BlockParser2
    {
        private static readonly Regex _containerBlockFirst = new(@"
                    ^          # Character before opening
                    [ ]{0,3}
                    (:{3,})          # $1 = Opening run of `
                    ([^\n`]*)        # $2 = The container type
                    \n
                    ((.|\n)+?)       # $3 = The code block
                    \n[ ]*
                    \1
                    (?!:)[\n]+", RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.Compiled);


        public ContainerBlockParser() : base(_containerBlockFirst, "ContainerBlockEvaluator")
        {
        }

        public override IEnumerable<DocumentElement>? Convert2(string text, Match firstMatch, ParseStatus status, IMarkdownEngine2 engine, out int parseTextBegin, out int parseTextEnd)
        {
            parseTextBegin = firstMatch.Index;
            parseTextEnd = parseTextBegin + firstMatch.Length;

            var name = firstMatch.Groups[2].Value;
            var content = firstMatch.Groups[3].Value;

            var gen = engine.ContainerBlockHandler;

            if (gen?.ProvideControl(engine.AssetPathRoot, name, content) is { } container)
            {
                container.Classes.Add(Markdown.ContainerBlockClass);
                return new[] { new UnBlockElement(container) };
            }
            else
            {
                Border border = FencedCodeBlockParser.Create(firstMatch.Value);
                border.Classes.Add(Markdown.NoContainerClass);
                return new[] { new UnBlockElement(border) };
            }
        }
    }
}
