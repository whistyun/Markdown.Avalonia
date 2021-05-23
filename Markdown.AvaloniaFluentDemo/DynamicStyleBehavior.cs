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

namespace Markdown.AvaloniaFluentDemo
{
    public class DynamicStyleBehavior
    {
        public static AttachedProperty<string> MyIdProperty =
            AvaloniaProperty.RegisterAttached<DynamicStyleBehavior, Styles, string>("MyId");

        public static AttachedProperty<string> MyLastParsedProperty =
            AvaloniaProperty.RegisterAttached<DynamicStyleBehavior, MarkdownScrollViewer, string>("MyLastParsed");

        public static AttachedProperty<string> XamlTextProperty =
            AvaloniaProperty.RegisterAttached<DynamicStyleBehavior, MarkdownScrollViewer, string>(
                "XamlText", coerce: Validate);

        public static AttachedProperty<ICommand> ValidationResultProperty =
            AvaloniaProperty.RegisterAttached<DynamicStyleBehavior, MarkdownScrollViewer, ICommand>(
                "ValidationResult");

        public static string Validate(IAvaloniaObject obj, string xamlTxt)
        {
            var ctrl = obj as MarkdownScrollViewer;

            try
            {
                var old = ctrl.GetValue(MyLastParsedProperty);
                if (old == xamlTxt) return xamlTxt;

                var style = (Styles)AvaloniaRuntimeXamlLoader.Load(xamlTxt);
                style.SetValue(MyIdProperty, nameof(DynamicStyleBehavior));

                foreach (var exists in ctrl.Styles.ToArray())
                    if (exists is Styles existsStyle)
                        if (existsStyle.GetValue(MyIdProperty) == nameof(DynamicStyleBehavior))
                            ctrl.Styles.Remove(exists);

                ctrl.SetValue(MyLastParsedProperty, xamlTxt);
                ctrl.Styles.Add(style);

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
