using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Markdown.Avalonia
{
    public static class MarkdownStyle
    {
        static Styles LoadXaml(string name)
        {
            var resourceName = "Markdown.Avalonia.MarkdownStyle.xml";

            Assembly asm = Assembly.GetCallingAssembly();
            using (var stream = asm.GetManifestResourceStream(resourceName))
            {
                var loader = new AvaloniaXamlLoader();
                var resources = (ResourceDictionary)loader.Load(stream, null);
                return (Styles)resources[name];
            }
        }

        public static Styles Standard
        {
            get => LoadXaml("DocumentStyleStandard");
        }

        public static Styles Standard2
        {
            get => LoadXaml("DocumentStyleStandard2");
        }

        public static Styles GithubLike
        {
            get => LoadXaml("DocumentStyleGithubLike");
        }
    }
}
