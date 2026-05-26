using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Markdown.Avalonia.Extensions
{
    public class DivideColorExtension : MarkupExtension
    {
        private readonly string _frmKey;
        private readonly string _toKey;
        private readonly double _relate;

        public DivideColorExtension(string frm, string to, double relate)
        {
            this._frmKey = frm;
            this._toKey = to;
            this._relate = relate;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            IBinding left;
            if (Color.TryParse(_frmKey, out var leftColor))
            {
                left = new Binding { Source = new ImmutableSolidColorBrush(leftColor), Mode = BindingMode.OneTime };
            }
            else
            {
                var lftExt = new DynamicResourceExtension(_frmKey);
                left = lftExt.ProvideValue(serviceProvider);
            }

            IBinding right;
            if (Color.TryParse(_toKey, out var rightColor))
            {
                right = new Binding { Source = new ImmutableSolidColorBrush(rightColor), Mode = BindingMode.OneTime };
            }
            else
            {
                var rgtExt = new DynamicResourceExtension(_toKey);
                right = rgtExt.ProvideValue(serviceProvider);
            }

            return new MultiBinding()
            {
                Bindings = new IBinding[] { left, right },
                Converter = new DivideConverter(_relate)
            };
        }
    }

    class DivideConverter : IMultiValueConverter
    {
        public double Relate { get; }

        public DivideConverter(double relate)
        {
            Relate = relate;
        }

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            Color colL;
            if (values[0] is ISolidColorBrush bl)
                colL = bl.Color;
            else if (values[0] is Color cl)
                colL = cl;
            else
                return values[0];

            Color colR;
            if (values[1] is ISolidColorBrush br)
                colR = br.Color;
            else if (values[1] is Color cr)
                colR = cr;
            else
                return values[0];

            static byte Calc(byte l, byte r, double d)
                => (byte)(l * (1 - d) + r * d);

            return new SolidColorBrush(
                        Color.FromArgb(
                            Calc(colL.A, colR.A, Relate),
                            Calc(colL.R, colR.R, Relate),
                            Calc(colL.G, colR.G, Relate),
                            Calc(colL.B, colR.B, Relate)));
        }
    }
}
