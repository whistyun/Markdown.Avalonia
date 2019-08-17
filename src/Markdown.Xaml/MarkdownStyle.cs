using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace Markdown.Xaml
{
    public static class MarkdownStyle
    {
        static MarkdownStyle()
        {
            Assembly asm = Assembly.GetCallingAssembly();
            using (var stream = asm.GetManifestResourceStream("Markdown.Xaml.Markdown.Style.xaml"))
            {
                var resources = (ResourceDictionary)XamlReader.Load(stream);
                Standard = (Style)resources["DocumentStyleStandard"];
                Compact = (Style)resources["DocumentStyleCompact"];
            }
        }

        public static Style Standard { private set; get; }

        public static Style Compact { private set; get; }
    }
}
