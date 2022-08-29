using System;
using System.Text;
using Avalonia.Media;
using Avalonia.Layout;

namespace Markdown.Avalonia.Tables
{
    class TextileTableCell : ITableCell
    {
        public int ColumnIndex { set; get; }

        public string? RawText { get; }
        public string? Text { get; }
        public int RowSpan { set; get; }
        public int ColSpan { set; get; }
        public TextAlignment? Horizontal { set; get; }
        public VerticalAlignment? Vertical { set; get; }

        public TextileTableCell(string? txt)
        {
            RawText = txt;
            RowSpan = 1;
            ColSpan = 1;
            Horizontal = null;
            Vertical = null;

            if (txt is null) return;

            txt = ParseFormatFrom(txt);

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
            Text = sb.ToString();
        }

        private string ParseFormatFrom(string txt)
        {
            int idx = txt.IndexOf('.');

            if (idx == -1)
            {
                return txt.Trim();
            }
            else
            {
                var styleTxt = txt.Substring(0, idx);

                for (var i = 0; i < styleTxt.Length; ++i)
                {
                    var c = styleTxt[i];

                    switch (c)
                    {
                        case '/': // /2 rowspan
                            ++i;
                            var numTxt = ContinueToNum(styleTxt, ref i);
                            if (numTxt.Length == 0) goto default;
                            RowSpan = Int32.Parse(numTxt);

                            break;

                        case '\\': // \2 colspan
                            ++i;
                            numTxt = ContinueToNum(styleTxt, ref i);
                            if (numTxt.Length == 0) goto default;
                            ColSpan = Int32.Parse(numTxt);
                            break;

                        case '<': // < left align
                            Horizontal = TextAlignment.Left;
                            break;

                        case '>': // > right align
                            Horizontal = TextAlignment.Right;
                            break;

                        case '=': // = center align 
                            Horizontal = TextAlignment.Center;
                            break;

                        case '^': // ^ top align
                            Vertical = VerticalAlignment.Top;
                            break;

                        case '~': // ~ bottom align
                            Vertical = VerticalAlignment.Bottom;
                            break;

                        default:
                            RowSpan = 1;
                            ColSpan = 1;
                            Horizontal = null;
                            Vertical = null;
                            return txt.Trim();
                    }
                }
                return txt.Substring(idx + 1).Trim();
            }
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
