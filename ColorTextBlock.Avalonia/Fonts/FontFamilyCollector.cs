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
            string[] requestFamilies = {
                "Menlo",
                "Monaco",
                "Consolas",
                "Droid Sans Mono",
                "Inconsolata",
                "Courier New",
                "Monospace",
                "DejaVu Sans Mono",
            };

            var fontManager = FontManager.Current;
            var systemFonts = fontManager.SystemFonts;

            // Prefer exact matches in the requested order.
            foreach (var requestedFamily in requestFamilies)
            {
                var exact = systemFonts.FirstOrDefault(family =>
                    string.Equals(family.Name, requestedFamily, StringComparison.OrdinalIgnoreCase));

                if (exact != null && CanCreateGlyphTypeface(fontManager, exact))
                {
                    return exact;
                }
            }

            // Then allow partial matches for font variants, but still validate they are loadable.
            foreach (var requestedFamily in requestFamilies)
            {
                var partial = systemFonts.FirstOrDefault(family =>
                    family.Name.Contains(requestedFamily, StringComparison.OrdinalIgnoreCase));

                if (partial != null && CanCreateGlyphTypeface(fontManager, partial))
                {
                    return partial;
                }
            }

            return null;
        }

        private static bool CanCreateGlyphTypeface(FontManager fontManager, FontFamily family)
        {
            var typeface = new Typeface(family, FontStyle.Normal, FontWeight.Normal, FontStretch.Normal);
            return fontManager.TryGetGlyphTypeface(typeface, out _);
        }
    }
}
