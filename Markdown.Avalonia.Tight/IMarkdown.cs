using Avalonia.Controls;
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
        IImageLoader? ImageLoader { get; set; }

        bool UseResource { get; set; }
        CascadeDictionary CascadeResources { get; }
        public IResourceDictionary Resources { get; set; }


        IContainerBlockHandler? ContainerBlockHandler { get; set; }
            
        Control Transform(string text);
    }
}
