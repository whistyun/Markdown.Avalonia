using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Markdown.Avalonia
{
    public class HeaderScrolledEventArgs : EventArgs, IEquatable<HeaderScrolledEventArgs>
    {
        public IReadOnlyList<Header> Tree { get; }
        public IReadOnlyList<Header> Viewing { get; }

        public HeaderScrolledEventArgs(IList<Header> tree, IList<Header> viewing)
        {
            Tree = new ReadOnlyCollection<Header>(tree);
            Viewing = new ReadOnlyCollection<Header>(viewing);
        }

        public override int GetHashCode()
            => Tree.Sum(e => e.GetHashCode()) + Viewing.Sum(e => e.GetHashCode());

        public override bool Equals(object? obj)
            => obj is HeaderScrolledEventArgs arg ? Equals(arg) : false;

        public bool Equals(HeaderScrolledEventArgs? other)
        {
            if (other is null)
                return false;

            return Enumerable.SequenceEqual(Tree, other.Tree)
                && Enumerable.SequenceEqual(Viewing, other.Viewing);
        }

        public static bool operator !=(HeaderScrolledEventArgs? left, HeaderScrolledEventArgs? right)
            => !(left == right);

        public static bool operator ==(HeaderScrolledEventArgs? left, HeaderScrolledEventArgs? right)
            => left is not null ? left.Equals(right) :
               right is not null ? false :
               true;
    }
}
