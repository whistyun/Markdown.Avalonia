using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Markdown.Avalonia.Controls
{
    public class MultiplyExtension : MarkupExtension
    {
        string ResourceKey;
        double Scale;

        public MultiplyExtension(string resourceKey) : this(resourceKey, 1) { }

        public MultiplyExtension(string resourceKey, double scale)
        {
            this.ResourceKey = resourceKey;
            this.Scale = scale;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var fgExt = new DynamicResourceExtension(ResourceKey);

            var fgBrush = fgExt.ProvideValue(serviceProvider);

            return new MultiBinding()
            {
                Bindings = new IBinding[] { fgBrush },
                Converter = new MultiplyConverter(Scale)
            };
        }

        private T GetServiceFrom<T>(IServiceProvider serviceProvider)
        {
            return (T)serviceProvider.GetService(typeof(T));
        }
    }

    class MultiplyConverter : IMultiValueConverter
    {
        public double Scale { get; }

        public MultiplyConverter(double scale)
        {
            Scale = scale;
        }

        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            switch (values[0])
            {
                case short s:
                    return (short)(s * Scale);
                case int i:
                    return (int)(i * Scale);
                case long l:
                    return (long)(l * Scale);

                case float f:
                    return (float)(f * Scale);
                case double d:
                    return (double)(d * Scale);

                default:
                    return values[0];
            }
        }
    }
}
