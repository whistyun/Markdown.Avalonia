using System.Reflection;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;

namespace Markdown.Xaml
{
    public static class MarkdownStyle
    {
        static MarkdownStyle()
        {
            LoadXaml();
        }

        /*
            Workaround for Visual Studio Xaml Designer.
            When you open MarkdownStyle from Xaml Designer,
            A null error occurs. Perhaps static constructor is not executed.         
        */
        static void LoadXaml()
        {
            Assembly asm = Assembly.GetCallingAssembly();
            using (var stream = asm.GetManifestResourceStream("Markdown.Xaml.Markdown.Style.xaml"))
            {
                var resources = (ResourceDictionary)XamlReader.Load(stream);
                _standard = (Style)resources["DocumentStyleStandard"];
                _compact = (Style)resources["DocumentStyleCompact"];
            }
        }


        private static Style _standard;
        private static Style _compact;

        public static Style Standard
        {
            get
            {
                if (_standard == null) LoadXaml();
                return _standard;
            }
        }

        public static Style Compact
        {
            get
            {
                if (_compact == null) LoadXaml();
                return _compact;
            }
        }
    }
}
