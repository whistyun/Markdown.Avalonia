using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorDocument.Avalonia
{
    public class SelectionList : IList<DocumentElement>
    {
        private SelectDirection _direction;
        private SelectRange _range;
        private IList<DocumentElement> _elements;

        public SelectionList(SelectDirection direction, SelectRange range, IList<DocumentElement> elements)
        {
            _direction = direction;
            _range = range;
            _elements = elements;
        }

        public SelectDirection Direction => _direction;

        public DocumentElement this[int index]
        {
            get => _elements[index];
            set => throw new InvalidOperationException();
        }

        public int Count => _elements.Count;

        public bool IsReadOnly => true;

        public void Add(DocumentElement item) => throw new InvalidOperationException();

        public void Clear() => throw new InvalidOperationException();

        public bool Contains(DocumentElement item) => _elements.Contains(item);

        public void CopyTo(DocumentElement[] array, int arrayIndex) => _elements.CopyTo(array, arrayIndex);

        public IEnumerator<DocumentElement> GetEnumerator() => _elements.GetEnumerator();

        public int IndexOf(DocumentElement item) => _elements.IndexOf(item);

        public void Insert(int index, DocumentElement item) => throw new InvalidOperationException();

        public bool Remove(DocumentElement item) => throw new InvalidOperationException();

        public void RemoveAt(int index) => throw new InvalidOperationException();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public enum SelectDirection
    {
        Forward, Backward
    }

    public enum SelectRange
    {
        Part = 0b0001,
        Begin = 0b0011,
        End = 0b0101,
        Fill = 0b0111,
    }
}
