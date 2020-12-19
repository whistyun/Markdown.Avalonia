using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Markdown.Avalonia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Markdown.AvaloniaDemo
{
    public class ThemeBehavior
    {
        public static AttachedProperty<Uri> StyleUriProperty =
            AvaloniaProperty.RegisterAttached<ThemeBehavior, Control, Uri>(
                "StyleUri", validate: Validate);

        public static Uri Validate(Control ctrl, Uri src)
        {
            var style = new StyleInclude(new Uri("resm:Styles?assembly=Markdown.AvaloniaDemo"))
            {
                Source = src
            };


            if (ctrl.Styles.Count == 0)
            {
                ctrl.Styles.Add(style);
            }
            else
            {
                ctrl.Styles[0] = style;
            }

            return src;
        }

        public static void SetStyleUri(Control ctrl, Uri value)
        {
            ctrl.SetValue(StyleUriProperty, value);
        }

        public static Uri GetStyleUri(Control ctrl)
        {
            return ctrl.GetValue(StyleUriProperty);
        }
    }
}
