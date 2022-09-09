using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

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
        private readonly string _foregroundName;

        public SyntaxHighlightWrapperExtension(string colorKey)
        {
            this._foregroundName = colorKey;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var dyExt = new DynamicResourceExtension(_foregroundName);
            var brush = dyExt.ProvideValue(serviceProvider);

            var tag = new Binding(nameof(TextEditor.Tag))
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.Self)
            };


            return new MultiBinding()
            {
                Bindings = new IBinding[] { brush, tag },
                Converter = new SyntaxHighlightWrapperConverter()
            };
        }

        class SyntaxHighlightWrapperConverter : IMultiValueConverter
        {
            public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
            {
                string codeLang = values[1] is string l ? l : null;

                if (String.IsNullOrEmpty(codeLang))
                    return null;

                var highlight = HighlightingManager.Instance.GetDefinitionByExtension("." + codeLang);
                if (highlight is null) return null;

                Color foreColor = values[0] is SolidColorBrush cBrush ? cBrush.Color : values[0] is Color cColor ? cColor : Colors.Black;

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

    public class HighlightWrapper : IHighlightingDefinition
    {
        private readonly IHighlightingDefinition _baseDef;
        private readonly Color _foreColor;
        private readonly Dictionary<HighlightingRuleSet, HighlightingRuleSet> _converted;
        private readonly Dictionary<string, HighlightingRuleSet> _namedRuleSet;
        private readonly Dictionary<string, HighlightingColor> _namedColors;

        public HighlightWrapper(IHighlightingDefinition baseDef, Color foreColor)
        {
            _baseDef = baseDef;
            _foreColor = foreColor;

            _converted = new Dictionary<HighlightingRuleSet, HighlightingRuleSet>();
            _namedRuleSet = new Dictionary<string, HighlightingRuleSet>();
            _namedColors = new Dictionary<string, HighlightingColor>();

            foreach (var color in baseDef.NamedHighlightingColors)
            {
                var name = color.Name;

                var newCol = color.Clone();
                newCol.Foreground = color.Foreground is null ?
                    null : new MixHighlightingBrush(color.Foreground, foreColor);
                _namedColors[name] = newCol;
            }

            MainRuleSet = Wrap(baseDef.MainRuleSet);
        }

        public string Name => "Re:" + _baseDef.Name;
        public HighlightingRuleSet MainRuleSet { get; }
        public IEnumerable<HighlightingColor> NamedHighlightingColors => _namedColors.Values;
        public IDictionary<string, string> Properties => _baseDef.Properties;

        public HighlightingColor GetNamedColor(string name)
        {
            return _namedColors.TryGetValue(name, out var color) ? color : null;
        }

        public HighlightingRuleSet GetNamedRuleSet(string name)
        {
            return _namedRuleSet.TryGetValue(name, out var rset) ? rset : null;
        }

        private HighlightingRuleSet Wrap(HighlightingRuleSet ruleSet)
        {
            if (ruleSet is null) return null;

            if (!String.IsNullOrEmpty(ruleSet.Name)
                && _namedRuleSet.TryGetValue(ruleSet.Name, out var cachedRule))
                return cachedRule;

            if (_converted.TryGetValue(ruleSet, out var cachedRule2))
                return cachedRule2;

            var copySet = new HighlightingRuleSet() { Name = ruleSet.Name };

            _converted[ruleSet] = copySet;
            if (!String.IsNullOrEmpty(copySet.Name))
                _namedRuleSet[copySet.Name] = copySet;

            foreach (var baseSpan in ruleSet.Spans)
            {
                if (baseSpan is null) continue;

                var copySpan = new HighlightingSpan()
                {
                    StartExpression = baseSpan.StartExpression,
                    EndExpression = baseSpan.EndExpression,
                    RuleSet = Wrap(baseSpan.RuleSet),
                    StartColor = Wrap(baseSpan.StartColor),
                    SpanColor = Wrap(baseSpan.SpanColor),
                    EndColor = Wrap(baseSpan.EndColor),
                    SpanColorIncludesStart = baseSpan.SpanColorIncludesStart,
                    SpanColorIncludesEnd = baseSpan.SpanColorIncludesEnd,
                };

                copySet.Spans.Add(copySpan);
            }

            foreach (var baseRule in ruleSet.Rules)
            {
                var copyRule = new HighlightingRule()
                {
                    Regex = baseRule.Regex,
                    Color = Wrap(baseRule.Color)
                };

                copySet.Rules.Add(copyRule);
            }

            return copySet;
        }

        private HighlightingColor Wrap(HighlightingColor color)
        {
            if (color is null) return null;

            if (!String.IsNullOrEmpty(color.Name)
                && _namedColors.TryGetValue(color.Name, out var cachedColor))
                return cachedColor;

            var copyColor = color.Clone();
            copyColor.Foreground = color.Foreground is null ?
                null : new MixHighlightingBrush(color.Foreground, _foreColor);

            if (!String.IsNullOrEmpty(copyColor.Name))
                _namedColors[copyColor.Name] = copyColor;

            return copyColor;
        }
    }

    class MixHighlightingBrush : HighlightingBrush
    {
        private readonly HighlightingBrush _baseBrush;
        private readonly Color _fore;

        public MixHighlightingBrush(HighlightingBrush baseBrush, Color fore)
        {
            this._baseBrush = baseBrush;
            this._fore = fore;
        }

        public override IBrush GetBrush(ITextRunConstructionContext context)
        {
            var originalBrush = _baseBrush.GetBrush(context);

            return (originalBrush is ISolidColorBrush sbrsh) ?
                 new SolidColorBrush(WrapColor(sbrsh.Color)) :
                 originalBrush;
        }

        public override Color? GetColor(ITextRunConstructionContext context)
        {
            if (_baseBrush.GetBrush(context) is ISolidColorBrush sbrsh)
            {
                return WrapColor(sbrsh.Color);
            }
            else
            {
                var colorN = this._baseBrush.GetColor(context);
                return colorN.HasValue ? WrapColor(colorN.Value) : colorN;
            }
        }

        private Color WrapColor(Color color)
        {
            if (color.A == 0) return color;

            var foreMax = Math.Max(_fore.R, Math.Max(_fore.G, _fore.B));
            var tgtHsv = new HSV(color);

            int newValue = tgtHsv.Value + foreMax;
            int newSaturation = tgtHsv.Saturation;
            if (newValue > 255)
            {
                var newSaturation2 = newSaturation - (newValue - 255);
                newValue = 255;

                var sChRtLm = (color.R >= color.G && color.R >= color.B) ? 0.95f * 0.7f :
                              (color.G >= color.R && color.G >= color.B) ? 0.95f :
                                                                           0.95f * 0.5f;

                var sChRt = Math.Max(sChRtLm, newSaturation2 / (float)newSaturation);
                if (Single.IsInfinity(sChRt)) sChRt = sChRtLm;

                newSaturation = (int)(newSaturation * sChRt);
            }

            tgtHsv.Value = (byte)newValue;
            tgtHsv.Saturation = (byte)newSaturation;

            var newColor = tgtHsv.ToColor();
            return newColor;
        }
    }

    struct HSV
    {
        public int Hue;
        public byte Saturation;
        public byte Value;

        public HSV(Color color)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));
            int div = max - min;

            if (div == 0)
            {
                Hue = 0;
                Saturation = 0;
            }
            else
            {
                Hue =
                        (min == color.B) ? 60 * (color.G - color.R) / div + 60 :
                        (min == color.R) ? 60 * (color.B - color.G) / div + 180 :
                                           60 * (color.R - color.B) / div + 300;
                Saturation = (byte)div;
            }

            Value = (byte)max;
        }

        public Color ToColor()
        {
            if (Hue == 0 && Saturation == 0)
            {
                return Color.FromRgb(Value, Value, Value);
            }

            //byte c = Saturation;

            // int HueInt = Hue / 60;

            int x = (int)(Saturation * (1 - Math.Abs((Hue / 60f) % 2 - 1)));

            Color FromRgb(int r, int g, int b)
                => Color.FromRgb((byte)r, (byte)g, (byte)b);


            switch (Hue / 60)
            {
                default:
                case 0: return FromRgb(Value, Value - Saturation + x, Value - Saturation);
                case 1: return FromRgb(Value - Saturation + x, Value, Value - Saturation);
                case 2: return FromRgb(Value - Saturation, Value, Value - Saturation + x);
                case 3: return FromRgb(Value - Saturation, Value - Saturation + x, Value);
                case 4: return FromRgb(Value - Saturation + x, Value - Saturation, Value);
                case 5:
                case 6: return FromRgb(Value, Value - Saturation, Value - Saturation + x);
            }
        }
    }
}
