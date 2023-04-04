using Avalonia.Controls;
using Markdown.Avalonia.Plugins;
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
        ICommand HyperlinkCommand { get; set; }
        IBitmapLoader BitmapLoader { get; set; }

        bool UseResource { get; set; }
        CascadeDictionary CascadeResources { get; }
        IResourceDictionary Resources { get; set; }

        IContainerBlockHandler ContainerBlockHandler { get; set; }

        SetupInfo SetupInfo { get; set; }


        Control Transform(string text);
    }
}
