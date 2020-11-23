using System;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Metadata;

namespace Markdown.Avalonia
{
    public class MarkdownScrollViewer : ScrollViewer, IUriContext
    {
        public static readonly AvaloniaProperty<string> MarkdownProperty =
            AvaloniaProperty.RegisterDirect<MarkdownScrollViewer, string>(
                nameof(Markdown),
                o => o.Markdown,
                (o, v) => o.Markdown = v);

        public Markdown Engine
        {
            set;
            get;
        }

        public Uri BaseUri
        {
            set { Engine.BaseUri = value; }
            get => Engine.BaseUri;
        }

        public string AssetPathRoot
        {
            set { Engine.AssetPathRoot = value; }
            get => Engine.AssetPathRoot;
        }

        [Content]
        public string HereMarkdown
        {
            get { return Markdown; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    Markdown = value;
                }
                else
                {
                    // like PHP's flexible_heredoc_nowdoc_syntaxes,
                    // The indentation of the closing tag dictates 
                    // the amount of whitespace to strip from each line 
                    var lines = Regex.Split(value, "\r\n|\r|\n", RegexOptions.Multiline);

                    int CountIndent(string line)
                    {
                        var count = 0;
                        foreach (var c in line)
                        {
                            if (c == ' ') count += 1;
                            else if (c == '\t')
                            {
                                // In default in vs, tab is treated as four-spaces.
                                count = ((count >> 2) + 1) << 2;
                            }
                            else break;
                        }
                        return count;
                    }


                    // count last line indent
                    int lastIdtCnt = CountIndent(lines.Last());
                    // count full indent
                    int someIdtCnt = lines
                        .Where(line => !String.IsNullOrWhiteSpace(line))
                        .Select(line => CountIndent(line))
                        .Min();

                    var indentCount = Math.Max(lastIdtCnt, someIdtCnt);

                    Markdown = String.Join(
                        "\n",
                        lines
                            // skip first blank line
                            .Skip(String.IsNullOrWhiteSpace(lines[0]) ? 1 : 0)
                            // strip indent
                            .Select(line =>
                            {
                                var realIdx = 0;
                                var viewIdx = 0;

                                while (viewIdx < indentCount && realIdx < line.Length)
                                {
                                    var c = line[realIdx];
                                    if (c == ' ')
                                    {
                                        realIdx += 1;
                                        viewIdx += 1;
                                    }
                                    else if (c == '\t')
                                    {
                                        realIdx += 1;
                                        viewIdx = ((viewIdx >> 2) + 1) << 2;
                                    }
                                    else break;
                                }

                                return line.Substring(realIdx);
                            })
                        );
                }
            }
        }

        private string _markdown;
        public string Markdown
        {
            get { return (string)GetValue(MarkdownProperty); }
            set
            {
                SetValue(MarkdownProperty, value);
                if (SetAndRaise(MarkdownProperty, ref _markdown, value))
                {
                    var doc = Engine.Transform(value ?? "");
                    Content = doc;
                }
            }
        }

        public MarkdownScrollViewer()
        {
            Engine = new Markdown();
        }
    }
}
