using System.Reflection;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;

namespace MdXaml
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
            /*
                Workaround for XamlParseException.
                When you don't load 'ICSharpCode.AvalonEdit.dll',
                XamlReader fail to read xmlns:avalonEdit="http://icsharpcode.net..."
             */
            var txtedit = typeof(ICSharpCode.AvalonEdit.TextEditor);
            txtedit.ToString();

            Assembly asm = Assembly.GetCallingAssembly();
            using (var stream = asm.GetManifestResourceStream("MdXaml.Markdown.Style.xaml"))
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
