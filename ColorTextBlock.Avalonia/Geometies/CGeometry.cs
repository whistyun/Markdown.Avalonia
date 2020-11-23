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
        public bool LineBreak { get; }

        public Action OnMouseEnter { get; set; }
        public Action OnMouseExit { get; set; }
        public Action OnClick { get; set; }

        public CGeometry(double width, double height, bool linebreak)
        {
            this.Width = width;
            this.Height = height;
            this.LineBreak = linebreak;
        }

        public abstract void Render(DrawingContext ctx);
    }
}
