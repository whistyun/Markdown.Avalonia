using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using ColorTextBlock.Avalonia.Geometries;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace ColorTextBlock.Avalonia
{
    public abstract class CInline : StyledElement
    {
        public static readonly StyledProperty<IBrush> BackgroundProperty =
            AvaloniaProperty.Register<CInline, IBrush>(nameof(Foreground));

        public static readonly StyledProperty<IBrush> ForegroundProperty =
            AvaloniaProperty.Register<CInline, IBrush>(nameof(Foreground));

        public static readonly StyledProperty<FontFamily> FontFamilyProperty =
            AvaloniaProperty.Register<CInline, FontFamily>(nameof(FontFamily));

        public static readonly StyledProperty<double?> FontSizeProperty =
            AvaloniaProperty.Register<CInline, double?>(nameof(FontSize));

        public static readonly StyledProperty<FontStyle?> FontStyleProperty =
            AvaloniaProperty.Register<CInline, FontStyle?>(nameof(FontStyle));

        public static readonly StyledProperty<FontWeight?> FontWeightProperty =
            AvaloniaProperty.Register<CInline, FontWeight?>(nameof(FontWeight));

        public static readonly StyledProperty<bool> IsUnderlineProperty =
            AvaloniaProperty.Register<CInline, bool>(nameof(IsUnderline));

        public static readonly StyledProperty<bool> IsStrikethroughProperty =
            AvaloniaProperty.Register<CInline, bool>(nameof(IsStrikethrough));

        static CInline()
        {
            Observable.Merge<AvaloniaPropertyChangedEventArgs>(
                BackgroundProperty.Changed,
                ForegroundProperty.Changed,
                FontFamilyProperty.Changed,
                FontSizeProperty.Changed,
                FontStyleProperty.Changed,
                FontWeightProperty.Changed,
                IsUnderlineProperty.Changed,
                IsStrikethroughProperty.Changed
            ).AddClassHandler<CInline>((x, _) => x.RequestRender());

            Observable.Merge<AvaloniaPropertyChangedEventArgs>(
                FontFamilyProperty.Changed,
                FontSizeProperty.Changed,
                FontStyleProperty.Changed,
                FontWeightProperty.Changed
            ).AddClassHandler<CInline>((x, _) => x.RequestMeasure());
        }

        public IBrush Background
        {
            get { return GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public IBrush Foreground
        {
            get { return GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        public FontFamily FontFamily
        {
            get { return GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        public double? FontSize
        {
            get { return GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public FontStyle? FontStyle
        {
            get { return GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        public FontWeight? FontWeight
        {
            get { return GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        public bool IsUnderline
        {
            get { return GetValue(IsUnderlineProperty); }
            set { SetValue(IsUnderlineProperty, value); }
        }

        public bool IsStrikethrough
        {
            get { return GetValue(IsStrikethroughProperty); }
            set { SetValue(IsStrikethroughProperty, value); }
        }

        protected void RequestMeasure()
        {
            if (Parent is CInline cline)
            {
                cline.RequestMeasure();
            }
            if (Parent is Layoutable layout)
            {
                layout.InvalidateMeasure();
            }
        }

        protected void RequestRender()
        {
            if (Parent is CInline cline)
            {
                cline.RequestRender();
            }
            if (Parent is Layoutable layout)
            {
                layout.InvalidateVisual();
            }
        }

        protected internal abstract IEnumerable<CGeometry> Measure(
            FontFamily parentFontFamily,
            double parentFontSize,
            FontStyle parentFontStyle,
            FontWeight parentFontWeight,
            IBrush parentForeground,
            IBrush parentBackground,
            bool parentUnderline,
            bool parentStrikethough,
            double entireWidth,
            double remainWidth);
    }
}
