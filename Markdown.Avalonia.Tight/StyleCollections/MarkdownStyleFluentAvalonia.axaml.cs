using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Text;

namespace Markdown.Avalonia.StyleCollections
{
    class MarkdownStyleFluentAvalonia : Styles, INamedStyle
    {
        public string Name => nameof(MarkdownStyle.FluentAvalonia);
        public bool IsEditted { get; set; }

        public MarkdownStyleFluentAvalonia()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
