using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorDocument.Avalonia
{
    internal static class EnumerableExt
    {
        public static EnumerableEx<T> ToEnumerable<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable is List<T> list)
                return new EnumerableExLst<T>(list);
            else if (enumerable is T[] array)
                return new EnumerableExAry<T>(array);

            return new EnumerableExLzy<T>(enumerable);
        }
    }

    internal abstract class EnumerableEx<T> : IEnumerable<T>
    {
        public abstract int Count { get; }

        public abstract T this[int idx] { get; }

        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal class EnumerableExLzy<T> : EnumerableEx<T>
    {
        private Lazy<T[]> _lzy;

        public EnumerableExLzy(IEnumerable<T> enm)
        {
            _lzy = new Lazy<T[]>(() => enm.ToArray());
        }

        public override int Count => _lzy.Value.Length;

        public override T this[int idx] { get => _lzy.Value[idx]; }

        public override IEnumerator<T> GetEnumerator() => ((ICollection<T>)_lzy.Value).GetEnumerator();
    }

    internal class EnumerableExAry<T> : EnumerableEx<T>
    {
        private T[] _array;

        public EnumerableExAry(T[] array)
        {
            _array = array;
        }

        public override int Count => _array.Length;

        public override T this[int idx] { get => _array[idx]; }

        public override IEnumerator<T> GetEnumerator() => ((ICollection<T>)_array).GetEnumerator();
    }

    internal class EnumerableExLst<T> : EnumerableEx<T>
    {
        private IList<T> _list;

        public EnumerableExLst(IList<T> array)
        {
            _list = array;
        }

        public override int Count => _list.Count;

        public override T this[int idx] { get => _list[idx]; }

        public override IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
    }
}
