using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Data.Core;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Markdown.Avalonia.Extensions
{
    public class DivideColorExtension : MarkupExtension
    {
        private readonly string _frmKey;
        private readonly string _toKey;
        private readonly double _relate;

        public DivideColorExtension(string frm, string to, double relate)
        {
            _frmKey = frm;
            _toKey = to;
            _relate = relate;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            BindingBase left;
            if (Color.TryParse(_frmKey, out var leftColor))
            {
                var leftBrush = new ImmutableSolidColorBrush(leftColor);
                left = CompiledBinding.Create<IBrush, IBrush>(v=>v, leftColor);
            }
            else
            {
                return Blend(leftColor, rightColor, _relate);
            }

            BindingBase right;
            if (Color.TryParse(_toKey, out var rightColor))
            {
                var rightBrush = new ImmutableSolidColorBrush(rightColor);
                right = CompiledBinding.Create<IBrush, IBrush>(v=>v, rightColor);
            }

            if (!leftIsConst && rightIsConst)
            {
                var leftExt = new DynamicResourceExtension(_frmKey);
                return new MultiBinding()
                {
                    Bindings = new IBinding[]
                    {
                        (IBinding)leftExt.ProvideValue(serviceProvider)!,
                        new Binding { Source = new SolidColorBrush(rightColor) },
                    },
                    Converter = new DivideConstantDynamicConverter(_relate, fromLeft: false),
                };
            }

            // Both dynamic: MultiBinding with only DynamicResource bindings.
            var lftExt = new DynamicResourceExtension(_frmKey);
            var rgtExt = new DynamicResourceExtension(_toKey);
            return new MultiBinding()
            {
                Bindings = new IBinding[]
                {
                    (IBinding)lftExt.ProvideValue(serviceProvider)!,
                    (IBinding)rgtExt.ProvideValue(serviceProvider)!,
                },
                Converter = new DivideConverter(_relate),
            };
        }

        internal static SolidColorBrush Blend(Color colL, Color colR, double relate)
        {
            static byte Calc(byte l, byte r, double d)
                => (byte)(l * (1 - d) + r * d);
            return new SolidColorBrush(Color.FromArgb(
                Calc(colL.A, colR.A, relate),
                Calc(colL.R, colR.R, relate),
                Calc(colL.G, colR.G, relate),
                Calc(colL.B, colR.B, relate)));
        }
    }

    /// <summary>Blends constant (one binding) with dynamic (other binding) for MultiBinding; used when one side is constant.</summary>
    internal sealed class DivideConstantDynamicConverter : IMultiValueConverter
    {
        private readonly double _relate;
        private readonly bool _fromLeft;

        public DivideConstantDynamicConverter(double relate, bool fromLeft)
        {
            _relate = relate;
            _fromLeft = fromLeft;
        }

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count < 2) return null;
            Color col0 = ToColor(values[0]);
            Color col1 = ToColor(values[1]);
            return _fromLeft ? DivideColorExtension.Blend(col0, col1, _relate) : DivideColorExtension.Blend(col0, col1, _relate);
        }

        private static Color ToColor(object? value)
        {
            if (value is ISolidColorBrush br) return br.Color;
            if (value is Color c) return c;
            return default;
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
                return AvaloniaProperty.UnsetValue;

            Color colR;
            if (values[1] is ISolidColorBrush br)
                colR = br.Color;
            else if (values[1] is Color cr)
                colR = cr;
            else
                return new ImmutableSolidColorBrush(colL);

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
