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
    public class DynamicStyleBehavior
    {
        public static AttachedProperty<string> MyIdProperty =
            AvaloniaProperty.RegisterAttached<DynamicStyleBehavior, Styles, string>("MyId");


        public static AttachedProperty<string> XamlTextProperty =
            AvaloniaProperty.RegisterAttached<DynamicStyleBehavior, MarkdownScrollViewer, string>(
                "XamlText", validate: Validate);

        public static AttachedProperty<ICommand> ValidationResultProperty =
            AvaloniaProperty.RegisterAttached<DynamicStyleBehavior, MarkdownScrollViewer, ICommand>(
                "ValidationResult");

        public static string Validate(MarkdownScrollViewer ctrl, string xamlTxt)
        {
            try
            {
                var loader = new AvaloniaXamlLoader();
                using (var stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(xamlTxt)))
                {
                    var style = (Styles)loader.Load(stream, null);
                    style.SetValue(MyIdProperty, nameof(DynamicStyleBehavior));

                    foreach (var exists in ctrl.Styles.ToArray())
                        if (exists is Styles existsStyle)
                            if (existsStyle.GetValue(MyIdProperty) == nameof(DynamicStyleBehavior))
                                ctrl.Styles.Remove(exists);

                    ctrl.Styles.Add(style);
                    ctrl.Rebuild();
                }

                var resultMsgr = ctrl.GetValue(ValidationResultProperty);
                resultMsgr?.Execute(null);
            }
            catch (Exception e)
            {
                string message = e.GetType().FullName + "\r\n" + e.Message;

                var resultMsgr = ctrl.GetValue(ValidationResultProperty);
                resultMsgr?.Execute(message);
            }

            return xamlTxt;
        }


        public static void SetXamlText(MarkdownScrollViewer ctrl, string xamlTxt)
            => ctrl.SetValue(XamlTextProperty, xamlTxt);

        public static string GetXamlText(MarkdownScrollViewer ctrl)
            => ctrl.GetValue(XamlTextProperty);

        public static void SetValidationResult(MarkdownScrollViewer ctrl, ICommand cmd)
            => ctrl.SetValue(ValidationResultProperty, cmd);

        public static ICommand GetValidationResult(MarkdownScrollViewer ctrl)
            => ctrl.GetValue(ValidationResultProperty);

    }
}
