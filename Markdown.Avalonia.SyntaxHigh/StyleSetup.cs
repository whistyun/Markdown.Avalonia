using Avalonia.Styling;
using Markdown.Avalonia.SyntaxHigh.StyleCollections;
using System;
using System.Collections.Generic;
using System.Text;

namespace Markdown.Avalonia.SyntaxHigh
{
    public class StyleSetup
    {
        public IEnumerable<KeyValuePair<string, Action<Styles>>> GetOverrideStyles()
        {
            var list = new List<KeyValuePair<string, Action<Styles>>>();

            list.Add(
                new KeyValuePair<string, Action<Styles>>(
                    nameof(MarkdownStyle.DefaultTheme),
                    styles =>
                    {
                        var appendStyles = new AppendixOfDefaultTheme();
                        styles.Add(appendStyles[0]);
                    }
                )
            );

            list.Add(
                new KeyValuePair<string, Action<Styles>>(
                    nameof(MarkdownStyle.FluentTheme),
                    styles =>
                    {
                        var appendStyles = new AppendixOfFluentTheme();
                        styles.Add(appendStyles[0]);
                    }
                )
            );

            return list;
        }
    }
}
