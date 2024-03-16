using Avalonia.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Media;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ColorDocument.Avalonia;
using ColorDocument.Avalonia.DocumentElements;

namespace Markdown.Avalonia.Parsers.Builtin
{
    internal class FencedCodeBlockParser : BlockParser2
    {
        private static readonly Regex _codeBlockBegin = new(@"
                    ^          # Character before opening
                    [ ]{0,3}
                    (`{3,})          # $1 = Opening run of `
                    ([^\n`]*)        # $2 = The code lang
                    \n", RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.Compiled);

        private bool _enablePreRenderingCodeBlock;

        public FencedCodeBlockParser(bool enablePreRenderingCodeBlock) : base(_codeBlockBegin, "CodeBlocksWithLangEvaluator")
        {
            _enablePreRenderingCodeBlock = enablePreRenderingCodeBlock;
        }

        public override IEnumerable<DocumentElement>? Convert2(string text, Match firstMatch, ParseStatus status, IMarkdownEngine2 engine, out int parseTextBegin, out int parseTextEnd)
        {
            var closeTagPattern = new Regex($"\n[ ]*{firstMatch.Groups[1].Value}[ ]*\n");
            var closeTagMatch = closeTagPattern.Match(text, firstMatch.Index + firstMatch.Length);

            int codeEndIndex;
            if (closeTagMatch.Success)
            {
                codeEndIndex = closeTagMatch.Index;
                parseTextEnd = closeTagMatch.Index + closeTagMatch.Length;
            }
            else if (_enablePreRenderingCodeBlock)
            {
                codeEndIndex = text.Length;
                parseTextEnd = text.Length;
            }
            else
            {
                parseTextBegin = parseTextEnd = -1;
                return null;
            }

            parseTextBegin = firstMatch.Index;

            string code = text.Substring(firstMatch.Index + firstMatch.Length, codeEndIndex - (firstMatch.Index + firstMatch.Length));
            var border = Create(code);
            return new[] { new UnBlockElement(border) };
        }

        public static Border Create(string code)
        {
            var ctxt = new TextBlock()
            {
                Text = code,
                TextWrapping = TextWrapping.NoWrap
            };
            ctxt.Classes.Add(Markdown.CodeBlockClass);

            var scrl = new ScrollViewer();
            scrl.Classes.Add(Markdown.CodeBlockClass);
            scrl.Content = ctxt;
            scrl.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

            var border = new Border();
            border.Classes.Add(Markdown.CodeBlockClass);
            border.Child = scrl;

            return border;
        }
    }
}
