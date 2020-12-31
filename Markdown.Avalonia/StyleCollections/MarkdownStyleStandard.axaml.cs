using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Text;

namespace Markdown.Avalonia.StyleCollections
{
    class MarkdownStyleStandard : Styles
    {
        public MarkdownStyleStandard()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
