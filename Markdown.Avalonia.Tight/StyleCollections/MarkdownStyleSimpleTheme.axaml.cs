using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Text;

namespace Markdown.Avalonia.StyleCollections
{
    public class MarkdownStyleSimpleTheme : Styles, INamedStyle
    {
        public string Name => nameof(MarkdownStyle.SimpleTheme);
        public bool IsEditted { get; set; }

        public MarkdownStyleSimpleTheme()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
