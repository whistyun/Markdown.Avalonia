using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

            var monospaceName = FontManager.Current.GetInstalledFontFamilyNames()
                                           .Where(name => RequestFamilies.Any(reqNm => name.ToLower().Contains(reqNm)))
                                           .FirstOrDefault();

            return String.IsNullOrEmpty(monospaceName) ?
                null :
                new FontFamily(monospaceName);
        }
    }
}
