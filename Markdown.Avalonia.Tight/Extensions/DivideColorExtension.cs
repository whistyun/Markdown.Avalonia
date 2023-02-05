using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
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
                left = new StaticBinding(leftColor);
            }
            else
            {
                var lftExt = new DynamicResourceExtension(_frmKey);
                left = lftExt.ProvideValue(serviceProvider);
            }

            IBinding right;
            if (Color.TryParse(_toKey, out var rightColor))
            {
                right = new StaticBinding(rightColor);
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

    class StaticBinding : IBinding
    {
        private readonly object _value;

        public StaticBinding(object value)
        {
            _value = value;
        }

        public InstancedBinding? Initiate(AvaloniaObject target, AvaloniaProperty? targetProperty, object? anchor = null, bool enableDataValidation = false)
        {
            return InstancedBinding.OneWay(new StaticBindingObservable(_value));
        }

        class StaticBindingObservable : IObservable<object>
        {
            object Value { get; set; }

            public StaticBindingObservable(object value)
            {
                Value = value;
            }

            private readonly ConcurrentDictionary<StaticTicket, IObserver<object>> _cache = new();

            public IDisposable Subscribe(IObserver<object> observer)
            {
                //StaticTicket ticket;
                //do
                //{
                //    ticket = new StaticTicket(this);
                //} while (Cache.ContainsKey(ticket));
                //
                //Cache[ticket] = observer;
                //
                //return ticket;

                observer.OnNext(Value);

                return new DummyDisposable();
            }

            public void Remove(StaticTicket nemui)
            {
                _cache.TryRemove(nemui, out var _);
            }
        }

        class DummyDisposable : IDisposable
        {
            public void Dispose() { }
        }

        class StaticTicket : IDisposable
        {
            private readonly StaticBindingObservable _owner;
            private readonly Guid _guid = new();

            public StaticTicket(StaticBindingObservable owner)
            {
                this._owner = owner;
            }


            ~StaticTicket() => Dispose();

            public override int GetHashCode()
                => _guid.GetHashCode();

            public override bool Equals(object? obj)
            {
                if (obj is StaticTicket nemu)
                    return nemu._guid.Equals(nemu._guid);

                return false;
            }

            public void Dispose()
            {
                _owner.Remove(this);
                GC.SuppressFinalize(this);
            }
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
