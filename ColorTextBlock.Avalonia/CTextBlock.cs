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

namespace ColorTextBlock.Avalonia
{
    public class CTextBlock : Control
    {
        private static readonly StyledProperty<double> LineHeightProperty =
            AvaloniaProperty.Register<CTextBlock, double>("LineHeight");

        public static readonly StyledProperty<IBrush> BackgroundProperty =
            Border.BackgroundProperty.AddOwner<CTextBlock>();

        public static readonly AttachedProperty<IBrush> ForegroundProperty =
            TextBlock.ForegroundProperty.AddOwner<CTextBlock>();

        public static readonly AttachedProperty<FontFamily> FontFamilyProperty =
            TextBlock.FontFamilyProperty.AddOwner<CTextBlock>();

        public static readonly AttachedProperty<FontWeight> FontWeightProperty =
            TextBlock.FontWeightProperty.AddOwner<CTextBlock>();

        public static readonly AttachedProperty<double> FontSizeProperty =
            TextBlock.FontSizeProperty.AddOwner<CTextBlock>();

        public static readonly AttachedProperty<FontStyle> FontStyleProperty =
            TextBlock.FontStyleProperty.AddOwner<CTextBlock>();

        public static readonly StyledProperty<TextVerticalAlignment> TextVerticalAlignmentProperty =
            AvaloniaProperty.Register<CTextBlock, TextVerticalAlignment>(
                nameof(TextVerticalAlignment),
                defaultValue: TextVerticalAlignment.Descent,
                inherits: true);

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
                BoundsProperty.Changed,
                TextVerticalAlignmentProperty.Changed
            ).AddClassHandler<CTextBlock>((x, _) => x.OnMeasureSourceChanged(true));

            Observable.Merge<AvaloniaPropertyChangedEventArgs>(
                LineHeightProperty.Changed
            ).AddClassHandler<CTextBlock>((x, _) => x.OnMeasureSourceChanged(false));
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

