using System.Reflection;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;

#if MIG_FREE
namespace Markdown.Xaml
#else
namespace MdXaml
#endif
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
#if MIG_FREE
            var resourceName = "Markdown.Xaml.MarkdownMigFree.Style.xaml";
#else
            /*
                Workaround for XamlParseException.
                When you don't load 'ICSharpCode.AvalonEdit.dll',
                XamlReader fail to read xmlns:avalonEdit="http://icsharpcode.net..."
             */
            var txtedit = typeof(ICSharpCode.AvalonEdit.TextEditor);
            txtedit.ToString();

            var resourceName = "MdXaml.Markdown.Style.xaml";
#endif
            Assembly asm = Assembly.GetCallingAssembly();
            using (var stream = asm.GetManifestResourceStream(resourceName))
            {
                var resources = (ResourceDictionary)XamlReader.Load(stream);
                _standard = (Style)resources["DocumentStyleStandard"];
                _compact = (Style)resources["DocumentStyleCompact"];
                _githublike = (Style)resources["DocumentStyleGithubLike"];
            }
        }


        private static Style _standard;
        private static Style _compact;
        private static Style _githublike;

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

        public static Style GithubLike
        {
            get
            {
                if (_githublike == null) LoadXaml();
                return _githublike;
            }
        }

    }
}
