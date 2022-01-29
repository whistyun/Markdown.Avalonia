using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Markdown.Avalonia.Parsers
{
    static class Parser
    {
        public static Parser<T> Create<T>(Regex pattern, Func<Match, T> v1, Func<Match, T> v2)
            => new Single<T>(pattern, v1 ?? v2);

        public static Parser<T> Create<T>(Regex pattern, Func<Match, T> v1, Func<Match, ParseStatus, T> v2)
            => v1 is null ? (Parser<T>)new Single2<T>(pattern, v2) : new Single<T>(pattern, v1);

        public static Parser<T> Create<T>(Regex pattern, Func<Match, T> v1, Func<Match, IEnumerable<T>> v2)
            => v1 is null ? (Parser<T>)new Multi<T>(pattern, v2) : new Single<T>(pattern, v1);

        public static Parser<T> Create<T>(Regex pattern, Func<Match, T> v1, Func<Match, ParseStatus, IEnumerable<T>> v2)
            => v1 is null ? (Parser<T>)new Multi2<T>(pattern, v2) : new Single<T>(pattern, v1);

        sealed class Single<T> : Parser<T>
        {
            private readonly Func<Match, T> converter;

            public Single(Regex pattern, Func<Match, T> converter) : base(pattern)
            {
                this.converter = converter;
            }

            public override IEnumerable<T> Convert(Match match, ParseStatus status)
            {
                yield return converter(match);
            }
        }

        sealed class Single2<T> : Parser<T>
        {
            private readonly Func<Match, ParseStatus, T> converter;

            public Single2(Regex pattern, Func<Match, ParseStatus, T> converter) : base(pattern)
            {
                this.converter = converter;
            }

            public override IEnumerable<T> Convert(Match match, ParseStatus status)
            {
                yield return converter(match, status);
            }
        }

        sealed class Multi<T> : Parser<T>
        {
            private readonly Func<Match, IEnumerable<T>> converter;

            public Multi(Regex pattern, Func<Match, IEnumerable<T>> converter) : base(pattern)
            {
                this.converter = converter;
            }

            public override IEnumerable<T> Convert(Match match, ParseStatus status)
                => converter(match);
        }

        sealed class Multi2<T> : Parser<T>
        {
            private readonly Func<Match, ParseStatus, IEnumerable<T>> converter;

            public Multi2(Regex pattern, Func<Match, ParseStatus, IEnumerable<T>> converter) : base(pattern)
            {
                this.converter = converter;
            }

            public override IEnumerable<T> Convert(Match match, ParseStatus status)
                => converter(match, status);
        }
    }

    abstract class Parser<T>
    {
        private Regex pattern;

        public Parser(Regex pattern)
        {
            this.pattern = pattern;
        }

        public Match Match(string text, int index, int length, ParseStatus status) => pattern.Match(text, index, length);

        public abstract IEnumerable<T> Convert(Match match, ParseStatus status);
    }
}
