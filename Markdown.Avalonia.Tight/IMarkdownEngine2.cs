using Avalonia.Controls;
using ColorDocument.Avalonia;
using ColorTextBlock.Avalonia;
using Markdown.Avalonia.Parsers;
using Markdown.Avalonia.Utils;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Markdown.Avalonia
{
    public interface IMarkdownEngine2 : IMarkdownEngineBase
    {
        string AssetPathRoot { get; set; }

        ICommand? HyperlinkCommand { get; set; }

        IContainerBlockHandler? ContainerBlockHandler { get; set; }

        MdAvPlugins Plugins { get; set; }

        bool UseResource { get; set; }
        CascadeDictionary CascadeResources { get; }
        public IResourceDictionary Resources { get; set; }

        Control Transform(string text);

        DocumentElement TransformElement(string text);

        IEnumerable<DocumentElement> ParseGamutElement(string? text, ParseStatus status);

        IEnumerable<CInline> ParseGamutInline(string? text);
    }
}