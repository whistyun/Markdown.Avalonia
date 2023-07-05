using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Markdown.Avalonia.Html
{
    internal static class SimpleHtmlUtils
    {
        private static readonly HashSet<string> s_emptyList = new(new[] {
            "area", "base", "br", "col", "embed", "hr", "img", "input", "keygen", "link", "meta", "param", "source",
        });

        private static readonly Regex s_tagPattern = new(@"<(?'close'/?)[\t ]*(?'tagname'[a-z][a-z0-9]*)(?'attributes'[ \t][^>]*|/)?>",
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex s_emptylinePattern = new(@"\n{2}", RegexOptions.Compiled);

        public static Regex CreateTagstartPattern(IEnumerable<string> tags)
        {
            var taglist = string.Join("|", tags);

            return new Regex(@$"<[\t ]*(?'tagname'{taglist})(?'attributes'[ \t][^>]*|/)?>",
                RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        public static int SearchTagRange(string text, Match tagStartPatternMatch)
        {
            int searchStart = tagStartPatternMatch.Index + tagStartPatternMatch.Length;

            if (tagStartPatternMatch.Value.EndsWith("/>"))
            {
                return searchStart;
            }
            else
            {
                int end = SearchTagEnd(text, searchStart, tagStartPatternMatch.Groups["tagname"].Value);
                return end == -1 ? text.Length : end;
            }
        }

        public static int SearchTagRangeContinuous(string text, Match tagStartPatternMatch)
        {
            int idx = SearchTagRange(text, tagStartPatternMatch);

            for (; ; )
            {
                if (text.Length - 1 <= idx) return idx;

                var emp = s_emptylinePattern.Match(text, idx);
                if (!emp.Success) return text.Length - 1;

                var tag = s_tagPattern.Match(text, idx);
                if (tag.Success && tag.Index < emp.Index)
                {
                    idx = SearchTagRange(text, tag);
                }
                else return emp.Index;
            }
        }


        public static int SearchTagEnd(string text, int start, string startTagName)
        {
            var tags = new Stack<string>();
            tags.Push(startTagName);

            for (; ; )
            {
                var isEmptyTag = s_emptyList.Contains(tags.Peek());

                var mch = s_tagPattern.Match(text, start);

                if (isEmptyTag && (!mch.Success || mch.Index != start))
                {
                    if (tags.Count == 1) return start;

                    tags.Pop();
                }

                if (!mch.Success) return -1;

                start = mch.Index + mch.Length;

                if (mch.Value.EndsWith("/>"))
                {
                    continue;
                }

                var tagName = mch.Groups["tagname"].Value.ToLower();

                if (!String.IsNullOrEmpty(mch.Groups["close"].Value))
                {
                    // pop until same tag name be found.

                    while (tags.Count > 0)
                    {
                        var peekTag = tags.Peek();

                        tags.Pop();

                        if (peekTag == tagName) break;
                    }

                    if (tags.Count == 0)
                    {
                        return mch.Index + mch.Length;
                    }
                }
                else
                {
                    if (s_emptyList.Contains(tags.Peek()))
                    {
                        tags.Pop();
                    }

                    tags.Push(mch.Groups["tagname"].Value);
                }
            }
        }
    }
}
