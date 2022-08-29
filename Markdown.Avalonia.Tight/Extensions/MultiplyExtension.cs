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

namespace Markdown.Avalonia.Extensions
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
            var dyExt = new DynamicResourceExtension(ResourceKey);

            var brush = dyExt.ProvideValue(serviceProvider);

            return new MultiBinding()
            {
                Bindings = new IBinding[] { brush },
                Converter = new MultiplyConverter(Scale)
            };
        }

        private T GetServiceFrom<T>(IServiceProvider serviceProvider)
        {
            return (T)serviceProvider.GetService(typeof(T));
        }

        class MultiplyConverter : IMultiValueConverter
        {
            public double Scale { get; }

            public MultiplyConverter(double scale)
            {
                Scale = scale;
            }

            public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
            {
                return values[0] switch
                {
                    short s => (short)(s * Scale),
                    int i => (int)(i * Scale),
                    long l => (long)(l * Scale),
                    float f => (float)(f * Scale),
                    double d => (double)(d * Scale),
                    _ => values[0],
                };
            }
        }
    }
}
