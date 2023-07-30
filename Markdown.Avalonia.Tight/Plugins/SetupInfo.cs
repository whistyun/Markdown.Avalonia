using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ColorTextBlock.Avalonia;
using Markdown.Avalonia.Parsers;
using Markdown.Avalonia.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

namespace Markdown.Avalonia.Plugins
{
    public class SetupInfo
    {
        internal const string BuiltinAsmNm = "Markdown.Avalonia.SyntaxHigh";
        internal const string BuiltinTpNm = "Markdown.Avalonia.SyntaxHigh.SyntaxHighlight";

        private bool _isFreezed = false;
        private bool _builtinCalled = false;

        private ICommand? _overwriteHyperlink;
        private ICommand? _command;
        private ICommand? _defaultHyperlink;

        private IContainerBlockHandler? _overwriteHandler;
        private IContainerBlockHandler? _containerBlock;

        private IPathResolver? _pathResolver;
        private IPathResolver? _defaultPathResolver;

        private ImageLoader _imageLoader;

        private bool _EnableNoteBlock = true;
        private bool _EnableTableBlock = true;
        private bool _EnableRuleExt = true;
        private bool _EnableTextAlignment = true;
        private bool _EnableStrikethrough = true;
        private bool _EnableListMarkerExt = true;
        private bool _EnableContainerBlockExt = true;
        private bool _EnableTextileInline = true;
        private bool _EnablePreRenderingCodeBlock = false;

        internal List<IBlockOverride> BlockOverrides { get; }
        internal List<BlockParser> TopBlock { get; }
        internal List<BlockParser> Block { get; }
        internal List<InlineParser> Inline { get; }
        internal List<IStyleEdit> StyleEdits { get; }
        internal List<IImageResolver> ImageResolvers { get; }
        internal ICommand HyperlinkCommand => _overwriteHyperlink ?? _command ?? (_defaultHyperlink ??= new DefaultHyperlinkCommand());
        internal IContainerBlockHandler? ContainerBlock => _overwriteHandler ?? _containerBlock;
        internal IPathResolver PathResolver => _pathResolver ?? (_defaultPathResolver ??= new DefaultPathResolver());


        public SetupInfo()
        {
            _imageLoader = new(this);

            BlockOverrides = new();
            TopBlock = new();
            Block = new();
            Inline = new();
            StyleEdits = new();
            ImageResolvers = new();
        }


        public bool EnableNoteBlock
        {
            get => _EnableNoteBlock;
            set
            {
                CheckChangeable();
                _EnableNoteBlock = value;
            }
        }

        public bool EnableTableBlock
        {
            get => _EnableTableBlock;
            set
            {
                CheckChangeable();
                _EnableTableBlock = value;
            }
        }

        public bool EnableRuleExt
        {
            get => _EnableRuleExt;
            set
            {
                CheckChangeable();
                _EnableRuleExt = value;
            }
        }

        public bool EnableTextAlignment
        {
            get => _EnableTextAlignment;
            set
            {
                CheckChangeable();
                _EnableTextAlignment = value;
            }
        }

        public bool EnableStrikethrough
        {
            get => _EnableStrikethrough;
            set
            {
                CheckChangeable();
                _EnableStrikethrough = value;
            }
        }

        public bool EnableListMarkerExt
        {
            get => _EnableListMarkerExt;
            set
            {
                CheckChangeable();
                _EnableListMarkerExt = value;
            }
        }

        public bool EnableContainerBlockExt
        {
            get => _EnableContainerBlockExt;
            set
            {
                CheckChangeable();
                _EnableContainerBlockExt = value;
            }
        }

        public bool EnableTextileInline
        {
            get => _EnableTextileInline;
            set
            {
                CheckChangeable();
                _EnableTextileInline = value;
            }
        }

        public bool EnablePreRenderingCodeBlock
        {
            get => _EnablePreRenderingCodeBlock;
            set
            {
                CheckChangeable();
                _EnablePreRenderingCodeBlock = value;
            }
        }


        public void Register(IBlockOverride overrider)
        {
            CheckChangeable();
            BlockOverrides.Add(overrider);
        }

        public void RegisterTop(BlockParser parser)
        {
            CheckChangeable();
            TopBlock.Add(parser);
        }

        public void RegisterSecond(BlockParser parser)
        {
            CheckChangeable();
            Block.Add(parser);
        }

        public void Register(InlineParser parser)
        {
            CheckChangeable();
            Inline.Add(parser);
        }

        public void Register(IStyleEdit editor)
        {
            CheckChangeable();
            StyleEdits.Add(editor);
        }

        public void SetOnce(ICommand command)
        {
            CheckChangeable();

            if (_command is not null)
            {
                throw new InvalidOperationException("IContainerBlockHandler is already set. Please check Markdown.Avalonia plugins");
            }

            _command = command;
        }

        public void SetOnce(IContainerBlockHandler handler)
        {
            CheckChangeable();

            if (_containerBlock is not null)
            {
                throw new InvalidOperationException("IContainerBlockHandler is already set. Please check Markdown.Avalonia plugins");
            }

            _containerBlock = handler;
        }

