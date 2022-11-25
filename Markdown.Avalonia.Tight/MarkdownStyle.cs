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
        private static readonly Dictionary<string, Action<Styles>> s_styleOverrideMap;

        static MarkdownStyle()
        {
            s_styleOverrideMap = new Dictionary<string, Action<Styles>>();

            try
            {
                var actions = InterassemblyUtil.InvokeInstanceMethodToGetProperty
                    <IEnumerable<KeyValuePair<string, Action<Styles>>>>(
                    "Markdown.Avalonia.SyntaxHigh",
                    "Markdown.Avalonia.SyntaxHigh.StyleSetup",
                    "GetOverrideStyles");

                if (actions is null)
                    throw new NullReferenceException("action");

                foreach (var action in actions)
                    s_styleOverrideMap[action.Key] = action.Value;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.GetType().Name + ":" + e.Message);
            }

        }

        private static Styles Filter(string name, Styles origin)
        {
            if (s_styleOverrideMap.TryGetValue(name, out var filter))
                filter(origin);

            return origin;
        }

        public static Styles Standard
        {
            get => Filter(nameof(Standard), new MarkdownStyleStandard());
        }

        [Obsolete("Use SimpleTheme instead")]
        public static Styles DefaultTheme
        {
            get => SimpleTheme;
        }

        public static Styles SimpleTheme
        {
            get => Filter(nameof(SimpleTheme), new MarkdownStyleDefaultTheme());
        }

        public static Styles FluentTheme
        {
            get => Filter(nameof(FluentTheme), new MarkdownStyleFluentTheme());
        }

        public static Styles FluentAvalonia
        {
            get => Filter(nameof(FluentAvalonia), new MarkdownStyleFluentAvalonia());
        }

        public static Styles GithubLike
        {
            get => Filter(nameof(GithubLike), new MarkdownStyleGithubLike());
        }
    }
}
