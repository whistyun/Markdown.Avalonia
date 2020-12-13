using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Metadata;
using ColorTextBlock.Avalonia.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace ColorTextBlock.Avalonia
{
    public class CSpan : CInline
    {
        public static readonly StyledProperty<Thickness> BorderThicknessProperty =
            AvaloniaProperty.Register<CSpan, Thickness>(nameof(BorderThickness));

        public static readonly StyledProperty<IBrush> BorderBrushProperty =
            AvaloniaProperty.Register<CSpan, IBrush>(nameof(BorderBrush));

        public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
            AvaloniaProperty.Register<CSpan, CornerRadius>(nameof(CornerRadius));

        public static readonly StyledProperty<Thickness> PaddingProperty =
            AvaloniaProperty.Register<CSpan, Thickness>(nameof(Padding));

        public static readonly StyledProperty<IEnumerable<CInline>> ContentProperty =
            AvaloniaProperty.Register<CSpan, IEnumerable<CInline>>(nameof(Content));

        static CSpan()
        {
            Observable.Merge<AvaloniaPropertyChangedEventArgs>(
                BorderThicknessProperty.Changed,
                BorderBrushProperty.Changed,
                CornerRadiusProperty.Changed,
                PaddingProperty.Changed
            ).AddClassHandler<CSpan>((x, _) => x.OnBorderPropertyChanged());

            ContentProperty.Changed.AddClassHandler<CSpan>(
                (x, e) =>
                {
                    if (e.OldValue is IEnumerable<CInline> oldlines)
                    {
                        foreach (var child in oldlines)
                            x.LogicalChildren.Remove(child);
                    }
                    if (e.NewValue is IEnumerable<CInline> newlines)
                    {
                        foreach (var child in newlines)
                            x.LogicalChildren.Remove(child);
                    }
                });
        }

        private Border _border;

        public Thickness BorderThickness
        {
            get => GetValue(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }
        public IBrush BorderBrush
        {
            get => GetValue(BorderBrushProperty);
            set => SetValue(BorderBrushProperty, value);
        }
        public CornerRadius CornerRadius
        {
            get => GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
        public Thickness Padding
        {
            get => GetValue(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }

        [Content]
        public IEnumerable<CInline> Content
        {
            get { return GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public CSpan(IEnumerable<CInline> inlines)
        {
            Content = inlines.ToArray();
        }

        private void OnBorderPropertyChanged()
        {
            bool borderEnabled =
                BorderThickness != default(Thickness) ||
                Padding != default(Thickness) ||
                CornerRadius != default(CornerRadius);

            bool borderEnabledChanged = (_border != null) == borderEnabled;

            if (borderEnabled)
            {
                _border = _border ?? new Border();
                _border.BorderThickness = BorderThickness;
                _border.BorderBrush = BorderBrush;
                _border.CornerRadius = CornerRadius;
                _border.Padding = Padding;
            }
            else
            {
                _border = null;
            }
        }

        protected internal override IEnumerable<CGeometry> Measure(
            FontFamily parentFontFamily,
            double parentFontSize,
            FontStyle parentFontStyle,
            FontWeight parentFontWeight,
            IBrush parentForeground,
            IBrush parentBackground,
            bool parentUnderline,
            bool parentStrikethough,
            double entireWidth,
            double remainWidth)
        {
            if (_border != null)
            {
                _border.Measure(Size.Infinity);
            }


            var metries = new List<CGeometry>();

            foreach (CInline inline in Content)
            {
                metries.AddRange(inline.Measure(
                        FontFamily ?? parentFontFamily,
                        FontSize.HasValue ? FontSize.Value : parentFontSize,
                        FontStyle.HasValue ? FontStyle.Value : parentFontStyle,
                        FontWeight.HasValue ? FontWeight.Value : parentFontWeight,
                        Foreground ?? parentForeground,
                        Background ?? parentBackground,
                        IsUnderline || parentUnderline,
                        IsStrikethrough || parentStrikethough,
                        entireWidth,
                        remainWidth));

                CGeometry last = metries[metries.Count - 1];

                remainWidth = last.LineBreak ?
                    entireWidth : entireWidth - last.Width;
            }

            return metries;
        }
    }


}