        public void SetOnce(IPathResolver resolver)
        {
            CheckChangeable();

            if (_pathResolver is not null)
            {
                throw new InvalidOperationException("IPathResolver is already set. Please check Markdown.Avalonia plugins");
            }

            _pathResolver = resolver;
        }

        public void Register(IImageResolver resolver)
        {
            CheckChangeable();

            ImageResolvers.Add(resolver);
        }

        internal SetupInfo Builtin()
        {
            if (_builtinCalled)
                return this;

            CheckChangeable();

            Assembly asm;
            try
            {
                asm = Assembly.Load(BuiltinAsmNm);

                var type = asm.GetType(BuiltinTpNm);
                if (type != null && Activator.CreateInstance(type) is IMdAvPlugin plugin)
                {
                    plugin.Setup(this);
                }
            }
            catch
            {
                return this;
            }

            _builtinCalled = true;
            return this;
        }

        internal BlockParser Override(BlockParser parser)
        {
            var overrider = BlockOverrides.FirstOrDefault(b => b.ParserName == parser.Name);

            if (overrider is null)
                return parser;

            return new BlockParserOverride(parser.Pattern, parser.Name, overrider);
        }

        internal void Overwrite(ICommand? hyperlink)
        {
            _overwriteHyperlink = hyperlink;
        }

        internal void Overwrite(IContainerBlockHandler? handler)
        {
            _overwriteHandler = handler;
        }

#pragma warning disable CS0618
        internal void Overwrite(IBitmapLoader? loader)
#pragma warning restore CS0618
        {
            _imageLoader.BitmapLoader = loader;
        }

        public void Freeze()
        {
            _isFreezed = true;
        }


        public CImage LoadImage(string urlTxt) => _imageLoader.Load(urlTxt);


        internal void CheckChangeable()
        {
            if (_isFreezed)
                throw new InvalidOperationException();
        }


        class BlockParserOverride : BlockParser
        {
            private IBlockOverride _overrider;
            public BlockParserOverride(Regex pattern, string name, IBlockOverride overrider) : base(pattern, name)
            {
                _overrider = overrider;
            }

            public override IEnumerable<Control>? Convert(
                string text,
                Match firstMatch,
                ParseStatus status,
                IMarkdownEngine engine,
                out int parseTextBegin, out int parseTextEnd)
            => _overrider.Convert(text, firstMatch, status, engine, out parseTextBegin, out parseTextEnd);
        }

        class ImageLoader
        {
            private SetupInfo _setupInfo;
            private Dictionary<string, WeakReference<IImage>> _cache;
            private Lazy<Bitmap> _imageNotFound;

#pragma warning disable CS0618
            public IBitmapLoader? BitmapLoader { get; set; }
#pragma warning restore CS0618


            public ImageLoader(SetupInfo info)
            {
                _setupInfo = info;
                _cache = new();

                _imageNotFound = new Lazy<Bitmap>(() =>
                {
                    using var strm = AssetLoader.Open(new Uri($"avares://Markdown.Avalonia/Assets/ImageNotFound.bmp"));
                    return new Bitmap(strm);
                });

            }

            public CImage Load(string urlTxt)
            {
                if (_cache.TryGetValue(urlTxt, out var bitmapRef) && bitmapRef.TryGetTarget(out var cachedBitmap))
                {
                    return new CImage(cachedBitmap ?? _imageNotFound.Value);
                }
                else
                {
#pragma warning disable CS0618
                    var imageTask = BitmapLoader is not null ?
                        Task.Run(() => (IImage?)BitmapLoader?.Get(urlTxt)) :
#pragma warning restore CS0618
                        LoadImageByPlugin(urlTxt);

                    return new CImage(imageTask, _imageNotFound.Value);
                }
            }

            private async Task<IImage?> LoadImageByPlugin(string urlTxt)
            {
                foreach (var key in _cache.Keys.ToArray())
                {
                    if (_cache[key].TryGetTarget(out var _))
                        _cache.Remove(key);
                }


                var streamTask = _setupInfo.PathResolver.ResolveImageResource(urlTxt);
                if (streamTask is null)
                {
                    _cache[urlTxt] = new WeakReference<IImage>(_imageNotFound.Value);
                    return null;
                }

                using var stream = await streamTask;
                if (stream is null)
                {
                    _cache[urlTxt] = new WeakReference<IImage>(_imageNotFound.Value);
                    return null;
                }

                Stream seekableStream;
                if (!stream.CanSeek)
                {
                    seekableStream = new MemoryStream();
                    await stream.CopyToAsync(seekableStream);
                }
                else
                {
                    seekableStream = stream;
                }

                var reuseStream = new UnclosableStream(seekableStream);

                foreach (var imageResolver in _setupInfo.ImageResolvers)
                {
                    reuseStream.Position = 0;
                    var image = await imageResolver.Load(reuseStream);

                    if (image is not null)
                    {
                        _cache[urlTxt] = new WeakReference<IImage>(image);
                        return image;
                    }
                }

                try
                {
                    var image = new Bitmap(reuseStream);
                    _cache[urlTxt] = new WeakReference<IImage>(image);
                    return image;
                }
                catch
                {
                    _cache[urlTxt] = new WeakReference<IImage>(_imageNotFound.Value);
                    return null;
                }
            }
        }
    }
}
