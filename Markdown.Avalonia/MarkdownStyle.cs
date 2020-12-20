using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Markdown.Avalonia
{
    public static class MarkdownStyle
    {
        static Styles LoadXaml(string name)
        {
            var asm = Assembly.GetExecutingAssembly();
            using (var stream = asm.GetManifestResourceStream("Markdown.Avalonia.MarkdownStyle.xml"))
            using (var text = new StreamReader(stream))
            {
                var resources = (ResourceDictionary)AvaloniaRuntimeXamlLoader.Load(text.ReadToEnd());
                return (Styles)resources[name];
            }
        }

        public static Styles Standard
        {
            get => LoadXaml("DocumentStyleStandard");
        }

        public static Styles DefaultTheme
        {
            get => LoadXaml("DocumentStyleDefaultTheme");
        }

        public static Styles FluentTheme
        {
            get => LoadXaml("DocumentStyleFluentTheme");
        }

        public static Styles GithubLike
        {
            get => LoadXaml("DocumentStyleGithubLike");
        }
    }
}
