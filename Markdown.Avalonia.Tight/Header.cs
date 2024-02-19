using System;

namespace Markdown.Avalonia
{
    public class Header : IEquatable<Header>
    {
        public int Level { get; }
        public string Text { get; }

        public Header(int lv, string txt)
        {
            Level = lv;
            Text = txt;
        }

        public override int GetHashCode()
            => Level + Text.GetHashCode();

        public override bool Equals(object? obj)
            => obj is Header arg ? Equals(arg) : false;

        public bool Equals(Header? other)
            => Level == other.Level && Text == other.Text;

        public static bool operator !=(Header? left, Header? right)
            => !(left == right);

        public static bool operator ==(Header? left, Header? right)
            => left is not null ? left.Equals(right) :
               right is not null ? false :
               true;
    }
}
