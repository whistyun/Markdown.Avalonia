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
                CornerRadiusProperty.Changed,
                PaddingProperty.Changed
            ).AddClassHandler<CSpan>((x, _) => x.OnBorderPropertyChanged(true));

            Observable.Merge<AvaloniaPropertyChangedEventArgs>(
                BorderBrushProperty.Changed
            ).AddClassHandler<CSpan>((x, _) => x.OnBorderPropertyChanged(false));

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
                            x.LogicalChildren.Add(child);
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

        internal bool HasBorderProperty
        {
            get => _border != null;
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

        private void OnBorderPropertyChanged(bool requestMeasure)
        {
            bool borderEnabled =
                BorderThickness != default(Thickness) ||
                Padding != default(Thickness) ||
                CornerRadius != default(CornerRadius);

            if (borderEnabled)
            {
                if (_border is null)
                {
                    _border = new Border();
                    LogicalChildren.Add(_border);
                }

                _border.BorderThickness = BorderThickness;
                _border.BorderBrush = BorderBrush;
                _border.CornerRadius = CornerRadius;
                _border.Padding = Padding;
            }
            else
            {
                LogicalChildren.Remove(_border);
                _border = null;
            }

            if (requestMeasure) RequestMeasure();
            else RequestRender();
        }

        protected internal override IEnumerable<CGeometry> Measure(
            double entireWidth,
            double remainWidth)
        {
            bool applyDeco = HasBorderProperty;

            var parent = Parent;
            while (parent is CSpan parentSpan)
            {
                if (parentSpan.HasBorderProperty)
                {
                    applyDeco = false;
                    break;
                }
                else parent = parent.Parent;
            }

            if (applyDeco)
            {
                _border.Measure(Size.Infinity);

                entireWidth -= _border.DesiredSize.Width;
                remainWidth -= _border.DesiredSize.Width;
            }


            var metries = new List<CGeometry>();

            foreach (CInline inline in Content)
            {
                var adding = inline.Measure(entireWidth, remainWidth);

                if (applyDeco)
                {
                    adding = adding.Select(metry =>
                    {
                        if (metry is DecoratorGeometry)
                            // It's not called, 
                            throw new InvalidOperationException();

                        if (metry is TextGeometry tmetry && String.IsNullOrWhiteSpace(tmetry.Text))
                            return metry;

                        return new DecoratorGeometry(this, metry, _border);
                    });
                }

                var preCnt = metries.Count();
                metries.AddRange(adding);
                var pstCnt = metries.Count();

                CGeometry last = metries[metries.Count - 1];

                remainWidth = last.LineBreak ? entireWidth :
                               pstCnt - preCnt > 1 ? entireWidth - last.Width :
                               remainWidth - last.Width;
            }

            return metries;
        }
    }


}
