using Avalonia.Controls;
using Avalonia.Rendering.Composition.Animations;
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
    public interface IMarkdownEngine : IMarkdownEngineBase
    {
        string AssetPathRoot { get; set; }

        ICommand? HyperlinkCommand { get; set; }

        [Obsolete("Please use Plugins propety. see https://github.com/whistyun/Markdown.Avalonia/wiki/How-to-migrages-to-ver11")]
        IBitmapLoader? BitmapLoader { get; set; }

        IContainerBlockHandler? ContainerBlockHandler { get; set; }

        MdAvPlugins Plugins { get; set; }

        bool UseResource { get; set; }
        CascadeDictionary CascadeResources { get; }
        public IResourceDictionary Resources { get; set; }

        Control Transform(string text);

        /// <summary>
        /// Perform transformations that form block-level tags like paragraphs, headers, and list items.
        /// </summary>
        IEnumerable<Control> RunBlockGamut(string? text, ParseStatus status);

        /// <summary>
        /// Perform transformations that occur *within* block-level tags like paragraphs, headers, and list items.
        /// </summary>
        IEnumerable<CInline> RunSpanGamut(string? text);
    }
}