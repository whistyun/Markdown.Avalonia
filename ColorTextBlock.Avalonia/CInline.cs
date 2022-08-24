using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using ColorTextBlock.Avalonia.Geometries;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;

namespace ColorTextBlock.Avalonia
{
    [TypeConverter(typeof(StringToRunConverter))]
    public abstract class CInline : StyledElement
    {
        public static readonly StyledProperty<IBrush?> BackgroundProperty =
            AvaloniaProperty.Register<CInline, IBrush?>(nameof(Background), inherits: true);

        public static readonly StyledProperty<IBrush?> ForegroundProperty =
            TextBlock.ForegroundProperty.AddOwner<CInline>();

        public static readonly StyledProperty<FontFamily> FontFamilyProperty =
            TextBlock.FontFamilyProperty.AddOwner<CInline>();

        public static readonly StyledProperty<FontWeight> FontWeightProperty =
            TextBlock.FontWeightProperty.AddOwner<CInline>();

        public static readonly StyledProperty<double> FontSizeProperty =
            TextBlock.FontSizeProperty.AddOwner<CInline>();

        public static readonly StyledProperty<FontStyle> FontStyleProperty =
            TextBlock.FontStyleProperty.AddOwner<CInline>();

        public static readonly StyledProperty<TextVerticalAlignment> TextVerticalAlignmentProperty =
            CTextBlock.TextVerticalAlignmentProperty.AddOwner<CInline>();

        public static readonly StyledProperty<bool> IsUnderlineProperty =
            AvaloniaProperty.Register<CInline, bool>(nameof(IsUnderline), inherits: true);

        public static readonly StyledProperty<bool> IsStrikethroughProperty =
            AvaloniaProperty.Register<CInline, bool>(nameof(IsStrikethrough), inherits: true);

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
                FontWeightProperty.Changed,
                TextVerticalAlignmentProperty.Changed
            ).AddClassHandler<CInline>((x, _) => x.RequestMeasure());
        }

        public IBrush? Background
        {
            get { return GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public IBrush? Foreground
        {
            get { return GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        public FontFamily FontFamily
        {
            get { return GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        public double FontSize
        {
            get { return GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public FontStyle FontStyle
        {
            get { return GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        public FontWeight FontWeight
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

        public TextVerticalAlignment TextVerticalAlignment
        {
            get { return GetValue(TextVerticalAlignmentProperty); }
            set { SetValue(TextVerticalAlignmentProperty, value); }
        }

        protected void RequestMeasure()
        {
            if (Parent is CInline cline)
            {
                cline.RequestMeasure();
            }
            if (Parent is CTextBlock ctxt)
            {
                ctxt.OnMeasureSourceChanged();
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

        internal IEnumerable<CGeometry> Measure(double entireWidth, double remainWidth)
        {
            /*
             * This is Imitation of Layoutable.MeasureCore.
             * If parent style is changed, StyledElement.InvalidedStyles is called.
             * This method clear all applied styles, 
             * so we should reapply style after style change.
             */
            ApplyStyling();

            return MeasureOverride(entireWidth, remainWidth);
        }

        protected abstract IEnumerable<CGeometry> MeasureOverride(
            double entireWidth,
            double remainWidth);
    }
}
