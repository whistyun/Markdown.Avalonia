using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using ColorTextBlock.Avalonia.Geometries;
using System.Collections.Generic;

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
