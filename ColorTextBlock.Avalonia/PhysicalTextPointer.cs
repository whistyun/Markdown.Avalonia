using Avalonia.Media;
using ColorTextBlock.Avalonia.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ColorTextBlock.Avalonia
{
    public sealed class PhysicalTextPointer : TextPointer, IEquatable<PhysicalTextPointer>, IComparable<PhysicalTextPointer>
    {
        public int Index { get; }
        internal int InternalIndex { get; }
        internal int TrailingLength { get; }
        internal double Distance { get; }
        internal CGeometry Geometry { get; }

        internal int PathDepth => _path.Length;
        internal CInline this[int idx] => _path[idx];

        private readonly CInline[] _path;

        private PhysicalTextPointer(CInline[] path, CGeometry geometry, int index, int internalIndex, int trallingLength, double distance)
        {
            _path = path;
            Geometry = geometry;
            Index = index;
            InternalIndex = internalIndex;
            TrailingLength = trallingLength;
            Distance = distance;
        }

        internal PhysicalTextPointer(CRun inline, TextLineGeometry target, CharacterHit charHit, bool isLast)
        {
            _path = new[] { inline };
            Geometry = target;
            Index = isLast
                ? charHit.FirstCharacterIndex + charHit.TrailingLength - target.Line.FirstTextSourceIndex
                : charHit.FirstCharacterIndex - target.Line.FirstTextSourceIndex;

            if (isLast)
            {
                var lastIdx = charHit.FirstCharacterIndex + charHit.TrailingLength;
                InternalIndex = lastIdx;
                TrailingLength = 0;
            }
            else
            {
                InternalIndex = charHit.FirstCharacterIndex;
                TrailingLength = charHit.TrailingLength;
            }
        }

        internal PhysicalTextPointer(CRun inline, TextLineGeometry target, CharacterHit charHit, double distance, bool isLast)
            : this(inline, target, charHit, isLast)
        {
            Distance = distance;
        }

        internal PhysicalTextPointer(CGeometry inline)
        {
            _path = new[] { inline.Owner };
            Geometry = inline;
            Index = 0;
            InternalIndex = 0;
            TrailingLength = 0;
        }

        internal PhysicalTextPointer(CGeometry inline, int idx, double distance)
        {
            _path = new[] { inline.Owner };
            Geometry = inline;
            Index = idx;
            InternalIndex = 0;
            TrailingLength = 0;
            Distance = distance;
        }

        internal PhysicalTextPointer Wrap(CInline owner, int indexAdding)
        {
            var path = new List<CInline>(_path.Length + 1);
            path.Add(owner);
            path.AddRange(_path);

            return new PhysicalTextPointer(
                path.ToArray(),
                Geometry,
                Index + indexAdding,
                InternalIndex,
                TrailingLength,
                Distance);
        }

        internal PhysicalTextPointer Wrap(CTextBlock host, int indexAdding)
        {
            return new PhysicalTextPointer(
                _path,
                Geometry,
                Index + indexAdding,
                InternalIndex,
                TrailingLength,
                Distance);
        }

        public override bool Equals(object? obj) => Equals(obj as PhysicalTextPointer);

        public override bool Equals(TextPointer? other) => Equals(other as PhysicalTextPointer);

        public bool Equals(PhysicalTextPointer? other)
        {
            if (other is null)
                return false;

            return PathDepth == other.PathDepth
                && Enumerable.Range(0, PathDepth).All(i => ReferenceEquals(_path[i], other._path[i]))
                && Index == other.Index
                && InternalIndex == other.InternalIndex
                && TrailingLength == other.TrailingLength;
        }

        public int CompareTo(PhysicalTextPointer? other)
        {
            if (other is null)
                throw new ArgumentNullException(nameof(other));

            var depthCompare = PathDepth.CompareTo(other.PathDepth);
            if (depthCompare != 0)
                return depthCompare;

            for (var i = 0; i < PathDepth; i++)
            {
                if (ReferenceEquals(_path[i], other._path[i]))
                    continue;

                var pathCompare = RuntimeHelpers.GetHashCode(_path[i]).CompareTo(RuntimeHelpers.GetHashCode(other._path[i]));
                if (pathCompare != 0)
                    return pathCompare;
            }

            var indexCompare = Index.CompareTo(other.Index);
            if (indexCompare != 0)
                return indexCompare;

            var internalIndexCompare = InternalIndex.CompareTo(other.InternalIndex);
            if (internalIndexCompare != 0)
                return internalIndexCompare;

            return TrailingLength.CompareTo(other.TrailingLength);
        }

        public static bool operator ==(PhysicalTextPointer? left, PhysicalTextPointer? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (left is null || right is null)
                return false;
            return left.Equals(right);
        }

        public static bool operator !=(PhysicalTextPointer? left, PhysicalTextPointer? right) => !(left == right);

        public static bool operator <(PhysicalTextPointer left, PhysicalTextPointer right) => left.CompareTo(right) < 0;
        public static bool operator >(PhysicalTextPointer left, PhysicalTextPointer right) => left.CompareTo(right) > 0;
        public static bool operator <=(PhysicalTextPointer left, PhysicalTextPointer right) => left.CompareTo(right) <= 0;
        public static bool operator >=(PhysicalTextPointer left, PhysicalTextPointer right) => left.CompareTo(right) >= 0;

        public override int GetHashCode()
        {
            return _path.Sum(e => e.GetHashCode())
                + Index.GetHashCode()
                + InternalIndex.GetHashCode()
                + TrailingLength.GetHashCode();
        }
    }
}
