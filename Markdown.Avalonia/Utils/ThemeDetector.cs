using Avalonia;
using Avalonia.Markup.Xaml.Styling;
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
                    if (style is StyleInclude incld
                            && incld.Source?.Host == "Avalonia.Themes.Default")
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
                    else if (style is StyleInclude incld
                            && incld.Source?.Host == "Avalonia.Themes.Fluent")
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
                    if (style is StyleInclude incld && incld.Source?.Host == "AvaloniaEdit")
                    {
                        return _isAvalonEditSetup = true;
                    }
                }

                return _isAvalonEditSetup = false;
            }
        }
    }
}
