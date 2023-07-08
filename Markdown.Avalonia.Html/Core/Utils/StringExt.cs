using System;
using System.Net;

namespace Markdown.Avalonia.Html.Core.Utils
{
    internal static class StringExt
    {
        public static string[] SplitLine(this string text) => text.Split('\n');

        internal static bool TryDecode(this string text, ref int start, out string decoded)
        {
            //  max length of entity is 33 (&CounterClockwiseContourIntegral;)
            var hit = text.IndexOf(';', start, Math.Min(text.Length - start, 40));

            if (hit == -1)
            {
                decoded = string.Empty;
                return false;
            }

            var entity = text.Substring(start, hit - start + 1);
            decoded = WebUtility.HtmlDecode(entity);
            start = hit;

            if (decoded == "<" && start + 1 < text.Length)
            {
                var c = text[start + 1];
                if ('a' <= c && c <= 'z' && 'A' <= c && c <= 'Z')
                {
                    // '<[a-zA-Z]' may be treated as tag
                    decoded = entity;
                }
            }


            return true;
        }
    }
}
