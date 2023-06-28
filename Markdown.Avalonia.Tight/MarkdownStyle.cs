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
        public static Styles Standard
        {
            get => new MarkdownStyleStandard();
        }

        [Obsolete("Use SimpleTheme instead")]
        public static Styles DefaultTheme
        {
            get => SimpleTheme;
        }

        public static Styles SimpleTheme
        {
            get => new MarkdownStyleDefaultTheme();
        }

        public static Styles FluentTheme
        {
            get => new MarkdownStyleFluentTheme();
        }

        public static Styles FluentAvalonia
        {
            get => new MarkdownStyleFluentAvalonia();
        }

        public static Styles GithubLike
        {
            get => new MarkdownStyleGithubLike();
        }
    }
}
