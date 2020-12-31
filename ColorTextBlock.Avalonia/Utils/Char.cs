using System;
using System.Collections.Generic;
using System.Text;

namespace ColorTextBlock.Avalonia.Utils
{
    static class CharExt
    {
        public static bool IsCJK(this char c)
        {
            // see https://medium.com/the-artificial-impostor/detecting-chinese-characters-in-unicode-strings-4ac839ba313a

            var cd = (int)c;

            return
                // CJK Unified Ideographs
                (0x4e00 <= cd && cd <= 0x9FFF) ||
                // Hiragana & Katakana
                (0x3040 <= cd && cd <= 0x30ff) ||
                // Hangul Syllables
                (0xac00 <= cd && cd <= 0xd7a3);
        }
    }
}
