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
                    styles.Add(new AppendixOfDefaultTheme()[0]);
                    break;

                case nameof(MarkdownStyle.FluentTheme):
                    styles.Add(new AppendixOfFluentTheme()[0]);
                    break;
            }
        }
    }

}
