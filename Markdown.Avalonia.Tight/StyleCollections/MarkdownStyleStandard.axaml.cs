using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Text;

namespace Markdown.Avalonia.StyleCollections
{
    class MarkdownStyleStandard : Styles, INamedStyle
    {
        public string Name => nameof(MarkdownStyle.Standard);
        public bool IsEditted { get; set; }

        public MarkdownStyleStandard()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
