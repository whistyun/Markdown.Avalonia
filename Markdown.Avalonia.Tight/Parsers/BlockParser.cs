using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Markdown.Avalonia.Parsers
{
    public delegate Control? ParseWithPositionChange(string text, Match match, out int parseTextBegin, out int parseTextEnd);

    public abstract class BlockParser
    {
        public Regex Pattern { get; }

        public string Name { get; }

        public BlockParser(Regex pattern, string name)
        {
            Pattern = pattern;
            Name = name;
        }

        public abstract IEnumerable<Control>? Convert(
            string text, Match firstMatch, ParseStatus status,
            IMarkdownEngine engine,
            out int parseTextBegin, out int parseTextEnd);

        public static BlockParser New(Regex pattern, string name, Func<Match, Control?> v1)
            => new Single(pattern, name, v1);

        public static BlockParser New(Regex pattern, string name, Func<Match, ParseStatus, Control?> v2)
            => new Single2(pattern, name, v2);

        public static BlockParser New(Regex pattern, string name, Func<Match, IEnumerable<Control>?> v2)
            => new Multi(pattern, name, v2);

        public static BlockParser New(Regex pattern, string name, Func<Match, ParseStatus, IEnumerable<Control>?> v2)
            => new Multi2(pattern, name, v2);

        public static BlockParser New(Regex pattern, string name, ParseWithPositionChange v2)
            => new ParsePosChange(pattern, name, v2);

        abstract class Wrapper : BlockParser
        {
            public Wrapper(Regex pattern, string name) : base(pattern, name)
            {
            }

            public override IEnumerable<Control>? Convert(
                string text, Match firstMatch, ParseStatus status,
                IMarkdownEngine engine,
                out int parseTextBegin, out int parseTextEnd)
            {
                parseTextBegin = firstMatch.Index;
                parseTextEnd = parseTextBegin + firstMatch.Length;
                return Convert(firstMatch, status);
            }

            public abstract IEnumerable<Control>? Convert(Match match, ParseStatus status);
        }

        sealed class Single : Wrapper
        {
            private readonly Func<Match, Control?> converter;

            public Single(Regex pattern, string name, Func<Match, Control?> converter) : base(pattern, name)
            {
                this.converter = converter;
            }

            public override IEnumerable<Control>? Convert(Match match, ParseStatus status)
            {
                return converter(match) is Control ctrl ? new[] { ctrl } : null;
            }
        }

        sealed class Single2 : Wrapper
        {
            private readonly Func<Match, ParseStatus, Control?> converter;

            public Single2(Regex pattern, string name, Func<Match, ParseStatus, Control?> converter) : base(pattern, name)
            {
                this.converter = converter;
            }

            public override IEnumerable<Control>? Convert(Match match, ParseStatus status)
            {
                return converter(match, status) is Control ctrl ? new[] { ctrl } : null;
            }
        }

        sealed class Multi : Wrapper
        {
            private readonly Func<Match, IEnumerable<Control>?> converter;

            public Multi(Regex pattern, string name, Func<Match, IEnumerable<Control>?> converter) : base(pattern, name)
            {
                this.converter = converter;
            }

            public override IEnumerable<Control>? Convert(Match match, ParseStatus status)
                => converter(match);
        }

        sealed class Multi2 : Wrapper
        {
            private readonly Func<Match, ParseStatus, IEnumerable<Control>?> converter;

            public Multi2(Regex pattern, string name, Func<Match, ParseStatus, IEnumerable<Control>?> converter) : base(pattern, name)
            {
                this.converter = converter;
            }

            public override IEnumerable<Control>? Convert(Match match, ParseStatus status)
                => converter(match, status);
        }

        sealed class ParsePosChange : BlockParser
        {
            private ParseWithPositionChange converter;

            public ParsePosChange(Regex pattern, string name, ParseWithPositionChange converter) : base(pattern, name)
            {
                this.converter = converter;
            }

            public override IEnumerable<Control>? Convert(string text, Match firstMatch, ParseStatus status, IMarkdownEngine engine, out int parseTextBegin, out int parseTextEnd)
            {
                return converter(text, firstMatch, out parseTextBegin, out parseTextEnd) is Control ctrl ?
                    new[] { ctrl } : null;
            }
        }
    }
}
