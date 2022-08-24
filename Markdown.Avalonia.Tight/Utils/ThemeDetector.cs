using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;


namespace Markdown.Avalonia.Utils
{
    static class ThemeDetector
    {
        private static readonly string s_SimpleThemeFQCN = "Avalonia.Themes.Simple.SimpleTheme";
        private static readonly string s_FluentThemeFQCN = "Avalonia.Themes.Fluent.FluentTheme";

        private static readonly string s_SimpleThemeHost = "Avalonia.Themes.Simple";
        private static readonly string s_FluentThemeHost = "Avalonia.Themes.Fluent";

        static bool? s_isSimpleUsed;
        static bool? s_isFluentUsed;

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

        public static bool? IsSimpleUsed
        {
            get
            {
                if (s_isSimpleUsed.HasValue)
                    return s_isSimpleUsed;

                if (Application.Current is null
                        || Application.Current.Styles is null)
                    return null;

                foreach (var style in Application.Current.Styles)
                {
                    if (style.GetType().FullName == s_SimpleThemeFQCN)
                    {
                        return s_isSimpleUsed = true;
                    }
                    if (CheckStyleSourceHost(style, s_SimpleThemeHost))
                    {
                        return s_isSimpleUsed = true;
                    }
                }

                return s_isSimpleUsed = false;
            }
        }


        public static bool? IsFluentUsed
        {
            get
            {
                if (s_isFluentUsed.HasValue)
                    return s_isFluentUsed;

                if (Application.Current is null
                        || Application.Current.Styles is null)
                    return null;

                foreach (var style in Application.Current.Styles)
                {
                    if (style.GetType().FullName == s_FluentThemeFQCN)
                    {
                        return s_isFluentUsed = true;
                    }
                    if (CheckStyleSourceHost(style, s_FluentThemeHost))
                    {
                        return s_isFluentUsed = true;
                    }
                }

                return s_isFluentUsed = false;
            }
        }
    }
}
