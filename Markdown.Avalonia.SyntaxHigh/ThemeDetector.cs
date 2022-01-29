using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;

namespace Markdown.Avalonia.SyntaxHigh
{
    static class ThemeDetector
    {
        private static bool _isAvalonEditSetup;

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
    }
}
