using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Markdown.Avalonia.Parsers
{
    abstract class Parser<T>
    {
        public Regex Pattern { get; }

        public string Name { get; }

        public Parser(Regex pattern, string name)
        {
            Pattern = pattern;
            Name = name;
        }

        public abstract IEnumerable<T> Convert(
            string text, Match firstMatch, ParseStatus status,
            IMarkdownEngine engine,
            out int parseTextBegin, out int parseTextEnd);
    }

    static class Parser
    {
        public static Parser<T> Create<T>(Regex pattern, string name, Func<Match, T> v1)
            => new Single<T>(pattern, name, v1);

        public static Parser<T> Create<T>(Regex pattern, string name, Func<Match, ParseStatus, T> v2)
            => new Single2<T>(pattern, name, v2);

        public static Parser<T> Create<T>(Regex pattern, string name, Func<Match, IEnumerable<T>> v2)
            => new Multi<T>(pattern, name, v2);

        public static Parser<T> Create<T>(Regex pattern, string name, Func<Match, ParseStatus, IEnumerable<T>> v2)
            => new Multi2<T>(pattern, name, v2);

        abstract class Wrapper<T> : Parser<T>
        {
            public Wrapper(Regex pattern, string name) : base(pattern, name)
            {
            }

            public override IEnumerable<T> Convert(
                string text, Match firstMatch, ParseStatus status,
                IMarkdownEngine engine,
                out int parseTextBegin, out int parseTextEnd)
            {
                parseTextBegin = firstMatch.Index;
                parseTextEnd = parseTextBegin + firstMatch.Length;
                return Convert(firstMatch, status);
            }

            public abstract IEnumerable<T> Convert(Match match, ParseStatus status);
        }

        sealed class Single<T> : Wrapper<T>
        {
            private readonly Func<Match, T> converter;

            public Single(Regex pattern, string name, Func<Match, T> converter) : base(pattern, name)
            {
                this.converter = converter;
            }

            public override IEnumerable<T> Convert(Match match, ParseStatus status)
            {
                yield return converter(match);
            }
        }

        sealed class Single2<T> : Wrapper<T>
        {
            private readonly Func<Match, ParseStatus, T> converter;

            public Single2(Regex pattern, string name, Func<Match, ParseStatus, T> converter) : base(pattern, name)
            {
                this.converter = converter;
            }

            public override IEnumerable<T> Convert(Match match, ParseStatus status)
            {
                yield return converter(match, status);
            }
        }

        sealed class Multi<T> : Wrapper<T>
        {
            private readonly Func<Match, IEnumerable<T>> converter;

            public Multi(Regex pattern, string name, Func<Match, IEnumerable<T>> converter) : base(pattern, name)
            {
                this.converter = converter;
            }

            public override IEnumerable<T> Convert(Match match, ParseStatus status)
                => converter(match);
        }

        sealed class Multi2<T> : Wrapper<T>
        {
            private readonly Func<Match, ParseStatus, IEnumerable<T>> converter;

            public Multi2(Regex pattern, string name, Func<Match, ParseStatus, IEnumerable<T>> converter) : base(pattern, name)
            {
                this.converter = converter;
            }

            public override IEnumerable<T> Convert(Match match, ParseStatus status)
                => converter(match, status);
        }
    }
}
