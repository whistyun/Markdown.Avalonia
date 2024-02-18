using Avalonia.Layout;
using Avalonia.Media;
using ColorDocument.Avalonia;
using ColorDocument.Avalonia.DocumentElements;
using ColorTextBlock.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Markdown.Avalonia.Parsers.Builtin
{
    internal class TableParser : BlockParser2
    {
        private static readonly Regex _table = new(@"
            (                               # whole table
                [ \n]*
                (?<hdr>                     # table header
                    ([^\n\|]*\|[^\n]+)
                )
                [ ]*\n[ ]*
                (?<col>                     # column style
                    \|?([ ]*:?-+:?[ ]*(\||$))+
                )
                (?<row>                     # table row
                    (
                        [ ]*\n[ ]*
                        ([^\n\|]*\|[^\n]+)
                    )+
                )
            )",
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        public TableParser() : base(_table, "TableEvalutor")
        {
        }

        public override IEnumerable<DocumentElement>? Convert2(
            string text, Match firstMatch,
            ParseStatus status,
            IMarkdownEngine2 engine,
            out int parseTextBegin, out int parseTextEnd)
        {
            parseTextBegin = firstMatch.Index;
            parseTextEnd = parseTextBegin + firstMatch.Length;

            return new[] { TableEvalutor(firstMatch, engine) };
        }

        private TableBlockElement TableEvalutor(Match match, IMarkdownEngine2 engine)
        {
            Dictionary<int, TextAlignment> styleMt =
                ExtractCoverBar(match.Groups["col"].Value.Trim())
                    .Split('|')
                    .Select((styleText, idx) =>
                    {
                        var text = styleText.Trim();
                        var firstChar = text[0];
                        var lastChar = text[text.Length - 1];

                        return
                            firstChar == ':' && lastChar == ':' ?
                                 Tuple.Create(idx, (TextAlignment?)TextAlignment.Center) :

                            lastChar == ':' ?
                                Tuple.Create(idx, (TextAlignment?)TextAlignment.Right) :

                            firstChar == ':' ?
                                Tuple.Create(idx, (TextAlignment?)TextAlignment.Left) :

                                Tuple.Create(idx, (TextAlignment?)null);
                    })
                    .Where(tpl => tpl.Item2.HasValue)
                    .ToDictionary(tpl => tpl.Item1, tpl => tpl.Item2!.Value);


            int colOffset = 0;
            TableCellElement[][] headerCells = new[] { CreateRow(styleMt, match.Groups["hdr"].Value, engine, true) };

            List<TableCellElement[]> detailCells = new();
            foreach (var cellline in match.Groups["row"].Value.Trim().Split('\n'))
            {
                detailCells.Add(CreateRow(styleMt, cellline, engine, false));
            }


            return new TableBlockElement(headerCells, detailCells.ToArray(), Array.Empty<TableCellElement[]>(), true);
        }

        private TableCellElement[] CreateRow(Dictionary<int, TextAlignment> styleMt, string txt, IMarkdownEngine2 engine, bool ignoreRowSpan)
        {
            int colOffset = 0;
            List<TableCellElement> cells = new();
            foreach (var celltxt in ExtractCoverBar(txt.Trim()).Split('|'))
            {
                var cell = CreateCell(celltxt, engine);

                if (ignoreRowSpan)
                    cell.RowSpan = 1;

                // apply text align
                if (styleMt.TryGetValue(colOffset, out var style))
                    cell.Horizontal = style;

                cells.Add(cell);

                colOffset += cell.ColSpan;
            }
            return cells.ToArray();
        }

        private TableCellElement CreateCell(string txt, IMarkdownEngine2 engine)
        {
            int colspan = 1;
            int rowspan = 1;
            TextAlignment? horizontal = null;
            VerticalAlignment? vertical = null;

            int idx = txt.IndexOf('.');
            if (idx != -1)
            {
                var styleTxt = txt.Substring(0, idx);

                for (var i = 0; i < styleTxt.Length; ++i)
                {
                    switch (styleTxt[i])
                    {
                        case '/': // /2 rowspan
                            ++i;
                            var numTxt = ContinueToNum(styleTxt, ref i);
                            if (numTxt.Length == 0) goto default;
                            rowspan = int.Parse(numTxt);

                            break;

                        case '\\': // \2 colspan
                            ++i;
                            numTxt = ContinueToNum(styleTxt, ref i);
                            if (numTxt.Length == 0) goto default;
                            colspan = int.Parse(numTxt);
                            break;

                        case '<': // < left align
                            horizontal = TextAlignment.Left;
                            break;

                        case '>': // > right align
                            horizontal = TextAlignment.Right;
                            break;

                        case '=': // = center align 
                            horizontal = TextAlignment.Center;
                            break;

                        case '^': // ^ top align
                            vertical = VerticalAlignment.Top;
                            break;

                        case '~': // ~ bottom align
                            vertical = VerticalAlignment.Bottom;
                            break;

                        default:
                            rowspan = 1;
                            colspan = 1;
                            horizontal = null;
                            vertical = null;
                            goto endparse;
                    }
                }

                txt = txt.Substring(idx + 1);

            endparse:;
            }

            var sb = new StringBuilder();
            for (var i = 0; i < txt.Length; ++i)
            {
                var c = txt[i];

                if (c == '\\')
                {
                    if (++i < txt.Length)
                    {
                        if (txt[i] == 'n')
                            sb.Append("  \n"); // \n => linebreak
                        else
                            sb.Append('\\').Append(txt[i]);
                    }
                    else
                        sb.Append('\\');
                }
                else
                    sb.Append(c);
            }

            return new TableCellElement(new CTextBlockElement(engine.ParseGamutInline(sb.ToString().Trim())))
            {
                ColSpan = colspan,
                RowSpan = rowspan,
                Horizontal = horizontal,
                Vertical = vertical,
            };
        }

        private static string ExtractCoverBar(string txt)
        {
            if (txt[0] == '|')
                txt = txt.Substring(1);

            if (string.IsNullOrEmpty(txt))
                return txt;

            if (txt[txt.Length - 1] == '|')
                txt = txt.Substring(0, txt.Length - 1);

            return txt;
        }

        private static string ContinueToNum(string charSource, ref int idx)
        {
            var builder = new StringBuilder();

            for (; idx < charSource.Length; ++idx)
            {
                var c = charSource[idx];

                if ('0' <= c && c <= '9')
                    builder.Append(c);

                else break;
            }
            --idx;
            return builder.ToString();
        }
    }
}
