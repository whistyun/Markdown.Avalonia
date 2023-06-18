using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Text;

namespace Markdown.Avalonia.StyleCollections
{
    class MarkdownStyleDefaultTheme : Styles, INamedStyle
    {
        public string Name => nameof(MarkdownStyle.SimpleTheme);
        public bool IsEditted { get; set; }

        public MarkdownStyleDefaultTheme()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
