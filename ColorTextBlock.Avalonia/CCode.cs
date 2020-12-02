using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;

namespace ColorTextBlock.Avalonia
{
    public class CCode : CSpan
    {
        private readonly static FontFamily Monospace;

        static CCode()
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
                                           .Where(name=> RequestFamilies.Any(reqNm => name.ToLower().Contains(reqNm)))
                                           .FirstOrDefault();

            Monospace = new FontFamily(monospaceName);
        }

        public CCode(IEnumerable<CInline> inlines) : base(inlines)
        {
            FontFamily = Monospace;
        }
    }
}
