using Avalonia.Controls;
using ColorTextBlock.Avalonia;
using Markdown.Avalonia.Parsers;
using Markdown.Avalonia.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Markdown.Avalonia.Plugins
{
    public class SetupInfo
    {
        internal const string BuiltinAsmNm = "Markdown.Avalonia.SyntaxHigh";
        internal const string BuiltinTpNm = "Markdown.Avalonia.SyntaxHigh.SyntaxHiglight";

        private bool _isFreezed = false;
        private bool _builtinCalled = false;
        private IPathResolver? _pathResolver;
        private IPathResolver? _defaultPathResolver = null;
        private bool _EnableNoteBlock = true;
        private bool _EnableTableBlock = true;
        private bool _EnableRuleExt = true;
        private bool _EnableTextAlignment = true;
        private bool _EnableStrikethrough = true;
        private bool _EnableListMarkerExt = true;
        private bool _EnableContainerBlockExt = true;
        private bool _EnableTextileInline = true;
        private List<IContainerBlockHandler> ContainerBlocks { get; }
        private IPathResolver DefaultPathResolver => _defaultPathResolver ??= new DefaultPathResolver();

        internal List<IBlockOverride> BlockOverrides { get; }
        internal List<BlockParser> TopBlock { get; }
        internal List<BlockParser> Block { get; }
        internal List<InlineParser> Inline { get; }
        internal List<IStyleEdit> StyleEdits { get; }
        internal List<IImageResolver> ImageResolvers { get; }
        internal IContainerBlockHandler ContainerBlock { get; }
        internal IPathResolver PathResolver => _pathResolver ?? DefaultPathResolver;


        public SetupInfo()
        {
            BlockOverrides = new();
            TopBlock = new();
            Block = new();
            Inline = new();
            StyleEdits = new();
            ImageResolvers = new();
            ContainerBlocks = new();
            ContainerBlock = new ChainContainerBlockHandler(this);
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

        public void SetOnce(IPathResolver resolver)
        {
            CheckChangeable();

            if (_pathResolver is not null)
            {
                throw new InvalidOperationException("IPathResolver is already set. Please check Markdown.Avalonia plugins");
            }

            _pathResolver = resolver;
        }

        public void Register(IContainerBlockHandler handler)
        {
            CheckChangeable();

            ContainerBlocks.Add(handler);
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
            }
            catch
            {
                return this;
            }

            var type = asm.GetType(BuiltinTpNm);
            if (type != null && Activator.CreateInstance(type) is IMdAvPlugin plugin)
            {
                plugin.Setup(this);
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

        public void Freeze()
        {
            _isFreezed = true;
        }

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

            public override IEnumerable<Control> Convert(
                string text,
                Match firstMatch,
                ParseStatus status,
                IMarkdownEngine engine,
                out int parseTextBegin, out int parseTextEnd)
            => _overrider.Convert(text, firstMatch, status, engine, out parseTextBegin, out parseTextEnd);
        }

        class ChainContainerBlockHandler : IContainerBlockHandler
        {
            private SetupInfo _info;

            public ChainContainerBlockHandler(SetupInfo info)
            {
                _info = info;
            }

            public Border? ProvideControl(string assetPathRoot, string blockName, string lines)
            {
                foreach (var handler in _info.ContainerBlocks)
                    if (handler.ProvideControl(assetPathRoot, blockName, lines) is Border border)
                        return border;

                return null;
            }
        }
    }
}