        public TextVerticalAlignment TextVerticalAlignment
        {
            get { return GetValue(TextVerticalAlignmentProperty); }
            set { SetValue(TextVerticalAlignmentProperty, value); }
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
                    olds.CollectionChanged -= ContentCollectionChangedd;
                    foreach (var oldrun in olds)
                        LogicalChildren.Remove(oldrun);

                    LogicalChildren.AddRange(_content);
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
        CGeometry pressed;

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

                Point point = e.GetPosition(this);

                bool isEntered(CGeometry metry)
                {
                    var relX = point.X - metry.Left;
                    var relY = point.Y - metry.Top;

                    return 0 <= relX && relX <= metry.Width
                        && 0 <= relY && relY <= metry.Height;
                }

                foreach (CGeometry metry in metries)
                {
                    if (isEntered(metry))
                    {
                        metry.OnMousePressed?.Invoke();
                        pressed = metry;
                        break;
                    }
                }
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            if (isPressed && e.InitialPressMouseButton == MouseButton.Left)
            {
                isPressed = false;
                e.Handled = true;

                if (pressed != null)
                {
                    pressed.OnMouseReleased?.Invoke();
                    pressed = null;
                }


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

        public void ObserveLineHeightOf(CTextBlock target)
        {
            this.Bind(LineHeightProperty, target.GetBindingObservable(LineHeightProperty));
        }

        private void ContentCollectionChangedd(object sender, NotifyCollectionChangedEventArgs e)
        {
            void Attach(IEnumerable<CInline> newItems)
            {
                foreach (CInline item in newItems)
                    LogicalChildren.Add(item);
            }

            void Detach(IEnumerable<CInline> removeItems)
            {
                foreach (CInline item in removeItems)
                    LogicalChildren.Remove(item);
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

        internal void OnMeasureSourceChanged(bool clearTemporary)
        {
            if (clearTemporary)
            {
                ClearValue(LineHeightProperty);
            }

            InvalidateMeasure();
        }

        private void RepaintRequested()
        {
            InvalidateVisual();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            metries.Clear();

            double entireWidth = availableSize.Width;
            if (Double.IsInfinity(availableSize.Width) && Bounds.Width != 0)
                entireWidth = Bounds.Width;


            double width = 0;
            double height = 0;

            // measure & split by linebreak
            var reqHeight = GetValue(LineHeightProperty);
            var lines = new List<LineInfo>();
            {
                LineInfo now = null;

                double remainWidth = entireWidth;

                foreach (CInline inline in Content)
                {
                    IEnumerable<CGeometry> inlineGeometry =
                        inline.Measure(
                            (TextWrapping == TextWrapping.NoWrap) ? Double.PositiveInfinity : entireWidth,
                            (TextWrapping == TextWrapping.NoWrap) ? Double.PositiveInfinity : remainWidth);

                    foreach (CGeometry metry in inlineGeometry)
                    {
                        if (now is null)
                        {
                            lines.Add(now = new LineInfo());
                            if (lines.Count == 1)
                                now.RequestLineHeight = reqHeight;
                        }

                        if (now.Add(metry))
                        {
                            width = Math.Max(width, now.Width);
                            height += now.Height;

                            now = null;
                            remainWidth = entireWidth;
                        }
                        else remainWidth -= metry.Width;
                    }
                }

                if (now != null)
                {
                    width = Math.Max(width, now.Width);
                    height += now.Height;
                }
            }

            if (lines.Count > 0)
            {
                SetValue(LineHeightProperty, lines[0].LineHeight);
            }

            // set position
            {
                var topOffset = 0d;
                var leftOffset = 0d;

                foreach (LineInfo lineInf in lines)
                {
                    switch (TextAlignment)
                    {
                        case TextAlignment.Left:
                            leftOffset = 0d;
                            break;
                        case TextAlignment.Center:
                            leftOffset = (entireWidth - lineInf.Width) / 2;
                            break;
                        case TextAlignment.Right:
                            leftOffset = entireWidth - lineInf.Width;
                            break;
                    }

                    foreach (CGeometry metry in lineInf.Metries)
                    {
                        metry.Left = leftOffset;
                        switch (metry.TextVerticalAlignment)
                        {
                            case TextVerticalAlignment.Top:
                                metry.Top = topOffset;
                                break;
                            case TextVerticalAlignment.Center:
                                metry.Top = topOffset + (lineInf.Height - metry.Height) / 2;
                                break;
                            case TextVerticalAlignment.Bottom:
                                metry.Top = topOffset + lineInf.Height - metry.Height;
                                break;
                            case TextVerticalAlignment.Descent:
                                metry.Top = topOffset + lineInf.LineHeight - metry.LineHeight;
                                break;
                        }

                        leftOffset += metry.Width;

                        metries.Add(metry);
                    }

                    topOffset += lineInf.Height;
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

    class LineInfo
    {
        public List<CGeometry> Metries = new List<CGeometry>();

        public double RequestLineHeight;
        private double LineHeight1;
        private double LineHeight2;

        private double _height;
        private double _dheightTop;
        private double _dheightBtm;

        public double Width { private set; get; }
        public double Height => Math.Max(_height, _dheightTop + _dheightBtm);
        public double LineHeight => Math.Max(RequestLineHeight, LineHeight1 != 0 ? LineHeight1 : LineHeight2);

        public bool Add(CGeometry metry)
        {
            Metries.Add(metry);

            Width += metry.Width;

            switch (metry.TextVerticalAlignment)
            {
                case TextVerticalAlignment.Descent:
                    Max(ref LineHeight1, metry.LineHeight);
                    Max(ref _dheightTop, metry.LineHeight);
                    Max(ref _dheightBtm, metry.Height - metry.LineHeight);
                    break;

                case TextVerticalAlignment.Top:
                    Max(ref LineHeight1, metry.LineHeight);
                    Max(ref _height, metry.Height);
                    break;

                case TextVerticalAlignment.Center:
                    Max(ref LineHeight1, metry.Height / 2);
                    Max(ref _height, metry.Height);
                    break;

                case TextVerticalAlignment.Bottom:
                    Max(ref LineHeight2, metry.LineHeight);
                    Max(ref _height, metry.Height);
                    break;

                default:
                    Throw("sorry library manager forget to modify.");
                    break;
            }

            return metry.LineBreak;
        }

        private static void Max(ref double v1, double v2) => v1 = Math.Max(v1, v2);
        private static void Throw(string msg) => throw new InvalidOperationException(msg);
    }
}
