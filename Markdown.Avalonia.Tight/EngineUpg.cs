using Avalonia.Controls;
using Avalonia.Rendering.Composition.Animations;
using ColorDocument.Avalonia;
using ColorDocument.Avalonia.DocumentElements;
using ColorTextBlock.Avalonia;
using Markdown.Avalonia.Parsers;
using Markdown.Avalonia.Utils;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows.Input;

namespace Markdown.Avalonia
{
    public static class MarkdownEngineExt
    {
        public static IMarkdownEngine2 Upgrade(this IMarkdownEngine engine)
            => engine is IMarkdownEngine2 engine2 ? engine2 : new EngineUpg(engine);

        class EngineUpg : IMarkdownEngine2
        {
            private IMarkdownEngine Engine { get; }
            public string AssetPathRoot { get => Engine.AssetPathRoot; set => Engine.AssetPathRoot = value; }
            public ICommand? HyperlinkCommand { get => Engine.HyperlinkCommand; set => Engine.HyperlinkCommand = value; }
            public IBitmapLoader? BitmapLoader { get => Engine.BitmapLoader; set => Engine.BitmapLoader = value; }
            public IContainerBlockHandler? ContainerBlockHandler { get => Engine.ContainerBlockHandler; set => Engine.ContainerBlockHandler = value; }
            public MdAvPlugins Plugins { get => Engine.Plugins; set => Engine.Plugins = value; }
            public bool UseResource { get => Engine.UseResource; set => Engine.UseResource = value; }
            public CascadeDictionary CascadeResources => Engine.CascadeResources;
            public IResourceDictionary Resources { get => Engine.Resources; set => Engine.Resources = value; }

            public EngineUpg(IMarkdownEngine engine)
            {
                Engine = engine;
            }

            public Control Transform(string text)
                => Engine.Transform(text);

            public DocumentElement TransformElement(string text)
                => new UnBlockElement(Engine.Transform(text));

            public IEnumerable<DocumentElement> ParseGamutElement(string? text, ParseStatus status)
            {
                foreach (var ctrl in Engine.RunBlockGamut(text, status))
                {
                    yield return new UnBlockElement(ctrl);
                }
            }

            public IEnumerable<CInline> ParseGamutInline(string? text)
                => Engine.RunSpanGamut(text);
        }
    }
}
