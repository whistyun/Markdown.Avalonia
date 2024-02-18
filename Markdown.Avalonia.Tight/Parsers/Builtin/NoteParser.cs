using Avalonia.Media;
using ColorDocument.Avalonia;
using ColorDocument.Avalonia.DocumentElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Markdown.Avalonia.Parsers.Builtin
{
    internal class NoteParser : BlockParser2
    {
        /// <summary>
        /// Turn Markdown into HTML paragraphs.
        /// </summary>
        /// <remarks>
        /// < Note
        /// </remarks>
        private static readonly Regex _note = new(@"
                ^(\<)       # $1 = starting marker <
                [ ]*
                (.+?)       # $2 = Header text
                [ ]*
                \>*         # optional closing >'s (not counted)
                \n+
            ", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private static readonly Regex _align = new(@"^p([<=>])\.", RegexOptions.Compiled);

        public NoteParser() : base(_note, "NoteEvaluator")
        {
        }

        public override IEnumerable<DocumentElement>? Convert2(string text, Match firstMatch, ParseStatus status, IMarkdownEngine2 engine, out int parseTextBegin, out int parseTextEnd)
        {
            parseTextBegin = firstMatch.Index;
            parseTextEnd = parseTextBegin + firstMatch.Length;

            string content = firstMatch.Groups[2].Value;

            TextAlignment? indiAlignment = null;

            if (status.SupportTextAlignment)
            {
                var alignMatch = _align.Match(content);
                if (alignMatch.Success)
                {
                    content = content.Substring(alignMatch.Length);
                    switch (alignMatch.Groups[1].Value)
                    {
                        case "<":
                            indiAlignment = TextAlignment.Left;
                            break;
                        case ">":
                            indiAlignment = TextAlignment.Right;
                            break;
                        case "=":
                            indiAlignment = TextAlignment.Center;
                            break;
                    }
                }
            }

            return new[] { new NoteBlockElement(engine.ParseGamutInline(content), indiAlignment) };
        }
    }
}
