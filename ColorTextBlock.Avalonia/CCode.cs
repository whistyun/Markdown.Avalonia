using System;
using System.Collections.Generic;
using FStyle = Avalonia.Media.FontStyle;
using FWeight = Avalonia.Media.FontWeight;
using FFamily = Avalonia.Media.FontFamily;
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

            Monospace = FFamily.SystemFontFamilies
                .Where(family => family.FamilyNames.Any(name => RequestFamilies.Any(reqNm => name.ToLower().Contains(reqNm))))
                .FirstOrDefault();
        }


        public CCode(IEnumerable<CInline> inlines) : base(inlines)
        {
            if (Monospace != null)
            {
                FontFamily = Monospace;
            }
        }
    }
}
