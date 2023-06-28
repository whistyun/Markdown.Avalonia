using Avalonia.Media;
using AvaloniaEdit.Highlighting;
using System;
using System.Collections.Generic;

namespace Markdown.Avalonia.SyntaxHigh.Extensions
{
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

            MainRuleSet = Wrap(baseDef.MainRuleSet)!;
        }

        public string Name => "Re:" + _baseDef.Name;
        public HighlightingRuleSet MainRuleSet { get; }
        public IEnumerable<HighlightingColor> NamedHighlightingColors => _namedColors.Values;
        public IDictionary<string, string> Properties => _baseDef.Properties;

        public HighlightingColor? GetNamedColor(string name)
        {
            return _namedColors.TryGetValue(name, out var color) ? color : null;
        }

        public HighlightingRuleSet? GetNamedRuleSet(string name)
        {
            return _namedRuleSet.TryGetValue(name, out var rset) ? rset : null;
        }

        private HighlightingRuleSet? Wrap(HighlightingRuleSet ruleSet)
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

        private HighlightingColor? Wrap(HighlightingColor color)
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
}
