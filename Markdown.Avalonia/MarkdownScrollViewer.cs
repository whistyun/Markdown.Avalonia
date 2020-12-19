using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Metadata;
using Avalonia.Styling;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using MdStyle = Markdown.Avalonia.MarkdownStyle;

namespace Markdown.Avalonia
{
    public class MarkdownScrollViewer : Control
    {
        public static readonly AvaloniaProperty<string> MarkdownProperty =
            AvaloniaProperty.RegisterDirect<MarkdownScrollViewer, string>(
                nameof(Markdown),
                o => o.Markdown,
                (o, v) => o.Markdown = v);

        public static readonly AvaloniaProperty<Styles> MarkdownStyleProperty =
            AvaloniaProperty.RegisterDirect<MarkdownScrollViewer, Styles>(
                nameof(MarkdownStyle),
                o => o.MarkdownStyle,
                (o, v) => o.MarkdownStyle = v);

        public static readonly AvaloniaProperty<string> MarkdownStyleNameProperty =
            AvaloniaProperty.RegisterDirect<MarkdownScrollViewer, string>(
                nameof(MarkdownStyleName),
                o => o.MarkdownStyleName,
                (o, v) => o.MarkdownStyleName = v);

        private ScrollViewer _viewer;

        public MarkdownScrollViewer()
        {
            Engine = new Markdown();

            this.InitializeComponent();

            MarkdownStyleName = nameof(MdStyle.Standard2);
        }

        private void InitializeComponent()
        {
            _viewer = new ScrollViewer()
            {
                Padding = new Thickness(5),
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            };

            VisualChildren.Add(_viewer);
            LogicalChildren.Add(_viewer);
        }

        public Markdown Engine
        {
            set;
            get;
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

                    // count last line indent
                    int lastIdtCnt = IndentUtil.CountIndent(lines.Last());
                    // count full indent
                    int someIdtCnt = lines
                        .Where(line => !String.IsNullOrWhiteSpace(line))
                        .Select(line => IndentUtil.CountIndent(line))
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
            get { return _markdown; }
            set
            {
                if (SetAndRaise(MarkdownProperty, ref _markdown, value))
                {
                    var doc = Engine.Transform(value ?? "");
                    _viewer.Content = doc;
                }
            }
        }

        private Styles _markdownStyle;
        public Styles MarkdownStyle
        {
            get { return _markdownStyle; }
            set
            {
                if (_markdownStyle != value)
                {
                    if (_markdownStyle != null)
                        Styles.Remove(_markdownStyle);

                    Styles.Insert(0, value);

                    // i have no idea to reflect style changed
                    Rebuild();
                }

                _markdownStyle = value;
            }
        }

        private string _markdownStyleName;
        public string MarkdownStyleName
        {
            get { return _markdownStyleName; }
            set
            {
                _markdownStyleName = value;

                if (_markdownStyleName is null)
                {
                    MarkdownStyle = MdStyle.Standard2;
                }
                else
                {
                    var prop = typeof(MarkdownStyle).GetProperty(_markdownStyleName);
                    if (prop == null) return;

                    MarkdownStyle = (Styles)prop.GetValue(null);
                }
            }
        }


        public void Rebuild()
        {
            var ctrl = _viewer.Content;
            _viewer.Content = null;
            Thread.MemoryBarrier();
            _viewer.Content = ctrl;
        }
    }
}
