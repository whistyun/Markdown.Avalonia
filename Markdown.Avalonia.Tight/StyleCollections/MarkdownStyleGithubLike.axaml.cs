using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Text;

namespace Markdown.Avalonia.StyleCollections
{
    class MarkdownStyleGithubLike : Styles, INamedStyle
    {
        public string Name => nameof(MarkdownStyle.GithubLike);
        public bool IsEditted { get; set; }

        public MarkdownStyleGithubLike()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
