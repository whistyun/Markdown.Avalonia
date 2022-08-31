using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Metadata;
using ColorTextBlock.Avalonia.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

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

        public static readonly StyledProperty<BoxShadows> BoxShadowProperty =
            AvaloniaProperty.Register<CSpan, BoxShadows>(nameof(BoxShadow));

        public static readonly StyledProperty<Thickness> PaddingProperty =
            AvaloniaProperty.Register<CSpan, Thickness>(nameof(Padding));

        public static readonly StyledProperty<Thickness> MarginProperty =
            InputElement.MarginProperty.AddOwner<CSpan>();

        public static readonly StyledProperty<IEnumerable<CInline>> ContentProperty =
            AvaloniaProperty.Register<CSpan, IEnumerable<CInline>>(nameof(Content));

        static CSpan()
        {
            Observable.Merge<AvaloniaPropertyChangedEventArgs>(
                BorderThicknessProperty.Changed,
                CornerRadiusProperty.Changed,
                BoxShadowProperty.Changed,
                PaddingProperty.Changed,
                MarginProperty.Changed
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

        private Border? _border;

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
        public BoxShadows BoxShadow
        {
            get => GetValue(BoxShadowProperty);
            set => SetValue(BoxShadowProperty, value);
        }
        public Thickness Padding
        {
            get => GetValue(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }
        public Thickness Margin
        {
            get => GetValue(MarginProperty);
            set => SetValue(MarginProperty, value);
        }

        [Content]
        public IEnumerable<CInline> Content
        {
            get { return GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public CSpan()
        {
            var clst = new AvaloniaList<CInline>();
            // for xaml loader
            clst.CollectionChanged += (s, e) =>
            {
                if (e.OldItems != null)
                    foreach (var child in e.OldItems)
                        LogicalChildren.Remove((CInline)child);

                if (e.NewItems != null)
                    foreach (var child in e.NewItems)
                        LogicalChildren.Add((CInline)child);
            };

            Content = clst;
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
                CornerRadius != default(CornerRadius) ||
                Margin != default(Thickness) ||
                !BoxShadow.Equals(default(BoxShadows));

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
                _border.BoxShadow = BoxShadow;
                _border.Padding = Padding;
                _border.Margin = Margin;
            }
            else
            {
                if (_border is not null)
                    LogicalChildren.Remove(_border);
                _border = null;
            }

            if (requestMeasure) RequestMeasure();
            else RequestRender();
        }

        protected override IEnumerable<CGeometry> MeasureOverride(
            double entireWidth,
            double remainWidth)
        {
            if (_border is not null)
            {
                _border.Measure(Size.Infinity);

                entireWidth -= _border.DesiredSize.Width;
                remainWidth -= _border.DesiredSize.Width;
            }

            var metries = new List<CGeometry>();
            foreach (CInline inline in Content)
            {
                IEnumerable<CGeometry> addings = inline.Measure(entireWidth, remainWidth);
                foreach (var add in addings)
                {
                    metries.Add(add);
                    if (add.LineBreak) remainWidth = entireWidth;
                    else remainWidth -= add.Width;
                }
            }

            if (_border is not null)
            {
                var renew = new List<CGeometry>();

                var buffer = new List<CGeometry>();
                foreach (var adding in metries)
                {
                    // save linebreak before span
                    if (adding is LineBreakMarkGeometry && buffer.Count == 0)
                    {
                        renew.Add(adding);
                        continue;
                    }

                    buffer.Add(adding);

                    if (adding.LineBreak)
                    {
                        renew.Add(DecoratorGeometry.New(this, buffer, _border));
                        buffer.Clear();
                    }
                }

                if (buffer.Count != 0)
                {
                    renew.Add(DecoratorGeometry.New(this, buffer, _border));
                }

                metries = renew;
            }

            return metries;
        }
    }


}
