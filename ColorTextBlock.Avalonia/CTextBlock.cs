using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Metadata;
using ColorTextBlock.Avalonia.Geometries;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace ColorTextBlock.Avalonia
{
    public class CTextBlock : Control
    {
        public static readonly StyledProperty<IBrush> BackgroundProperty =
            Border.BackgroundProperty.AddOwner<CTextBlock>();

        public static readonly AttachedProperty<IBrush> ForegroundProperty =
            TextBlock.ForegroundProperty.AddOwner<CTextBlock>();

        public static readonly AttachedProperty<FontWeight> FontWeightProperty =
            TextBlock.FontWeightProperty.AddOwner<CTextBlock>();

        public static readonly AttachedProperty<double> FontSizeProperty =
            TextBlock.FontSizeProperty.AddOwner<CTextBlock>();

        public static readonly AttachedProperty<FontStyle> FontStyleProperty =
            TextBlock.FontStyleProperty.AddOwner<CTextBlock>();


        public static readonly StyledProperty<TextWrapping> TextWrappingProperty =
            AvaloniaProperty.Register<CTextBlock, TextWrapping>(nameof(TextWrapping), defaultValue: TextWrapping.Wrap);

        public static readonly DirectProperty<CTextBlock, AvaloniaList<CInline>> ContentProperty =
            AvaloniaProperty.RegisterDirect<CTextBlock, AvaloniaList<CInline>>(
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
                TextBlock.ForegroundProperty,
                TextBlock.FontWeightProperty,
                TextBlock.FontSizeProperty,
                TextBlock.FontStyleProperty);

            Observable.Merge<AvaloniaPropertyChangedEventArgs>(
                ContentProperty.Changed,
                TextBlock.FontSizeProperty.Changed,
                TextBlock.FontStyleProperty.Changed,
                TextBlock.FontWeightProperty.Changed,
                TextWrappingProperty.Changed,
                BoundsProperty.Changed
            ).AddClassHandler<CTextBlock>((x, _) => x.OnMeasureSourceChanged());
        }

        private AvaloniaList<CInline> _content;
        private List<CGeometry> metries;

        public IBrush Background
        {
            get { return GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public IBrush Foreground
        {
            get { return TextBlock.GetForeground(this); }
            set { TextBlock.SetForeground(this, value); }
        }

        public FontFamily FontFamily
        {
            get { return TextBlock.GetFontFamily(this); }
            set { TextBlock.SetFontFamily(this, value); }
        }

        public double FontSize
        {
            get { return TextBlock.GetFontSize(this); }
            set { TextBlock.SetFontSize(this, value); }
        }

        public FontStyle FontStyle
        {
            get { return TextBlock.GetFontStyle(this); }
            set { TextBlock.SetFontStyle(this, value); }
        }

        public FontWeight FontWeight
        {
            get { return TextBlock.GetFontWeight(this); }
            set { TextBlock.SetFontWeight(this, value); }
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
        public AvaloniaList<CInline> Content
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


                    olds.CollectionChanged -= ContentCollectionChangedd;
                    _content.CollectionChanged += ContentCollectionChangedd;
                }
            }
        }

        public CTextBlock()
        {
            _content = new AvaloniaList<CInline>();
            _content.CollectionChanged += ContentCollectionChangedd;

            metries = new List<CGeometry>();
        }

        public CTextBlock(string text) : this()
        {
            _content.Add(new CRun() { Text = text });
        }

        public CTextBlock(IEnumerable<CInline> inlines) : this()
        {
            _content.AddRange(inlines);
        }


        #region pointer event

        bool isPressed;
        CGeometry entered;

        protected override void OnPointerLeave(PointerEventArgs e)
        {
            base.OnPointerLeave(e);

            if (entered != null)
            {
                entered.OnMouseLeave?.Invoke();
                entered = null;
            }
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);

            Point point = e.GetPosition(this);

            bool isEntered(CGeometry metry)
            {
                var relX = point.X - metry.Left;
                var relY = point.Y - metry.Top;

                return 0 <= relX && relX <= metry.Width
                    && 0 <= relY && relY <= metry.Height;
            }

            if (entered != null)
            {
                var relX = point.X - entered.Left;
                var relY = point.Y - entered.Top;

                if (!isEntered(entered))
                {
                    entered.OnMouseLeave?.Invoke();
                    entered = null;
                }
                else return;
            }

            foreach (CGeometry metry in metries)
            {
                if (isEntered(metry))
                {
                    metry.OnMouseEnter?.Invoke();
                    entered = metry;
                    break;
                }
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                isPressed = true;
                e.Handled = true;
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            if (isPressed && e.InitialPressMouseButton == MouseButton.Left)
            {
                isPressed = false;
                e.Handled = true;

                Point point = e.GetPosition(this);

                foreach (CGeometry metry in metries)
                {
                    var relX = point.X - metry.Left;
                    var relY = point.Y - metry.Top;

                    if (0 <= relX && relX <= metry.Width
                        && 0 <= relY && relY <= metry.Height)
                    {
                        metry.OnClick?.Invoke();
                        break;
                    }
                }
            }
        }

        #endregion

        private void RegisterOrUnregister(CInline inline, bool unregister)
        {
            if (unregister)
            {
                inline.PropertyChanged -= OnTextStructureChanged;
                LogicalChildren.Remove(inline);
            }

            else
            {
                inline.PropertyChanged += OnTextStructureChanged;
                LogicalChildren.Add(inline);
            }

            if (inline is CSpan span)
                foreach (CInline spanCnt in span.Content)
                    RegisterOrUnregister(spanCnt, unregister);
        }

        private void ContentCollectionChangedd(object sender, NotifyCollectionChangedEventArgs e)
        {
            void Attach(IEnumerable<CInline> newItems)
            {
                foreach (CInline item in newItems) RegisterOrUnregister(item, false);
            }

            void Detach(IEnumerable<CInline> removeItems)
            {
                foreach (CInline item in removeItems) RegisterOrUnregister(item, true);
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                case NotifyCollectionChangedAction.Remove:
                    Detach(e.OldItems.Cast<CInline>());
                    break;

                case NotifyCollectionChangedAction.Replace:
                    Detach(e.OldItems.Cast<CInline>());
                    Attach(e.NewItems.Cast<CInline>());
                    break;

                case NotifyCollectionChangedAction.Add:
                    Attach(e.NewItems.Cast<CInline>());
                    break;
            }
        }


        private void OnTextStructureChanged(object sender, AvaloniaPropertyChangedEventArgs args)
        {
            var prop = args.Property;

            if (prop == CInline.FontFamilyProperty
                || prop == CInline.FontSizeProperty
                || prop == CInline.FontStyleProperty
                || prop == CInline.FontWeightProperty
                || prop == CRun.TextProperty
                || prop == CSpan.ContentProperty
                || prop == CImage.LayoutHeightProperty
                || prop == CImage.LayoutWidthProperty)
            {
                OnMeasureSourceChanged();
            }
        }

        private void OnMeasureSourceChanged()
        {
            InvalidateMeasure();
        }

        private void RepaintRequested()
        {
            InvalidateVisual();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (CGeometry metry in metries) metry.RepaintRequested -= RepaintRequested;

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
                            Foreground, null, false, false,
                            (TextWrapping == TextWrapping.NoWrap) ? Double.PositiveInfinity : entireWidth,
                            (TextWrapping == TextWrapping.NoWrap) ? Double.PositiveInfinity : remainWidth);

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

            foreach (CGeometry metry in metries) metry.RepaintRequested += RepaintRequested;

            return new Size(width, height);
        }

        public override void Render(DrawingContext context)
        {
            if (Background != null)
            {
                context.FillRectangle(Background, new Rect(0, 0, Bounds.Width, Bounds.Height));
            }

            foreach (var metry in metries)
            {
                metry.Render(context);
            }
        }
    }
}
