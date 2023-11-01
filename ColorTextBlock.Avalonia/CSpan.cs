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

namespace ColorTextBlock.Avalonia
{
    /// <summary>
    /// Text decoration
    /// </summary>
    public class CSpan : CInline
    {
        /// <summary>
        /// The thickness of the border
        /// </summary>
        /// <seealso cref="BorderThickness"/>
        public static readonly StyledProperty<Thickness> BorderThicknessProperty =
            AvaloniaProperty.Register<CSpan, Thickness>(nameof(BorderThickness));

        /// <summary>
        /// The brush of the border.
        /// </summary>
        /// <seealso cref="BorderBrush"/>
        public static readonly StyledProperty<IBrush> BorderBrushProperty =
            AvaloniaProperty.Register<CSpan, IBrush>(nameof(BorderBrush));

        /// <summary>
        /// The radius of the border rounded corners
        /// </summary>
        /// <seealso cref="CornerRadius"/>
        public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
            AvaloniaProperty.Register<CSpan, CornerRadius>(nameof(CornerRadius));

        /// <summary>
        /// The box shadow effect parameters
        /// </summary>
        /// <seealso cref="BoxShadow"/>
        public static readonly StyledProperty<BoxShadows> BoxShadowProperty =
            AvaloniaProperty.Register<CSpan, BoxShadows>(nameof(BoxShadow));

        /// <summary>
        /// The padding to place around the Child control.
        /// </summary>
        /// <seealso cref="Padding"/>
        public static readonly StyledProperty<Thickness> PaddingProperty =
            AvaloniaProperty.Register<CSpan, Thickness>(nameof(Padding));

        /// <summary>
        /// The margin around the element.
        /// </summary>
        /// <seealso cref="Margin"/>
        public static readonly StyledProperty<Thickness> MarginProperty =
            InputElement.MarginProperty.AddOwner<CSpan>();

        /// <summary>
        /// THe content of the eleemnt
        /// </summary>
        /// <seealso cref="Content"/>
        public static readonly StyledProperty<IEnumerable<CInline>> ContentProperty =
            AvaloniaProperty.Register<CSpan, IEnumerable<CInline>>(nameof(Content));

        static CSpan()
        {
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

        /// <summary>
        /// The thickness of the border
        /// </summary>
        public Thickness BorderThickness
        {
            get => GetValue(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }

        /// <summary>
        /// The brush of the border.
        /// </summary>
        public IBrush BorderBrush
        {
            get => GetValue(BorderBrushProperty);
            set => SetValue(BorderBrushProperty, value);
        }

        /// <summary>
        /// The radius of the border rounded corners
        /// </summary>
        public CornerRadius CornerRadius
        {
            get => GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        /// <summary>
        /// The box shadow effect parameters
        /// </summary>
        public BoxShadows BoxShadow
        {
            get => GetValue(BoxShadowProperty);
            set => SetValue(BoxShadowProperty, value);
        }

        /// <summary>
        /// The padding to place around the Child control.
        /// </summary>
        public Thickness Padding
        {
            get => GetValue(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }

        /// <summary>
        /// The margin around the element.
        /// </summary>
        public Thickness Margin
        {
            get => GetValue(MarginProperty);
            set => SetValue(MarginProperty, value);
        }

        /// <summary>
        /// THe content of the eleemnt
        /// </summary>
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

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            switch (change.Property.Name)
            {
                case nameof(BorderThickness):
                case nameof(CornerRadius):
                case nameof(BoxShadow):
                case nameof(Padding):
                case nameof(Margin):
                    OnBorderPropertyChanged(true);
                    break;

                case nameof(BorderBrush):
                    OnBorderPropertyChanged(false);
                    break;
            }
        }

        private void OnBorderPropertyChanged(bool requestMeasure)
        {
            bool borderEnabled =
                BorderThickness != default ||
                Padding != default ||
                CornerRadius != default ||
                Margin != default ||
                !BoxShadow.Equals(default);

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

        public override string AsString() => String.Join("", Content.Select(c => c.AsString()));
    }
}
