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
        ICommand HyperlinkCommand { get; set; }
        IBitmapLoader BitmapLoader { get; set; }

        IContainerBlockHandler ContainerBlockHandler { get; set; }
            
        Control Transform(string text);
    }
}
