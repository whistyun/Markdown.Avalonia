using Markdown.Avalonia.Plugins;
using Avalonia.Styling;
using Markdown.Avalonia.SyntaxHigh.StyleCollections;

namespace Markdown.Avalonia.SyntaxHigh
{
    internal class StyleEdit : IStyleEdit
    {
        public void Edit(string styleName, Styles styles)
        {
            switch (styleName)
            {
                case nameof(MarkdownStyle.SimpleTheme):
                    styles.AddRange(new AppendixOfDefaultTheme());
                    break;

                case nameof(MarkdownStyle.FluentTheme):
                    styles.AddRange(new AppendixOfFluentTheme());
                    break;
            }
        }
    }

}
