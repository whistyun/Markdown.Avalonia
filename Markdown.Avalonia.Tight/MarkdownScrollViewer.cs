﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;
using Avalonia.Platform;
using Avalonia.Styling;
using ColorDocument.Avalonia;
using ColorDocument.Avalonia.DocumentElements;
using Markdown.Avalonia.Plugins;
using Markdown.Avalonia.StyleCollections;
using Markdown.Avalonia.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using MdStyle = Markdown.Avalonia.MarkdownStyle;

namespace Markdown.Avalonia
{
    public class MarkdownScrollViewer : Control
    {
        public static readonly DirectProperty<MarkdownScrollViewer, Uri?> SourceDirectProperty =
            AvaloniaProperty.RegisterDirect<MarkdownScrollViewer, Uri?>(
                nameof(Source),
                o => o.Source,
                (o, v) => o.Source = v);

        public static readonly AvaloniaProperty<Uri?> SourceProperty = SourceDirectProperty;

        private static readonly DirectProperty<MarkdownScrollViewer, string?> MarkdownDirectProperty =
            AvaloniaProperty.RegisterDirect<MarkdownScrollViewer, string?>(
                nameof(Markdown),
                o => o.Markdown,
                (o, v) => o.Markdown = v);

        public static readonly AvaloniaProperty<string?> MarkdownProperty = MarkdownDirectProperty;

        private static readonly AvaloniaProperty<IStyle> MarkdownStyleProperty =
            AvaloniaProperty.RegisterDirect<MarkdownScrollViewer, IStyle>(
                nameof(MarkdownStyle),
                o => o.MarkdownStyle,
                (o, v) => o.MarkdownStyle = v);

        public static readonly AvaloniaProperty<string?> MarkdownStyleNameProperty =
            AvaloniaProperty.RegisterDirect<MarkdownScrollViewer, string?>(
                nameof(MarkdownStyleName),
                o => o.MarkdownStyleName,
                (o, v) => o.MarkdownStyleName = v);

        public static readonly AvaloniaProperty<string?> AssetPathRootProperty =
            AvaloniaProperty.RegisterDirect<MarkdownScrollViewer, string?>(
                nameof(AssetPathRoot),
                o => o.AssetPathRoot,
                (o, v) => o.AssetPathRoot = v);

        public static readonly StyledProperty<bool> SaveScrollValueWhenContentUpdatedProperty =
            AvaloniaProperty.Register<MarkdownScrollViewer, bool>(
                nameof(SaveScrollValueWhenContentUpdated),
                defaultValue: false);

        public static readonly AvaloniaProperty<Vector> ScrollValueProperty =
            AvaloniaProperty.RegisterDirect<MarkdownScrollViewer, Vector>(
                nameof(ScrollValue),
                owner => owner.ScrollValue,
                (owner, v) => owner.ScrollValue = v);


        private static readonly HttpClient s_httpclient = new();
        private readonly ScrollViewer _viewer;
        private SetupInfo _setup;
        private DocumentElement? _document;

        public MarkdownScrollViewer()
        {
            _plugins = new MdAvPlugins();
            _setup = Plugins.Info;

            var md = new Markdown();
            md.CascadeResources.SetParent(this);
            md.UseResource = _useResource;
            md.Plugins = _plugins;

            _engine = md;

            if (nvl(ThemeDetector.IsFluentAvaloniaUsed))
            {
                _markdownStyleName = nameof(MdStyle.FluentAvalonia);
                _markdownStyle = MdStyle.FluentAvalonia;
            }
            else if (nvl(ThemeDetector.IsFluentUsed))
            {
                _markdownStyleName = nameof(MdStyle.FluentTheme);
                _markdownStyle = MdStyle.FluentTheme;
            }
            else if (nvl(ThemeDetector.IsSimpleUsed))
            {
                _markdownStyleName = nameof(MdStyle.SimpleTheme);
                _markdownStyle = MdStyle.SimpleTheme;
            }
            else
            {
                _markdownStyleName = nameof(MdStyle.Standard);
                _markdownStyle = MdStyle.Standard;
            }
            Styles.Insert(0, _markdownStyle);

            _viewer = new ScrollViewer()
            {
                // TODO: ScrollViewer does not seem to take Padding into account in 11.0.0-preview1
                Padding = new Thickness(0),
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            };

            ((ISetLogicalParent)_viewer).SetParent(this);
            VisualChildren.Add(_viewer);
            LogicalChildren.Add(_viewer);

            EditStyle(_markdownStyle);

            static bool nvl(bool? vl) => vl.HasValue && vl.Value;

            _viewer.ScrollChanged += (s, e) => OnScrollChanged();
        }

