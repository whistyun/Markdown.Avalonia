using Avalonia.Controls;
using ColorTextBlock.Avalonia;
using Markdown.Avalonia.Parsers;
using Markdown.Avalonia.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Markdown.Avalonia
{
    public interface IMarkdownEngine
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

        IEnumerable<Control> RunBlockGamut(string? text, ParseStatus status);

        IEnumerable<CInline> RunSpanGamut(string? text);
    }
}