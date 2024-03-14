using Avalonia.Media;
using ColorTextBlock.Avalonia.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ColorTextBlock.Avalonia
{
    public class TextPointer : IEquatable<TextPointer>, IComparable<TextPointer>
    {
        public int Index { get; }
        internal int InternalIndex { get; }
        internal int TrailingLength { get; }
        internal double Distance { get; }
        internal CGeometry Geometry { get; }

        internal int PathDepth => _path.Length;
        internal CInline this[int idx] => _path[idx];

        private CInline[] _path;

        private TextPointer(CInline[] path, CGeometry geometry, int index, int internalIndex, int trallingLength, double distance)
        {
            _path = path;
            Geometry = geometry;
            Index = index;
            InternalIndex = internalIndex;
            TrailingLength = trallingLength;
            Distance = distance;
        }

        internal TextPointer(CRun inline, TextLineGeometry target, CharacterHit charHit, bool isLast)
        {
            _path = new[] { inline };
            Geometry = target;

            if (isLast)
            {
                var lastIdx = charHit.FirstCharacterIndex + charHit.TrailingLength;
                Index = lastIdx - target.Line.FirstTextSourceIndex;
                InternalIndex = lastIdx;
                TrailingLength = 0;
            }
            else
            {
                Index = charHit.FirstCharacterIndex - target.Line.FirstTextSourceIndex;
                InternalIndex = charHit.FirstCharacterIndex;
                TrailingLength = charHit.TrailingLength;
            }
        }

        internal TextPointer(CRun inline, TextLineGeometry target, CharacterHit charHit, double distance, bool isLast) :
            this(inline, target, charHit, isLast)
        {
            Distance = distance;
        }

        internal TextPointer(CGeometry inline)
        {
            _path = new[] { inline.Owner };
            Geometry = inline;
            Index = 0;
            InternalIndex = 0;
            TrailingLength = 0;
        }

        internal TextPointer(CGeometry inline, int idx, double distance)
        {
            _path = new[] { inline.Owner };
            Geometry = inline;
            Index = idx;
            InternalIndex = 0;
            TrailingLength = 0;
            Distance = distance;
        }

        internal TextPointer(CTextBlock host, int idx)
        {
            _path = Array.Empty<CInline>();
            Index = idx;
            InternalIndex = 0;
            TrailingLength = 0;
        }

        internal TextPointer Wrap(CInline owner, int indexAdding)
        {
            var path = new List<CInline>(_path.Length + 1);
            path.Add(owner);
            path.AddRange(_path);

            return new TextPointer(
                path.ToArray(),
                Geometry,
                Index + indexAdding,
                InternalIndex,
                TrailingLength,
                Distance);
        }

        internal TextPointer Wrap(CTextBlock host, int indexAdding)
        {
            return new TextPointer(
                _path,
                Geometry,
                Index + indexAdding,
                InternalIndex,
                TrailingLength,
                Distance);
        }

        public override int GetHashCode()
        {
            return _path.Sum(e => e.GetHashCode())
                + Index.GetHashCode()
                + InternalIndex.GetHashCode()
                + TrailingLength.GetHashCode();
        }

        public bool Equals(TextPointer? other)
        {
            return PathDepth == other.PathDepth
                && Enumerable.Range(0, PathDepth).All(i => Object.ReferenceEquals(_path[i], other[i]))
                && Index == other.Index
                && InternalIndex == other.InternalIndex
                && TrailingLength == other.TrailingLength;
        }

        public int CompareTo(TextPointer? other)
            => other is not null ? Index.CompareTo(other.Index) : throw new ArgumentNullException(nameof(other));

        public static bool operator <(TextPointer left, TextPointer right) => left.CompareTo(right) < 0;
        public static bool operator >(TextPointer left, TextPointer right) => left.CompareTo(right) > 0;

        public static bool operator <=(TextPointer left, TextPointer right) => left.CompareTo(right) <= 0;
        public static bool operator >=(TextPointer left, TextPointer right) => left.CompareTo(right) >= 0;
    }

    public interface ITextPointerHandleable
    {
        /// <summary>
        /// Calcuates position from relative coordinates. 
        /// The origin of the relative coordinates is based on CTextBlock.
        /// </summary>
        /// <param name="x">The x coordinate of caret position on CTextBlock</param>
        /// <param name="y">The y coordinate of caret position on CTextBlock</param>
        /// <returns></returns>
        public TextPointer CalcuatePointerFrom(double x, double y);

        public TextPointer CalcuatePointerFrom(int index);

        public TextPointer GetBegin();

        public TextPointer GetEnd();
    }

    public interface ISelectable
    {
        public void ClearSelection();
        public void Select(TextPointer start, TextPointer end);
    }
}
