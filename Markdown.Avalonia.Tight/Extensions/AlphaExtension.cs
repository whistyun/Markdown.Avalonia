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
    public class AlphaExtension : MarkupExtension
    {
        private readonly string _brushName;
        private readonly float _alpha;

        public AlphaExtension(string colorKey) : this(colorKey, 1f) { }

        public AlphaExtension(string colorKey, float alpha)
        {
            this._brushName = colorKey;
            this._alpha = alpha;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var dyExt = new DynamicResourceExtension(_brushName);

            var brush = dyExt.ProvideValue(serviceProvider);

            return new MultiBinding()
            {
                Bindings = new IBinding[] { brush },
                Converter = new AlphaConverter(_alpha)
            };
        }

        class AlphaConverter : IMultiValueConverter
        {
            public float Alpha { get; }

            public AlphaConverter(float alpha)
            {
                Alpha = alpha;
            }

            public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
            {
                Color c;
                if (values[0] is ISolidColorBrush b)
                    c = b.Color;
                else if (values[0] is Color col)
                    c = col;
                else
                    return values[0];

                return new SolidColorBrush(
                            Color.FromArgb(
                                (byte)(c.A / 255f * Alpha * 255f),
                                c.R, c.G, c.B));
            }
        }
    }
}
