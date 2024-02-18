using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ColorTextBlock.Avalonia
{
    public class TextPointer : IEquatable<TextPointer>, IComparable<TextPointer>
    {
        private WeakReference<ITextPointerHandleable>[] _path;

        public int Index { get; }
        /// <summary>The x coordinate of caret position on Control like as CTextBlock</summary>
        public double HostPosX { get; }
        /// <summary>The y coordinate of caret position on Control like as CTextBlock</summary>
        public double HostPosY { get; }

        public double Height { get; }

        internal int InternalIndex { get; }
        internal int TrailingLength { get; }

        internal int PathDepth
            => _path.Length;

        public ITextPointerHandleable Host
        {
            get
            {
                if (!_path[0].TryGetTarget(out var host))
                    throw new InvalidOperationException();

                return host;
            }
        }

        public ITextPointerHandleable Last
        {
            get
            {
                if (!_path[_path.Length - 1].TryGetTarget(out var host))
                    throw new InvalidOperationException();

                return host;
            }
        }

        internal ITextPointerHandleable this[int idx]
            => _path[idx].TryGetTarget(out var element) ?
                    element :
                    throw new InvalidOperationException();

        private TextPointer(
            WeakReference<ITextPointerHandleable>[] path, int index,
            int internalIndex, int trallingLength,
            double hostPosX, double hostPosY, double caretHeight)
        {
            _path = path;
            Index = index;
            InternalIndex = internalIndex;
            TrailingLength = trallingLength;
            HostPosX = hostPosX;
            HostPosY = hostPosY;
            Height = caretHeight;
        }

        internal TextPointer(ITextPointerHandleable owner, int idx, double hostPosX, double hostPosY, double caretHeight)
        {
            _path = new[] { new WeakReference<ITextPointerHandleable>(owner) };
            Index = InternalIndex = idx;
            TrailingLength = 0;
            HostPosX = hostPosX;
            HostPosY = hostPosY;
            Height = caretHeight;
        }

        internal TextPointer(
            ITextPointerHandleable owner, TextLine baseLine, CharacterHit charaHit,
            double hostPosX, double hostPosY, double caretHeight)
        {
            _path = new[] { new WeakReference<ITextPointerHandleable>(owner) };
            Index = charaHit.FirstCharacterIndex - baseLine.FirstTextSourceIndex;
            InternalIndex = charaHit.FirstCharacterIndex;
            TrailingLength = charaHit.TrailingLength;
            HostPosX = hostPosX;
            HostPosY = hostPosY;
            Height = caretHeight;
        }

        internal TextPointer Wrap(ITextPointerHandleable owner, double hostPosY, double caretHeight, int appendIndex)
        {
            var path = new WeakReference<ITextPointerHandleable>[_path.Length + 1];
            path[0] = new WeakReference<ITextPointerHandleable>(owner);
            Array.Copy(_path, 0, path, 1, _path.Length);

            return new TextPointer(path, Index + appendIndex, InternalIndex, TrailingLength, HostPosX, hostPosY, caretHeight);
        }

        public override int GetHashCode()
        {
            return _path.Sum(e => e.GetHashCode())
                + HostPosX.GetHashCode()
                + HostPosY.GetHashCode()
                + Height.GetHashCode()
                + InternalIndex.GetHashCode()
                + TrailingLength.GetHashCode();
        }

        public bool Equals(TextPointer? other)
        {
            return PathDepth == other.PathDepth
                && HostPosX == other.HostPosX
                && HostPosY == other.HostPosY
                && Height == other.Height
                && Enumerable.Range(0, PathDepth)
                             .All(i => Object.ReferenceEquals(_path[i], other[i]))
                && InternalIndex == other.InternalIndex
                && TrailingLength == other.TrailingLength;
        }

        public int CompareTo(TextPointer other) => Index.CompareTo(other.Index);

        public static bool operator <(TextPointer left, TextPointer right) => left.CompareTo(right) < 0;
        public static bool operator >(TextPointer left, TextPointer right) => left.CompareTo(right) > 0;

        public static bool operator <=(TextPointer left, TextPointer right) => left.CompareTo(right) <= 0;
        public static bool operator >=(TextPointer left, TextPointer right) => left.CompareTo(right) >= 0;
    }

    public interface ITextPointerHandleable
    {
        /// <summary>
        /// Calcuates next position from the indicated position.
        /// </summary>
        /// <param name="current">The indicated position</param>
        /// <param name="next">The next position. If the indicated position is the end then this is equals to the indicated position.</param>
        /// <returns>Returns true if the indicated position is not equals to the end.</returns>
        // 指定されたテキスト位置から次の位置を計算します。
        public bool TryMoveNext(
            TextPointer current,
#if NETCOREAPP3_0_OR_GREATER
            [MaybeNullWhen(false)]
            out TextPointer? next
#else
            out TextPointer next
#endif
        );
        /// <summary>
        /// Calcuates previous position from the indicated position.
        /// </summary>
        /// <param name="current">The indicated position</param>
        /// <param name="next">The previous position. If the indicated position is the begin then this is equals to the indicated position.</param>
        /// <returns>Returns true if the indicated position is not equals to the end.</returns>
        public bool TryMovePrev(
            TextPointer current,
#if NETCOREAPP3_0_OR_GREATER
            [MaybeNullWhen(false)]
            out TextPointer? prev
#else
            out TextPointer prev
#endif
        );

        /// <summary>
        /// Calcuates position from relative coordinates. 
        /// The origin of the relative coordinates is based on CTextBlock.
        /// </summary>
        /// <param name="x">The x coordinate of caret position on CTextBlock</param>
        /// <param name="y">The y coordinate of caret position on CTextBlock</param>
        /// <returns></returns>
        public TextPointer CalcuatePointerFrom(double x, double y);

        public TextPointer GetBegin();

        public TextPointer GetEnd();
    }

    public interface ISelectable
    {
        public void ClearSelection();
        public void Select(TextPointer start, TextPointer end);
    }
}
