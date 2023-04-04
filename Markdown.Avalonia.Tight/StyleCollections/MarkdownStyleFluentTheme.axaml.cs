using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Text;

namespace Markdown.Avalonia.StyleCollections
{
    class MarkdownStyleFluentTheme : Styles, INamedStyle
    {
        public string Name => nameof(MarkdownStyle.FluentTheme);
        public bool IsEditted { get; set; }

        public MarkdownStyleFluentTheme()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
