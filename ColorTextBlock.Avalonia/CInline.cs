using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using ColorTextBlock.Avalonia.Geometries;
using System.Collections.Generic;
using System.ComponentModel;

namespace ColorTextBlock.Avalonia
{
    /// <summary>
    /// The base class for representing a text element.
    /// </summary>
    // テキスト要素を表現するための基底のクラス
    [TypeConverter(typeof(StringToRunConverter))]
    public abstract class CInline : StyledElement
    {
        /// <summary>
        /// The brush of background.
        /// </summary>
        /// <seealso cref="Background"/>
        public static readonly StyledProperty<IBrush?> BackgroundProperty =
            AvaloniaProperty.Register<CInline, IBrush?>(nameof(Background), inherits: true);

        /// <summary>
        /// The brush of the text element.
        /// </summary>
        /// <seealso cref="Foreground"/>
        public static readonly StyledProperty<IBrush?> ForegroundProperty =
            TextBlock.ForegroundProperty.AddOwner<CInline>();

        /// <summary>
        /// The font family of the text element
        /// </summary>
        /// <seealso cref="FontFamily"/>
        public static readonly StyledProperty<FontFamily> FontFamilyProperty =
            TextBlock.FontFamilyProperty.AddOwner<CInline>();

        /// <summary>
        /// The font weight of the text element
        /// </summary>
        /// <seealso cref="FontWeight"/>
        public static readonly StyledProperty<FontWeight> FontWeightProperty =
            TextBlock.FontWeightProperty.AddOwner<CInline>();

        /// <summary>
        /// The font stretch of the text element
        /// </summary>
        /// <seealso cref="FontStretch"/>
        public static readonly StyledProperty<FontStretch> FontStretchProperty =
            TextBlock.FontStretchProperty.AddOwner<CInline>();

        /// <summary>
        /// The font size of the text element
        /// </summary>
        /// <seealso cref="FontSize"/>
        public static readonly StyledProperty<double> FontSizeProperty =
            TextBlock.FontSizeProperty.AddOwner<CInline>();

        /// <summary>
        /// The font style of the text element
        /// </summary>
        /// <seealso cref="FontStyle"/>
        public static readonly StyledProperty<FontStyle> FontStyleProperty =
            TextBlock.FontStyleProperty.AddOwner<CInline>();

        /// <summary>
        /// Use to indicate the vertical position of text within line.
        /// For example, it is used to align text to the top or to the bottom.
        /// </summary>
        /// <seealso cref="TextVerticalAlignment"/>
        // テキストを上揃えで描画するか下揃えで描画するか指定します。
        public static readonly StyledProperty<TextVerticalAlignment> TextVerticalAlignmentProperty =
            CTextBlock.TextVerticalAlignmentProperty.AddOwner<CInline>();

        /// <summary>
        /// Indicates whether the text element is underlined.
        /// If this property value is true, the text element is underlined.
        /// </summary>
        /// <seealso cref="IsUnderline"/>
        public static readonly StyledProperty<bool> IsUnderlineProperty =
            AvaloniaProperty.Register<CInline, bool>(nameof(IsUnderline), inherits: true);

        /// <summary>
        /// Indicates whether the text element is strikethrough.
        /// If the value of this property is true, the text element is strikethrough.
        /// </summary>
        /// <seealso cref="IsStrikethrough"/>
        public static readonly StyledProperty<bool> IsStrikethroughProperty =
            AvaloniaProperty.Register<CInline, bool>(nameof(IsStrikethrough), inherits: true);

        /// <summary>
        /// The brush of background.
        /// </summary>
        public IBrush? Background
        {
            get { return GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        /// <summary>
        /// The brush of the text element.
        /// </summary>
        public IBrush? Foreground
        {
            get { return GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        /// <summary>
        /// The font family of the text element
        /// </summary>
        public FontFamily FontFamily
        {
            get { return GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        /// <summary>
        /// The font size of the text element
        /// </summary>
        public double FontSize
        {
            get { return GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        /// <summary>
        /// The font stretch of the text element
        /// </summary>
        public FontStyle FontStyle
        {
            get { return GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        /// <summary>
        /// The font weight of the text element
        /// </summary>
        public FontWeight FontWeight
        {
            get { return GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        /// <summary>
        /// The font stretch of the text element
        /// </summary>
            public FontStretch FontStretch
        {
            get { return GetValue(FontStretchProperty); }
            set { SetValue(FontStretchProperty, value); }
        }

        /// <summary>
        /// Typeface of the text element
        /// </summary>
        public Typeface Typeface
        {
            get;
            private set;
        }

        /// <summary>
        /// Indicates whether the text element is underlined.
        /// If this property value is true, the text element is underlined.
        /// </summary>
        public bool IsUnderline
        {
            get { return GetValue(IsUnderlineProperty); }
            set { SetValue(IsUnderlineProperty, value); }
        }

        /// <summary>
        /// Indicates whether the text element is strikethrough.
        /// If the value of this property is true, the text element is strikethrough.
        /// </summary>
        public bool IsStrikethrough
        {
            get { return GetValue(IsStrikethroughProperty); }
            set { SetValue(IsStrikethroughProperty, value); }
        }

        /// <summary>
        /// Use to indicate the vertical position of text within line.
        /// For example, it is used to align text to the top or to the bottom.
        /// </summary>
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


        /// <summary>
        /// Returns the string that this instance displays.
        /// </summary>
        /// <returns></returns>
        // この要素が表示する文字を返します。
        public abstract string AsString();
    }
}