        public event HeaderScrolled? HeaderScrolled;
        private List<HeaderRect>? _headerRects;
        private HeaderScrolledEventArgs? _eventArgs;

        private void OnViewportSizeChanged(object? obj, EventArgs arg)
        {
            _headerRects = null;
        }

        private void OnScrollChanged()
        {
            if (HeaderScrolled is null) return;

            double offsetY = _viewer.Offset.Y;
            double viewHeight = _viewer.Viewport.Height;

            if (_headerRects is null)
            {
                if (_document is null) return;

                _headerRects = new List<HeaderRect>();
                foreach (var doc in _document.Children.OfType<HeaderElement>())
                {
                    var t = doc.GetRect();
                    var rect = new Rect(t.Left, t.Top + offsetY, t.Width, t.Height);
                    _headerRects.Add(new HeaderRect(rect, doc));
                }
            }

            var tree = new Header?[5];
            var viewing = new List<Header>();

            tree[0] = _headerRects.Where(rct => rct.Header.Level == 1)
                                  .Select(rct => CreateHeader(rct))
                                  .FirstOrDefault();

            foreach (var headerRect in _headerRects)
            {
                var boundY = headerRect.BaseBound.Bottom - offsetY;

                if (boundY < 0)
                {
                    var header = CreateHeader(headerRect);
                    tree[header.Level - 1] = header;

                    for (var i = header.Level; i < tree.Length; ++i)
                        tree[i] = null;
                }
                else if (0 <= boundY && boundY < viewHeight)
                {
                    viewing.Add(CreateHeader(headerRect));
                }
                else break;
            }

            var newEvArg = new HeaderScrolledEventArgs(tree.OfType<Header>().ToList(), viewing);
            if (_eventArgs != newEvArg)
            {
                _eventArgs = newEvArg;
                HeaderScrolled(this, _eventArgs);
            }

            static Header CreateHeader(HeaderRect headerRect)
            {
                var header = headerRect.Header;
                return new Header(header.Level, header.Text);
            }
        }


        private void EditStyle(IStyle mdstyle)
        {
            if (mdstyle is INamedStyle nameStyle && !nameStyle.IsEditted
             && mdstyle is Styles styles)
            {
                foreach (var edit in _setup.StyleEdits)
                    edit.Edit(nameStyle.Name, styles);

                nameStyle.IsEditted = true;
            }
        }

        private void UpdateMarkdown()
        {
            if (_viewer.Content is null && String.IsNullOrEmpty(Markdown))
                return;

            _document = _engine.TransformElement(Markdown ?? "");
            var doc = _document.Control;

            var ofst = _viewer.Offset;

            if (_viewer.Content is Control contentControl)
            {
                contentControl.SizeChanged -= OnViewportSizeChanged;
            }

            _viewer.Content = doc;
            if (doc is not null)
            {
                doc.SizeChanged += OnViewportSizeChanged;
            }

            _headerRects = null;

            if (SaveScrollValueWhenContentUpdated)
                _viewer.Offset = ofst;
        }

        private IMarkdownEngine2 _engine;
        public IMarkdownEngineBase Engine
        {
            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(Engine));

                if (value is IMarkdownEngine engine1)
                    _engine = engine1.Upgrade();
                else if (value is IMarkdownEngine2 engine)
                    _engine = engine;
                else
                    throw new ArgumentException();

                _engine.CascadeResources.SetParent(this);
                _engine.UseResource = _useResource;
                _engine.Plugins = _plugins;

