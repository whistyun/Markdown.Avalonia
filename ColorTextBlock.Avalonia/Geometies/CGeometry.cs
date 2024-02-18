using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Diagnostics.CodeAnalysis;

namespace ColorTextBlock.Avalonia.Geometries
{
    public abstract class CGeometry : ITextPointerHandleable
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; }
        public double Height { get; }
        public double BaseHeight { get; }
        public bool LineBreak { get; }
        public TextVerticalAlignment TextVerticalAlignment { get; }

        public event Action? RepaintRequested;

        public virtual Action<Control>? OnMouseEnter { get; set; }
        public virtual Action<Control>? OnMouseLeave { get; set; }
        public virtual Action<Control>? OnMousePressed { get; set; }
        public virtual Action<Control>? OnMouseReleased { get; set; }
        public virtual Action<Control>? OnClick { get; set; }

        private int? _caretLength;

        public CGeometry(
            double width, double height, double baseHeight,
            TextVerticalAlignment textVerticalAlignment,
            bool linebreak)
        {
            this.Width = width;
            this.Height = height;
            this.BaseHeight = baseHeight;
            this.TextVerticalAlignment = textVerticalAlignment;
            this.LineBreak = linebreak;
        }

        public abstract void Render(DrawingContext ctx);

        internal void RequestRepaint() => RepaintRequested?.Invoke();


        public abstract bool TryMoveNext(
            TextPointer current,
#if NETCOREAPP3_0_OR_GREATER
            [MaybeNullWhen(false)]
            out TextPointer? next
#else
            out TextPointer next
#endif
            );
        public abstract bool TryMovePrev(
            TextPointer current,
#if NETCOREAPP3_0_OR_GREATER
            [MaybeNullWhen(false)]
            out TextPointer? prev
#else
            out TextPointer prev
#endif
            );
        public abstract TextPointer CalcuatePointerFrom(double x, double y);
        public abstract TextPointer GetBegin();
        public abstract TextPointer GetEnd();

        public virtual int CaretLength
        {
            get
            {
                if (!_caretLength.HasValue)
                    _caretLength = GetEnd().Index - GetBegin().Index;

                return _caretLength.Value;
            }
        }
    }
}
