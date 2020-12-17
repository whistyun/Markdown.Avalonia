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

namespace Markdown.Avalonia.Utils
{
    public class ColorBlurExtension : MarkupExtension
    {
        public int Level { get; set; }

        public ColorBlurExtension(object resourceKey)
        {
            if (resourceKey is string lvtxt)
            {
                Level = Int32.Parse(lvtxt);
            }
            else if (resourceKey is int lv)
            {
                Level = lv;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var fgExt = new DynamicResourceExtension("ThemeForegroundBrush");
            var bgExt = new DynamicResourceExtension("ThemeBackgroundBrush");

            var fgBrush = fgExt.ProvideValue(serviceProvider);
            var bgBrush = bgExt.ProvideValue(serviceProvider);


            var provideTarget = GetServiceFrom<IProvideValueTarget>(serviceProvider);
            var setter = (Setter)provideTarget.TargetObject;


            switch (setter.Property.Name)
            {
                case nameof(TextBlock.Foreground):
                    return new MultiBinding()
                    {
                        Bindings = new IBinding[] { fgBrush, bgBrush }.ToList(),
                        Converter = new BluarConverter(Level)
                    };

                case nameof(Border.Background):
                    return new MultiBinding()
                    {
                        Bindings = new IBinding[] { bgBrush, fgBrush }.ToList(),
                        Converter = new BluarConverter(Level)
                    };

                default:
                    throw new InvalidOperationException();
            }
        }

        private T GetServiceFrom<T>(IServiceProvider serviceProvider)
        {
            return (T)serviceProvider.GetService(typeof(T));
        }
    }

    class BluarConverter : IMultiValueConverter
    {
        public int Level { get; }

        public BluarConverter(int level)
        {
            Level = level;
        }

        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            var targetBrush = values[0] as ISolidColorBrush;
            var pairBrush = values[1] as ISolidColorBrush;

            if (targetBrush is null || pairBrush is null)
                return values[0];

            var tColor = targetBrush.Color;
            var pColor = pairBrush.Color;

            var diffR = pColor.R - tColor.R;
            var diffG = pColor.G - tColor.G;
            var diffB = pColor.B - tColor.B;


            return new SolidColorBrush(
                        Color.FromRgb(
                            Cutt(tColor.R + diffR * (Level - 100) / 100),
                            Cutt(tColor.G + diffG * (Level - 100) / 100),
                            Cutt(tColor.B + diffB * (Level - 100) / 100)));
        }

        private byte Cutt(int v)
        {
            return (byte)Math.Min(Math.Max(v, 0), 255);
        }
    }
}
