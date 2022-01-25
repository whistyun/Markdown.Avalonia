using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Markdown.Avalonia.StyleCollections;
using Markdown.Avalonia.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace Markdown.Avalonia
{
    public static class MarkdownStyle
    {
        private static Dictionary<string, Action<Styles>> StyleOverrideMap;

        static MarkdownStyle()
        {
            StyleOverrideMap = new Dictionary<string, Action<Styles>>();

            try
            {
                var actions = InterassemblyUtil.InvokeInstanceMethodToGetProperty
                    <IEnumerable<KeyValuePair<string, Action<Styles>>>>(
                    "Markdown.Avalonia.SyntaxHigh",
                    "Markdown.Avalonia.SyntaxHigh.StyleSetup",
                    "GetOverrideStyles");

                foreach (var action in actions)
                    StyleOverrideMap[action.Key] = action.Value;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.GetType().Name + ":" + e.Message);
            }

        }

        private static Styles Filter(string name, Styles origin)
        {
            if (StyleOverrideMap.TryGetValue(name, out var filter))
                filter(origin);

            return origin;
        }

        public static Styles Standard
        {
            get => Filter(nameof(Standard), new MarkdownStyleStandard());
        }

        public static Styles DefaultTheme
        {
            get => Filter(nameof(DefaultTheme), new MarkdownStyleDefaultTheme());
        }

        public static Styles FluentTheme
        {
            get => Filter(nameof(FluentTheme), new MarkdownStyleFluentTheme());
        }

        public static Styles GithubLike
        {
            get => Filter(nameof(GithubLike), new MarkdownStyleGithubLike());
        }
    }
}
