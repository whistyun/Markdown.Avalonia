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
        static MarkdownStyle()
        {
            LoadXaml();
        }

        static void LoadXaml()
        {
            var resourceName = "Markdown.Avalonia.MarkdownStyle.xml";

            Assembly asm = Assembly.GetCallingAssembly();
            using (var stream = asm.GetManifestResourceStream(resourceName))
            {
                var loader = new AvaloniaXamlLoader();
                var resources = (ResourceDictionary)loader.Load(stream, null);
                _standard = (Styles)resources["DocumentStyleStandard"];
            }
        }

        private static Styles _standard;

        public static Styles Standard
        {
            get
            {
                if (_standard == null) LoadXaml();
                return _standard;
            }
        }
    }
}
