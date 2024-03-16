using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using System.Linq;

namespace ColorTextBlock.Avalonia.Geometries
{
    internal abstract class TextGeometry : CGeometry
    {
        private IBrush? _TemporaryForeground;
        public IBrush? TemporaryForeground
        {
            get => _TemporaryForeground;
            set => _TemporaryForeground = value;
        }

        private IBrush? _TemporaryBackground;
        public IBrush? TemporaryBackground
        {
            get => _TemporaryBackground;
            set => _TemporaryBackground = value;
        }

        public IBrush? Foreground
        {
            get => Owner?.Foreground;
        }
        public IBrush? Background
        {
            get => Owner?.Background;
        }
        public bool IsUnderline
        {
            get => Owner is null ? false : Owner.IsUnderline;
        }
        public bool IsStrikethrough
        {
            get => Owner is null ? false : Owner.IsStrikethrough;
        }

        internal TextGeometry(
            CInline owner,
            double width, double height, double lineHeight,
            TextVerticalAlignment alignment,
            bool linebreak) :
            base(owner, width, height, lineHeight, alignment, linebreak)
        {
        }
    }
}
