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

        static void LoadXaml()
        {
            var asm = Assembly.GetExecutingAssembly();
            using (var stream = asm.GetManifestResourceStream("Markdown.Avalonia.MarkdownStyle.xml"))
            using (var text = new StreamReader(stream))
            {
                var resources = (ResourceDictionary)AvaloniaRuntimeXamlLoader.Load(text.ReadToEnd());
                _standard = (Styles)resources["DocumentStyleStandard"];
                _githublike = (Styles)resources["DocumentStyleGithubLike"];
            }
        }

        private static Styles _standard;
        private static Styles _githublike;

        public static Styles Standard
        {
            get
            {
                if (_standard == null) LoadXaml();
                return _standard;
            }
        }

        public static Styles GithubLike
        {
            get
            {
                if (_githublike == null) LoadXaml();
                return _githublike;
            }
        }
    }
}
