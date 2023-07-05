using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using ColorTextBlock.Avalonia.Geometries;
using System.Collections.Generic;
using System.ComponentModel;

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

        public static readonly StyledProperty<FontStretch> FontStretchProperty =
            TextBlock.FontStretchProperty.AddOwner<CInline>();

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

        public FontStretch FontStretch
        {
            get { return GetValue(FontStretchProperty); }
            set { SetValue(FontStretchProperty, value); }
        }
        public Typeface Typeface
        {
            get;
            private set;
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

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            switch (change.Property.Name)
            {
                case nameof(Background):
                case nameof(Foreground):
                case nameof(IsUnderline):
                case nameof(IsStrikethrough):
                    RequestRender();
                    break;

                case nameof(FontFamily):
                case nameof(FontSize):
                case nameof(FontStyle):
                case nameof(FontWeight):
                case nameof(FontStretch):
                    Typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
                    goto case nameof(TextVerticalAlignment);

                case nameof(TextVerticalAlignment):
                    RequestMeasure();
                    break;
            }
        }

        protected void RequestMeasure()
        {
            if (Parent is CInline cline)
            {
                cline.RequestMeasure();
            }
            else if (Parent is CTextBlock ctxt)
            {
                ctxt.OnMeasureSourceChanged();
            }
            else if (Parent is Layoutable layout)
            {
                layout.InvalidateMeasure();
            }
        }

        protected void RequestRender()
        {
            try
            {
                if (Parent is CInline cline)
                {
                    cline.RequestRender();
                }
                else if (Parent is Layoutable layout)
                {
                    layout.InvalidateVisual();
                }
            }
            catch
            {
                // An error occured sometimes with FluentAvalonia.
            }
        }

        internal IEnumerable<CGeometry> Measure(double entireWidth, double remainWidth)
        {
            Typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);

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

        public abstract string AsString();
    }
}
