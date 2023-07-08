using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Markdown.Avalonia.SyntaxHigh.Extensions
{
    /// <summary>
    /// Change syntax color according to the Foreground color.
    /// </summary>
    /// <remarks>
    /// This class change hue and saturation of the syntax color according to Foreground.
    /// This class assume that Foreground is the complementary color of Background.
    /// 
    /// You may think It's better to change it according to Bachground,
    /// But Background may be declared as absolutly transparent.
    /// </remarks>
    public class SyntaxHighlightWrapperExtension : MarkupExtension
    {
        public static readonly AvaloniaProperty<SyntaxHighlightProvider> ProviderProperty =
            AvaloniaProperty.Register<TextEditor, SyntaxHighlightProvider>("Provider");

        private string ForegroundName;

        public SyntaxHighlightWrapperExtension(string colorKey)
        {
            this.ForegroundName = colorKey;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var dyExt = new DynamicResourceExtension(ForegroundName);
            var brush = dyExt.ProvideValue(serviceProvider);

            var tag = new Binding(nameof(TextEditor.Tag))
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.Self)
            };

            var provider = new Binding(ProviderProperty.Name)
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.Self)
            };

            return new MultiBinding()
            {
                Bindings = new IBinding[] { brush, provider, tag },
                Converter = new SyntaxHighlightWrapperConverter()
            };
        }

        class SyntaxHighlightWrapperConverter : IMultiValueConverter
        {
            public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
            {
                var provider = values[1] as SyntaxHighlightProvider;
                var codeLang = values[2] as string;

                if (String.IsNullOrEmpty(codeLang))
                    return null;

                var highlight = provider is null ?
                    HighlightingManager.Instance.GetDefinitionByExtension("." + codeLang) :
                    provider.Solve(codeLang!);

                if (highlight is null) return null;

                Color foreColor = values[0] is SolidColorBrush cBrush ?
                    cBrush.Color :
                    values[0] is Color cColor ? cColor : Colors.Black;

                try
                {
                    return new HighlightWrapper(highlight, foreColor);
                }
                catch (Exception e)
                {
                    Trace.TraceError(e.ToString());
                    return highlight;
                }
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}
