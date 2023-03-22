using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColorTextBlock.Avalonia.Fonts
{
    internal class FontFamilyCollector
    {
        public static FontFamily? TryGetMonospace()
        {
            string[] RequestFamilies = {
                "menlo",
                "monaco",
                "consolas",
                "droid sans mono",
                "inconsolata",
                "courier new",
                "monospace",
                "dejavu sans mono",
            };

            var monospaceName = FontManager.Current.SystemFonts
                                           .Where(family => RequestFamilies.Any(reqNm => family.Name.ToLower().Contains(reqNm)))
                                           .FirstOrDefault();

            return monospaceName;
        }
    }
}
