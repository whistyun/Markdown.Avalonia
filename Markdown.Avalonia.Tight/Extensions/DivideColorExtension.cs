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
        private string FrmKey;
        private string ToKey;
        private double Relate;

        public DivideColorExtension(string frm, string to, double relate)
        {
            this.FrmKey = frm;
            this.ToKey = to;
            this.Relate = relate;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            IBinding left;
            if (Color.TryParse(FrmKey, out var leftColor))
            {
                left = new StaticBinding(leftColor);
            }
            else
            {
                var lftExt = new DynamicResourceExtension(FrmKey);
                left = lftExt.ProvideValue(serviceProvider);
            }

            IBinding right;
            if (Color.TryParse(ToKey, out var rightColor))
            {
                right = new StaticBinding(rightColor);
            }
            else
            {
                var rgtExt = new DynamicResourceExtension(ToKey);
                right = rgtExt.ProvideValue(serviceProvider);
            }

            return new MultiBinding()
            {
                Bindings = new IBinding[] { left, right },
                Converter = new DivideConverter(Relate)
            };
        }
    }

    class StaticBinding : IBinding
    {
        object Value;

        public StaticBinding(object value)
        {
            Value = value;
        }

        public InstancedBinding? Initiate(IAvaloniaObject target, AvaloniaProperty? targetProperty, object? anchor = null, bool enableDataValidation = false)
        {
            return InstancedBinding.OneWay(new StaticBindingObservable(Value));
        }

        class StaticBindingObservable : IObservable<object>
        {
            object Value { get; set; }

            public StaticBindingObservable(object value)
            {
                Value = value;
            }

            ConcurrentDictionary<StaticTicket, IObserver<object>> Cache
                = new ConcurrentDictionary<StaticTicket, IObserver<object>>();

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
                Cache.TryRemove(nemui, out var notinterest);
            }
        }

        class DummyDisposable : IDisposable
        {
            public void Dispose() { }
        }

        class StaticTicket : IDisposable
        {

            private StaticBindingObservable Owner;
            private Guid guid = new Guid();

            public StaticTicket(StaticBindingObservable owner)
            {
                this.Owner = owner;
            }


            ~StaticTicket() => Dispose();

            public override int GetHashCode()
                => guid.GetHashCode();

            public override bool Equals(object obj)
            {
                if (obj is StaticTicket nemu)
                    return nemu.guid.Equals(nemu.guid);

                return false;
            }

            public void Dispose()
            {
                Owner.Remove(this);
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

            byte Calc(byte l, byte r, double d)
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
