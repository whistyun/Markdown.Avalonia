using System;

namespace ColorTextBlock.Avalonia
{
    public abstract class TextPointer : IEquatable<TextPointer>
    {
        protected TextPointer()
        {
        }

        public abstract override bool Equals(object? obj);
        public abstract bool Equals(TextPointer? other);
        public abstract override int GetHashCode();
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
