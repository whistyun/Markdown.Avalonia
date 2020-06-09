using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

#if MIG_FREE
using MdStyle = Markdown.Xaml.MarkdownStyle;
namespace Markdown.Xaml
#else
using MdStyle = MdXaml.MarkdownStyle;
namespace MdXaml
#endif
{
    [ContentProperty(nameof(HereMarkdown))]
    public class MarkdownScrollViewer : FlowDocumentScrollViewer, IUriContext
    {
        public static readonly DependencyProperty MarkdownProperty =
            DependencyProperty.Register(
                nameof(Markdown),
                typeof(string),
                typeof(MarkdownScrollViewer),
                new PropertyMetadata("", UpdateMarkdown));


        public static readonly DependencyProperty MarkdownStyleProperty =
            DependencyProperty.Register(
                nameof(MarkdownStyle),
                typeof(Style),
                typeof(MarkdownScrollViewer),
                new PropertyMetadata(null, UpdateStyle));

        public static readonly DependencyProperty MarkdownStyleNameProperty =
            DependencyProperty.Register(
            nameof(MarkdownStyleName),
            typeof(string),
            typeof(MarkdownScrollViewer),
            new PropertyMetadata(null, UpdateStyleName));

        private static void UpdateMarkdown(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MarkdownScrollViewer owner)
            {
                var doc = owner.Engine.Transform((string)e.NewValue ?? "");
                owner.SetCurrentValue(DocumentProperty, doc);
            }
        }

        private static void UpdateStyle(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MarkdownScrollViewer owner)
            {
                owner.Engine.DocumentStyle = (Style)e.NewValue;
            }
        }

        private static void UpdateStyleName(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MarkdownScrollViewer owner)
            {
                var newName = (string)e.NewValue;

                if (newName == null) return;

                var prop = typeof(MarkdownStyle).GetProperty(newName);
                if (prop == null) return;

                owner.MarkdownStyle = (Style)prop.GetValue(null);
            }
        }

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

        public string Markdown
        {
            get { return (string)GetValue(MarkdownProperty); }
            set { SetValue(MarkdownProperty, value); }
        }

        public Style MarkdownStyle
        {
            get { return (Style)GetValue(MarkdownStyleProperty); }
            set { SetValue(MarkdownStyleProperty, value); }
        }

        public string MarkdownStyleName
        {
            get { return (string)GetValue(MarkdownStyleNameProperty); }
            set { SetValue(MarkdownStyleNameProperty, value); }
        }

        public MarkdownScrollViewer()
        {
            Engine = new Markdown();
            MarkdownStyleName = nameof(MdStyle.Standard);
        }
    }
}
