using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColorTextBlock.Avalonia.Geometries
{
    public abstract class CGeometry
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; }
        public double Height { get; }
        public double BaseHeight { get; }
        public bool LineBreak { get; }
        public TextVerticalAlignment TextVerticalAlignment { get; }

        public event Action? RepaintRequested;

        public virtual Action? OnMouseEnter { get; set; }
        public virtual Action? OnMouseLeave { get; set; }
        public virtual Action? OnMousePressed { get; set; }
        public virtual Action? OnMouseReleased { get; set; }
        public virtual Action? OnClick { get; set; }

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
    }
}
