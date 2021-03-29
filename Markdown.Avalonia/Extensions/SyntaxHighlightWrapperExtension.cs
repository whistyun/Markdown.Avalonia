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

namespace Markdown.Avalonia.Extensions
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
        string ForegroundName;

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
        IHighlightingDefinition baseDef;
        Color foreColor;

        Dictionary<HighlightingRuleSet, HighlightingRuleSet> Converted;
        Dictionary<string, HighlightingRuleSet> NamedRuleSet;
        Dictionary<string, HighlightingColor> NamedColors;

        public HighlightWrapper(IHighlightingDefinition baseDef, Color foreColor)
        {
            this.baseDef = baseDef;
            this.foreColor = foreColor;

            Converted = new Dictionary<HighlightingRuleSet, HighlightingRuleSet>();
            NamedRuleSet = new Dictionary<string, HighlightingRuleSet>();
            NamedColors = new Dictionary<string, HighlightingColor>();

            foreach (var color in baseDef.NamedHighlightingColors)
            {
                var name = color.Name;

                var newCol = color.Clone();
                newCol.Foreground = color.Foreground is null ?
                    null : new MixHighlightingBrush(color.Foreground, foreColor);
                NamedColors[name] = newCol;
            }

            MainRuleSet = Wrap(baseDef.MainRuleSet);
        }

        public string Name => "Re:" + baseDef.Name;
        public HighlightingRuleSet MainRuleSet { get; }
        public IEnumerable<HighlightingColor> NamedHighlightingColors => NamedColors.Values;
        public IDictionary<string, string> Properties => baseDef.Properties;

        public HighlightingColor GetNamedColor(string name)
        {
            return NamedColors.TryGetValue(name, out var color) ? color : null;
        }

        public HighlightingRuleSet GetNamedRuleSet(string name)
        {
            return NamedRuleSet.TryGetValue(name, out var rset) ? rset : null;
        }

        private HighlightingRuleSet Wrap(HighlightingRuleSet ruleSet)
        {
            if (ruleSet is null) return null;

            if (!String.IsNullOrEmpty(ruleSet.Name)
                && NamedRuleSet.TryGetValue(ruleSet.Name, out var cachedRule))
                return cachedRule;

            if (Converted.TryGetValue(ruleSet, out var cachedRule2))
                return cachedRule2;

            var copySet = new HighlightingRuleSet();
            copySet.Name = ruleSet.Name;

            Converted[ruleSet] = copySet;
            if (!String.IsNullOrEmpty(copySet.Name))
                NamedRuleSet[copySet.Name] = copySet;

            foreach (var baseSpan in ruleSet.Spans)
            {
                if (baseSpan is null) continue;

                var copySpan = new HighlightingSpan();
                copySpan.StartExpression = baseSpan.StartExpression;
                copySpan.EndExpression = baseSpan.EndExpression;
                copySpan.RuleSet = Wrap(baseSpan.RuleSet);
                copySpan.StartColor = Wrap(baseSpan.StartColor);
                copySpan.SpanColor = Wrap(baseSpan.SpanColor);
                copySpan.EndColor = Wrap(baseSpan.EndColor);
                copySpan.SpanColorIncludesStart = baseSpan.SpanColorIncludesStart;
                copySpan.SpanColorIncludesEnd = baseSpan.SpanColorIncludesEnd;

                copySet.Spans.Add(copySpan);
            }

            foreach (var baseRule in ruleSet.Rules)
            {
                var copyRule = new HighlightingRule();
                copyRule.Regex = baseRule.Regex;
                copyRule.Color = Wrap(baseRule.Color);

                copySet.Rules.Add(copyRule);
            }

            return copySet;
        }

        private HighlightingColor Wrap(HighlightingColor color)
        {
            if (color is null) return null;

            if (!String.IsNullOrEmpty(color.Name)
                && NamedColors.TryGetValue(color.Name, out var cachedColor))
                return cachedColor;

            var copyColor = color.Clone();
            copyColor.Foreground = color.Foreground is null ?
                null : new MixHighlightingBrush(color.Foreground, foreColor);

            if (!String.IsNullOrEmpty(copyColor.Name))
                NamedColors[copyColor.Name] = copyColor;

            return copyColor;
        }
    }

    class MixHighlightingBrush : HighlightingBrush
    {
        HighlightingBrush baseBrush;
        Color fore;

        public MixHighlightingBrush(HighlightingBrush baseBrush, Color fore)
        {
            this.baseBrush = baseBrush;
            this.fore = fore;
        }

        public override IBrush GetBrush(ITextRunConstructionContext context)
        {
            return new SolidColorBrush(GetColor(context).Value);
        }

        public override Color? GetColor(ITextRunConstructionContext context)
        {
            Color color;

            if (baseBrush.GetBrush(context) is ISolidColorBrush sbrsh)
            {
                color = sbrsh.Color;
            }
            else
            {
                var colorN = this.baseBrush.GetColor(context);
                if (colorN.HasValue) color = colorN.Value;
                else return fore;
            }

            if (color.A == 0) return color;

            var foreMax = Math.Max(fore.R, Math.Max(fore.G, fore.B));
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

            int HueInt = Hue / 60;

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
