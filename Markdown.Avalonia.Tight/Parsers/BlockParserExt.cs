using Avalonia.Controls;
using ColorDocument.Avalonia;
using ColorDocument.Avalonia.DocumentElements;
using ColorTextBlock.Avalonia;
using Markdown.Avalonia.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Markdown.Avalonia.Parsers
{
    public static class BlockParserExt
    {
        public static BlockParser2 Upgrade(this BlockParser parser)
        {
            return parser is BlockParser2 parser2 ? parser2 : new BlockParserUpg(parser);
        }

        class BlockParserUpg : BlockParser2
        {
            private BlockParser _parser;

            public BlockParserUpg(BlockParser parser) : base(parser.Pattern, parser.Name)
            {
                _parser = parser;
            }

            public override IEnumerable<DocumentElement>? Convert2(string text, Match firstMatch, ParseStatus status, IMarkdownEngine2 engine2, out int parseTextBegin, out int parseTextEnd)
            {
                IMarkdownEngine engine = engine2 is IMarkdownEngine e ? e : new MarkdownEngineDng(engine2);

                var rtn = _parser.Convert(text, firstMatch, status, engine, out parseTextBegin, out parseTextEnd);
                return rtn.Select(c => new UnBlockElement(c));
            }
        }

        class MarkdownEngineDng : IMarkdownEngine
        {
            private IMarkdownEngine2 _engine;

            public string AssetPathRoot { get => _engine.AssetPathRoot; set => _engine.AssetPathRoot = value; }
            public ICommand? HyperlinkCommand { get => _engine.HyperlinkCommand; set => _engine.HyperlinkCommand = value; }
            public IBitmapLoader? BitmapLoader { 
                get => throw new NotImplementedException();
                set => throw new NotImplementedException(); }
            public IContainerBlockHandler? ContainerBlockHandler { get => _engine.ContainerBlockHandler; set => _engine.ContainerBlockHandler = value; }
            public MdAvPlugins Plugins { get => _engine.Plugins; set => _engine.Plugins = value; }
            public bool UseResource { get => _engine.UseResource; set => _engine.UseResource = value; }
            public CascadeDictionary CascadeResources => _engine.CascadeResources;
            public IResourceDictionary Resources { get => _engine.Resources; set => _engine.Resources = value; }

            public MarkdownEngineDng(IMarkdownEngine2 engine2)
            {
                _engine = engine2;
            }

            public IEnumerable<Control> RunBlockGamut(string? text, ParseStatus status)
            {
                if (text is null)
                {
                    throw new ArgumentNullException(nameof(text));
                }

                return _engine.ParseGamutElement(TextUtil.Normalize(text), status).Select(e => e.Control);
            }

            public IEnumerable<CInline> RunSpanGamut(string? text)
            {
                if (text is null)
                {
                    throw new ArgumentNullException(nameof(text));
                }

                return _engine.ParseGamutInline(TextUtil.Normalize(text));
            }

            public Control Transform(string text)
            {
                return _engine.Transform(text);
            }
        }
    }
}
