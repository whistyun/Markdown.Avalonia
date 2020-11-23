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
    public class CTextBlock : Control
    {
        public static readonly StyledProperty<IBrush> BackgroundProperty =
         Border.BackgroundProperty.AddOwner<CTextBlock>();

        public static readonly StyledProperty<IBrush> ForegroundProperty =
            AvaloniaProperty.Register<CTextBlock, IBrush>(
                nameof(Foreground), defaultValue: Brushes.Black);

        public static readonly StyledProperty<FontFamily> FontFamilyProperty =
            AvaloniaProperty.Register<CTextBlock, FontFamily>(
                nameof(FontFamily), defaultValue: FontFamily.Default);

        public static readonly StyledProperty<double> FontSizeProperty =
            AvaloniaProperty.Register<CTextBlock, double>(
                nameof(FontSize), defaultValue: 12);

        public static readonly StyledProperty<FontStyle> FontStyleProperty =
            AvaloniaProperty.Register<CTextBlock, FontStyle>(
                nameof(FontStyle), defaultValue: FontStyle.Normal);

        public static readonly StyledProperty<FontWeight> FontWeightProperty =
            AvaloniaProperty.Register<CTextBlock, FontWeight>(
                nameof(FontWeight), defaultValue: FontWeight.Normal);

        public static readonly StyledProperty<TextWrapping> TextWrappingProperty =
            AvaloniaProperty.Register<CTextBlock, TextWrapping>(nameof(TextWrapping));

        public static readonly DirectProperty<CTextBlock, IEnumerable<CInline>> ContentProperty =
            AvaloniaProperty.RegisterDirect<CTextBlock, IEnumerable<CInline>>(
                nameof(Content),
                    o => o.Content,
                    (o, v) => o.Content = v);

        public static readonly StyledProperty<TextAlignment> TextAlignmentProperty =
            AvaloniaProperty.Register<CTextBlock, TextAlignment>(
                nameof(TextAlignment), defaultValue: TextAlignment.Left);


        static CTextBlock()
        {
            ClipToBoundsProperty.OverrideDefaultValue<CTextBlock>(true);

            AffectsRender<CTextBlock>(
                BackgroundProperty,
                ForegroundProperty,
                FontWeightProperty,
                FontSizeProperty,
                FontStyleProperty);

            Observable.Merge(
                ContentProperty.Changed,
                FontSizeProperty.Changed,
                FontStyleProperty.Changed,
                FontWeightProperty.Changed,
                TextWrappingProperty.Changed
            ).AddClassHandler<CTextBlock>((x, _) => x.OnMeasureSourceChanged());
        }

        private IEnumerable<CInline> _content = new List<CInline>();
        private List<CGeometry> metries;

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

        public TextWrapping TextWrapping
        {
            get { return GetValue(TextWrappingProperty); }
            set { SetValue(TextWrappingProperty, value); }
        }

        public TextAlignment TextAlignment
        {
            get { return GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }

        [Content]
        public IEnumerable<CInline> Content
        {

            get => _content;
            set
            {
                var olds = _content;

                if (SetAndRaise(ContentProperty, ref _content, value))
                {
                    // remove change listener
                    foreach (var oldrun in olds)
                        RegisterOrUnregister(oldrun, true);


                    // set change listener
                    foreach (var newrun in _content)
                        RegisterOrUnregister(newrun, false);
                }
            }
        }

        private void RegisterOrUnregister(CInline inline, bool unregister)
        {
            if (unregister)
                inline.PropertyChanged -= OnTextStructureChanged;
            else
                inline.PropertyChanged += OnTextStructureChanged;

            if (inline is CSpan span)
                foreach (CInline spanCnt in span.Content)
                    RegisterOrUnregister(spanCnt, unregister);
        }



        private void OnTextStructureChanged(object sender, AvaloniaPropertyChangedEventArgs args)
        {
            var prop = args.Property;

            if (prop == CInline.FontFamilyProperty
                || prop == CInline.FontSizeProperty
                || prop == CInline.FontStyleProperty
                || prop == CInline.FontWeightProperty
                || prop == CRun.TextProperty
                || prop == CSpan.ContentProperty)
            {
                OnMeasureSourceChanged();
            }
        }

        private void OnMeasureSourceChanged()
        {
            InvalidateMeasure();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            metries = new List<CGeometry>();

            double width = 0;
            double height = 0;

            double prevElmntWid = 0;
            double prevElmntHei = 0;

            int lineStartIndex = 0;

            double entireWidth = availableSize.Width;
            double remainWidth = entireWidth;
            foreach (CInline inline in Content)
            {
                IEnumerable<CGeometry> inlineGeometry =
                    inline.Measure(
                        FontFamily,
                        FontSize,
                        FontStyle,
                        FontWeight,
                        Foreground,
                        Background,
                        false,
                        false,
                        entireWidth,
                        remainWidth);

                foreach (CGeometry metry in inlineGeometry)
                {
                    metries.Add(metry);

                    metry.Left = prevElmntWid;

                    prevElmntWid += metry.Width;
                    prevElmntHei = Math.Max(prevElmntHei, metry.Height);

                    if (metry.LineBreak)
                    {
                        foreach (CGeometry metryAtLine in metries.Skip(lineStartIndex))
                            metryAtLine.Top = height + prevElmntHei - metryAtLine.Height;

                        width = Math.Max(width, prevElmntWid);
                        height += prevElmntHei;

                        prevElmntWid = 0;
                        prevElmntHei = 0;
                        lineStartIndex = metries.Count;
                    }
                }
            }

            return new Size(width, height);
        }

        public override void Render(DrawingContext context)
        {
            foreach (var metry in metries)
            {
                metry.Render(context);
            }
        }
    }
}
