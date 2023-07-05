using System.Collections;
using System.Collections.Generic;

namespace Markdown.Avalonia.Html.Core.Utils
{
    internal static class EnumerableExt
    {
        public static bool TryCast<T>(this IEnumerable list, out List<T> casts)
        {
            casts = new List<T>();

            foreach (var e in list)
            {
                if (e is T t) casts.Add(t);
                else return false;
            }

            return true;
        }

        public static T[] Empty<T>() => EmptyArray<T>.Value;
    }

    internal class EmptyArray<T>
    {
        // net45 dosen't have Array.Empty<T>()
#pragma warning disable CA1825
        public static T[] Value = new T[0];
#pragma warning restore CA1825
    }
}
