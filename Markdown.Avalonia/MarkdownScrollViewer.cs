using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Metadata;
using Avalonia.Platform;
using Avalonia.Styling;
using Markdown.Avalonia.Utils;
using System;
using System.IO;
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

        public static readonly AvaloniaProperty<string> AssetPathRootProperty =
            AvaloniaProperty.RegisterDirect<MarkdownScrollViewer, string>(
                nameof(AssetPathRoot),
                o => o.AssetPathRoot,
                (o, v) => o.AssetPathRoot = v);

        private ScrollViewer _viewer;

        public MarkdownScrollViewer()
        {
            Engine = new Markdown();

            this.InitializeComponent();

            bool nvl(bool? vl) => vl.HasValue && vl.Value;

            MarkdownStyleName =
                nvl(ThemeDetector.IsFluentUsed) ? nameof(MdStyle.FluentTheme) :
                nvl(ThemeDetector.IsDefaultUsed) ? nameof(MdStyle.DefaultTheme) :
                nameof(MdStyle.Standard);
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

        private void UpdateMarkdown()
        {
            var doc = Engine.Transform(Markdown ?? "");
            _viewer.Content = doc;
        }

        private Markdown _engine;
        public Markdown Engine
        {
            set
            {
                _engine = value;

                if (AssetPathRoot != null)
                    _engine.AssetPathRoot = AssetPathRoot;
            }
            get => _engine;
        }

        private string _AssetPathRoot;
        public string AssetPathRoot
        {
            set
            {
                if (value != null)
                {
                    Engine.AssetPathRoot = _AssetPathRoot = value;
                    UpdateMarkdown();
                }
            }
            get => _AssetPathRoot;
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
                    UpdateMarkdown();
                }
            }
        }

        private Uri _source;
        public Uri Source
        {
            get { return _source; }
            set
            {
                if (!value.IsAbsoluteUri)
                    throw new ArgumentException("it is not absolute.");

                _source = value;

                switch (_source.Scheme)
                {
                    case "http":
                    case "https":
                        using (var wc = new System.Net.WebClient())
                        using (var strm = new MemoryStream(wc.DownloadData(_source)))
                        using (var reader = new StreamReader(strm, true))
                            Markdown = reader.ReadToEnd();
                        break;

                    case "file":
                        using (var strm = File.OpenRead(_source.LocalPath))
                        using (var reader = new StreamReader(strm, true))
                            Markdown = reader.ReadToEnd();
                        break;

                    case "avares":
                        var loader = AvaloniaLocator.Current.GetService<IAssetLoader>();
                        using (var strm = loader.Open(_source))
                        using (var reader = new StreamReader(strm, true))
                            Markdown = reader.ReadToEnd();
                        break;

                    default:
                        throw new ArgumentException($"unsupport schema {_source.Scheme}");
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

                    ResetContent();
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
                    bool nvl(bool? vl) => vl.HasValue && vl.Value;

                    MarkdownStyle =
                        nvl(ThemeDetector.IsFluentUsed) ? MdStyle.FluentTheme :
                        nvl(ThemeDetector.IsDefaultUsed) ? MdStyle.DefaultTheme :
                        MdStyle.Standard;
                }
                else
                {
                    var prop = typeof(MarkdownStyle).GetProperty(_markdownStyleName);
                    if (prop == null) return;

                    MarkdownStyle = (Styles)prop.GetValue(null);
                }
            }
        }

        public void ResetContent()
        {
            var ctrl = _viewer.Content;
            _viewer.Content = null;
            Thread.MemoryBarrier();
            _viewer.Content = ctrl;
        }
    }
}
