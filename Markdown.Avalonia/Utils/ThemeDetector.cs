using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using System;
using System.Collections.Generic;
using System.Text;

namespace Markdown.Avalonia.Utils
{
    static class ThemeDetector
    {
        static bool? _isDefaultUsed;
        static bool? _isFluentUsed;
        static bool _isAvalonEditSetup;

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

        public static bool? IsDefaultUsed
        {
            get
            {
                if (_isDefaultUsed.HasValue)
                    return _isDefaultUsed;

                if (Application.Current is null
                        || Application.Current.Styles is null)
                    return null;

                foreach (var style in Application.Current.Styles)
                {
                    if (CheckStyleSourceHost(style, "Avalonia.Themes.Default"))
                    {
                        return _isDefaultUsed = true;
                    }
                }

                return _isDefaultUsed = false;
            }
        }


        public static bool? IsFluentUsed
        {
            get
            {
                if (_isFluentUsed.HasValue)
                    return _isFluentUsed;

                if (Application.Current is null
                        || Application.Current.Styles is null)
                    return null;

                foreach (var style in Application.Current.Styles)
                {
                    if (style is FluentTheme)
                    {
                        return _isFluentUsed = true;
                    }
                    if (CheckStyleSourceHost(style, "Avalonia.Themes.Fluent"))
                    {
                        return _isFluentUsed = true;
                    }
                }

                return _isFluentUsed = false;
            }
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