                if (AssetPathRoot is not null)
                    _engine.AssetPathRoot = AssetPathRoot;
            }
            get => _engine;
        }

        private string? _AssetPathRoot;
        public string? AssetPathRoot
        {
            set
            {
                if (value is not null)
                {
                    _engine.AssetPathRoot = _AssetPathRoot = value;
                    UpdateMarkdown();
                }
            }
            get => _AssetPathRoot;
        }

        public bool SaveScrollValueWhenContentUpdated
        {
            set { SetValue(SaveScrollValueWhenContentUpdatedProperty, value); }
            get { return GetValue(SaveScrollValueWhenContentUpdatedProperty); }
        }

        public Vector ScrollValue
        {
            set { _viewer.Offset = value; }
            get { return _viewer.Offset; }
        }

        [Content]
        public string? HereMarkdown
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
                    int lastIdtCnt = TextUtil.CountIndent(lines.Last());
                    // count full indent
                    int someIdtCnt = lines
                        .Where(line => !String.IsNullOrWhiteSpace(line))
                        .Select(line => TextUtil.CountIndent(line))
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

        private string? _markdown;
        public string? Markdown
        {
            get { return _markdown; }
            set
            {
                if (SetAndRaise(MarkdownDirectProperty, ref _markdown, value))
                {
                    UpdateMarkdown();
                }
            }
        }

        private Uri? _source;
        public Uri? Source
        {
            get { return _source; }
            set
            {
                if (!SetAndRaise(SourceDirectProperty, ref _source, value))
                    return;

                if (value is null)
                {
                    _source = value;
                    Markdown = null;
                    return;
                }

                if (!value.IsAbsoluteUri)
                    throw new ArgumentException("it is not absolute.");

                _source = value;

                switch (_source.Scheme)
                {
                    case "http":
                    case "https":
                        using (var res = s_httpclient.GetAsync(_source).Result)
                        using (var strm = res.Content.ReadAsStreamAsync().Result)
                        using (var reader = new StreamReader(strm, true))
                            Markdown = reader.ReadToEnd();
                        break;

                    case "file":
                        using (var strm = File.OpenRead(_source.LocalPath))
                        using (var reader = new StreamReader(strm, true))
                            Markdown = reader.ReadToEnd();
                        break;

                    case "avares":
                        using (var strm = AssetLoader.Open(_source))
                        using (var reader = new StreamReader(strm, true))
                            Markdown = reader.ReadToEnd();
                        break;

                    default:
                        throw new ArgumentException($"unsupport schema {_source.Scheme}");
                }

                AssetPathRoot =
                    value.Scheme == "file" ?
                    value.LocalPath :
                    value.AbsoluteUri;
            }
        }

        private IStyle _markdownStyle;
        public IStyle MarkdownStyle
        {
            get { return _markdownStyle; }
            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(MarkdownStyle));

                if (_markdownStyle != value)
                {
                    EditStyle(value);

                    if (_markdownStyle is not null)
                        Styles.Remove(_markdownStyle);

                    Styles.Insert(0, value);

                    //ResetContent();
                }

                _markdownStyle = value;
            }
        }

        private string? _markdownStyleName;
        public string? MarkdownStyleName
        {
            get { return _markdownStyleName; }
            set
            {
                _markdownStyleName = value;

                if (_markdownStyleName is null)
                {
                    MarkdownStyle =
                        nvl(ThemeDetector.IsFluentAvaloniaUsed) ? MdStyle.FluentAvalonia :
                        nvl(ThemeDetector.IsFluentUsed) ? MdStyle.FluentTheme :
                        nvl(ThemeDetector.IsSimpleUsed) ? MdStyle.SimpleTheme :
                        MdStyle.Standard;
                }
                else
                {
                    var prop = typeof(MarkdownStyle).GetProperty(_markdownStyleName);
                    if (prop is null) return;

                    var propVal = prop.GetValue(null) as IStyle;
                    if (propVal is null) return;

                    MarkdownStyle = propVal;
                }

                static bool nvl(bool? vl) => vl.HasValue && vl.Value;
            }
        }

        private MdAvPlugins _plugins;
        public MdAvPlugins Plugins
        {
            get => _plugins;
            set
            {
                _plugins = _engine.Plugins = value;
                _setup = Plugins.Info;

                EditStyle(MarkdownStyle);
                UpdateMarkdown();
            }
        }

        private bool _useResource;
        public bool UseResource
        {
            get => _useResource;
            set
            {
                _engine.UseResource = value;
                _useResource = value;
                UpdateMarkdown();
            }
        }

        //public void ResetContent()
        //{
        //    var ctrl = _viewer.Content;
        //    _viewer.Content = null;
        //    Thread.MemoryBarrier();
        //    _viewer.Content = ctrl;
        //}


        class HeaderRect
        {
            public Rect BaseBound { get; }
            public HeaderElement Header { get; }

            public HeaderRect(Rect bound, HeaderElement header)
            {
                BaseBound = bound;
                Header = header;
            }
        }
    }
}
