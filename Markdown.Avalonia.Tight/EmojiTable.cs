using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text;

namespace Markdown.Avalonia
{
    public static class EmojiTable
    {
        private static ConcurrentDictionary<String, String> s_keywords;

        static EmojiTable()
        {
            s_keywords = LoadTxt();
        }


        /*
            Workaround for Visual Studio Xaml Designer.
            When you open MarkdownStyle from Xaml Designer,
            A null error occurs. Perhaps static constructor is not executed.         
        */
        private static ConcurrentDictionary<string, string> LoadTxt()
        {
            var resourceName = "Markdown.Avalonia.EmojiTable.txt";

            var dic = new ConcurrentDictionary<string, string>();

            Assembly asm = typeof(EmojiTable).Assembly;
            using var stream = asm.GetManifestResourceStream(resourceName);
            Debug.Assert(stream is not null);

            using var reader = new StreamReader(stream, true);

            string? line;
            while ((line = reader.ReadLine()) is not null)
            {
                var elms = line.Split('\t');
                dic[elms[1]] = elms[0];
            }

            return dic;
        }

#if NET6_0_OR_GREATER
        public static bool TryGet(string keyword, [MaybeNullWhen(false)] out string? emoji)
#else
        public static bool TryGet(string keyword, out string emoji)
#endif
        {
            if (s_keywords is null) s_keywords = LoadTxt();

            return s_keywords.TryGetValue(keyword, out emoji);
        }

    }
}
