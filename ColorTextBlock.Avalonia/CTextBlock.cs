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
                TextWrappingProperty.Changed,
                BoundsProperty.Changed
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

        public CTextBlock() { }
        public CTextBlock(string text)
        {
            Content = new[] { new CRun() { Text = text } };
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

            double entireWidth = availableSize.Width;
            if (Double.IsInfinity(availableSize.Width) && Bounds.Width != 0)
                entireWidth = Bounds.Width;

            // measure & split by linebreak
            var lineMetries = new List<Tuple<List<CGeometry>, Size>>();
            {

                double remainWidth = entireWidth;

                var atlineWid = 0d;
                var atlineHei = 0d;
                var atline = new List<CGeometry>();
                foreach (CInline inline in Content)
                {
                    IEnumerable<CGeometry> inlineGeometry =
                        inline.Measure(
                            FontFamily, FontSize, FontStyle, FontWeight,
                            Foreground, null, false, false, entireWidth, remainWidth);

                    foreach (CGeometry metry in inlineGeometry)
                    {
                        atline.Add(metry);
                        atlineWid += metry.Width;
                        atlineHei = Math.Max(metry.Height, atlineHei);

                        remainWidth -= metry.Width;

                        if (metry.LineBreak)
                        {
                            metries.AddRange(atline);

                            lineMetries.Add(Tuple.Create(atline, new Size(atlineWid, atlineHei)));

                            atline = new List<CGeometry>();
                            atlineWid = 0d;
                            atlineHei = 0d;

                            remainWidth = entireWidth;
                        }
                    }
                }

                if (atline.Count > 0)
                {
                    metries.AddRange(atline);

                    lineMetries.Add(Tuple.Create(atline, new Size(atlineWid, atlineHei)));

                    atline = new List<CGeometry>();
                    atlineWid = 0d;
                    atlineHei = 0d;

                    remainWidth = entireWidth;
                }
            }

            double width = lineMetries.Count == 0 ? 0d : lineMetries.Max(tpl => tpl.Item2.Width);
            double height = lineMetries.Count == 0 ? 0d : lineMetries.Sum(tpl => tpl.Item2.Height);

            // set position
            {
                var topOffset = 0d;
                var leftOffset = 0d;

                foreach (Tuple<List<CGeometry>, Size> lineMetry in lineMetries)
                {
                    switch (TextAlignment)
                    {
                        case TextAlignment.Left:
                            leftOffset = 0d;
                            break;
                        case TextAlignment.Center:
                            leftOffset = (entireWidth - lineMetry.Item2.Width) / 2;
                            break;
                        case TextAlignment.Right:
                            leftOffset = entireWidth - lineMetry.Item2.Width;
                            break;
                    }

                    topOffset += lineMetry.Item2.Height;

                    foreach (CGeometry metry in lineMetry.Item1)
                    {
                        metry.Left = leftOffset;
                        metry.Top = topOffset - metry.Height;

                        leftOffset += metry.Width;
                    }
                }
            }

            return new Size(width, height);
        }

        public override void Render(DrawingContext context)
        {
            if (Background != null)
            {
                context.FillRectangle(Background, Bounds);
            }

            foreach (var metry in metries)
            {
                metry.Render(context);
            }
        }
    }
}
