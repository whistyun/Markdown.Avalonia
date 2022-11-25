using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;


namespace Markdown.Avalonia.Utils
{
    static class ThemeDetector
    {
        private static readonly string s_SimpleThemeFQCN = "Avalonia.Themes.Simple.SimpleTheme";
        private static readonly string s_FluentThemeFQCN = "Avalonia.Themes.Fluent.FluentTheme";
        private static readonly string s_FluentAvaloniaFQCN = "FluentAvalonia.Styling.FluentAvaloniaTheme";

        private static readonly string s_SimpleThemeHost = "Avalonia.Themes.Simple";
        private static readonly string s_FluentThemeHost = "Avalonia.Themes.Fluent";
        private static readonly string s_FluentAvaloniaHost = "FluentAvalonia";

        static bool? s_isSimpleUsed;
        static bool? s_isFluentUsed;
        static bool? s_isFluentAvaloniaUsed;

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

        public static bool? IsSimpleUsed
        {
            get
            {
                if (s_isSimpleUsed.HasValue)
                    return s_isSimpleUsed;

                return s_isSimpleUsed = CheckApplicationCurrentStyle(s_SimpleThemeFQCN, s_SimpleThemeHost);
            }
        }


        public static bool? IsFluentUsed
        {
            get
            {
                if (s_isFluentUsed.HasValue)
                    return s_isFluentUsed;

                return s_isFluentUsed = CheckApplicationCurrentStyle(s_FluentThemeFQCN, s_FluentThemeHost);
            }
        }


        public static bool? IsFluentAvaloniaUsed
        {
            get
            {
                if (s_isFluentAvaloniaUsed.HasValue)
                    return s_isFluentAvaloniaUsed;

                return s_isFluentAvaloniaUsed = CheckApplicationCurrentStyle(s_FluentAvaloniaFQCN, s_FluentAvaloniaHost);
            }
        }
    }
}
