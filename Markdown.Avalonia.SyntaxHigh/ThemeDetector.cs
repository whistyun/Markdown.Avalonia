using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;

namespace Markdown.Avalonia.SyntaxHigh
{
    static class ThemeDetector
    {
        private static readonly string s_SimpleThemeFQCN = "Avalonia.Themes.Simple.SimpleTheme";
        private static readonly string s_FluentThemeFQCN = "Avalonia.Themes.Fluent.FluentTheme";

        private static readonly string s_SimpleThemeHost = "Avalonia.Themes.Simple";
        private static readonly string s_FluentThemeHost = "Avalonia.Themes.Fluent";

        private static bool _isAvalonEditSetup;
        private static bool? s_isSimpleUsed;
        private static bool? s_isFluentUsed;

        private static bool CheckStyleSourceHost(IStyle style, string hostName)
        {
            if (style is StyleInclude incld)
            {
                var uri = incld.Source;

                if (uri is null) return false;
                if (!uri.IsAbsoluteUri) return false;

                try { return uri.Host == hostName; }
                catch { return false; }
            }
            else return false;
        }

        private static bool? CheckApplicationCurrentStyle(string themeFQCN, string avaresHost)
        {
            if (Application.Current is null
                    || Application.Current.Styles is null)
                return null;

            foreach (var style in Application.Current.Styles)
            {
                if (style.GetType().FullName == themeFQCN)
                {
                    return true;
                }
                if (style is StyleInclude incld)
                {
                    var uri = incld.Source;

                    if (uri is null) return false;
                    if (!uri.IsAbsoluteUri) return false;

                    try { return uri.Host == avaresHost; }
                    catch { return false; }
                }
                else return false;
            }

            return false;
        }

        public static bool IsAvalonEditSetup
        {
            get
            {
                if (_isAvalonEditSetup)
                    return true;

                if (Application.Current is null
                        || Application.Current.Styles is null)
                    return false;

                foreach (var style in Application.Current.Styles)
                {
                    if (CheckStyleSourceHost(style, "AvaloniaEdit"))
                    {
                        return _isAvalonEditSetup = true;
                    }
                }

                return _isAvalonEditSetup = false;
            }
        }

        public static bool IsFluentUsed
        {
            get
            {
                if (s_isFluentUsed.HasValue)
                    return s_isFluentUsed.Value;

                return Nvl(s_isFluentUsed = CheckApplicationCurrentStyle(s_FluentThemeFQCN, s_FluentThemeHost));
            }
        }

        public static bool IsSimpleUsed
        {
            get
            {
                if (s_isSimpleUsed.HasValue)
                    return s_isSimpleUsed.Value;

                return Nvl(s_isSimpleUsed = CheckApplicationCurrentStyle(s_SimpleThemeFQCN, s_SimpleThemeHost));
            }
        }

        private static bool Nvl(bool? val) => val.HasValue && val.Value;
    }
}
