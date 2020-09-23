using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

#if MIG_FREE
namespace Markdown.Xaml
#else
namespace MdXaml
#endif
{
    public static class EmojiTable
    {
        private static ConcurrentDictionary<String, String> keywords;

        static EmojiTable()
        {
            LoadTxt();
        }


        /*
            Workaround for Visual Studio Xaml Designer.
            When you open MarkdownStyle from Xaml Designer,
            A null error occurs. Perhaps static constructor is not executed.         
        */
        static void LoadTxt()
        {
#if MIG_FREE
            var resourceName = "Markdown.Xaml.EmojiTable.txt";
#else
            var resourceName = "MdXaml.EmojiTable.txt";
#endif
            keywords = new ConcurrentDictionary<string, string>();

            Assembly asm = Assembly.GetCallingAssembly();
            using (var stream = asm.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream, true))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var elms = line.Split('\t');
                    keywords[elms[1]] = elms[0];
                }
            }
        }

        public static bool TryGet(string keyword, out string emoji)
        {
            if (keywords is null) LoadTxt();
            return keywords.TryGetValue(keyword, out emoji);
        }

    }
}
