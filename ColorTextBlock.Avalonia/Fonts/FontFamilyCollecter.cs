using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FFamily = Avalonia.Media.FontFamily;

namespace ColorTextBlock.Avalonia.Fonts
{
    internal class FontFamilyCollecter
    {
        public static FontFamily TryGetMonospace()
        {
            string[] RequestFamilies = {
                "menlo",
                "monaco",
                "consolas",
                "droid sans mono",
                "inconsolata",
                "courier new",
                "monospace",
                "droid sans fallback"
            };

            return FFamily.SystemFontFamilies
                        .Where(family => family.FamilyNames.Any(name => RequestFamilies.Any(reqNm => name.ToLower().Contains(reqNm))))
                        .FirstOrDefault();
        }
    }
}
