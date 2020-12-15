using Avalonia;
using Avalonia.Media;
using ColorTextBlock.Avalonia.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FStyle = Avalonia.Media.FontStyle;

namespace ColorTextBlock.Avalonia
{
    public class CHyperlink : CSpan
    {
        public static readonly StyledProperty<IBrush> HoverBackgroundProperty =
            AvaloniaProperty.Register<CHyperlink, IBrush>(nameof(Foreground));

        public static readonly StyledProperty<IBrush> HoverForegroundProperty =
            AvaloniaProperty.Register<CHyperlink, IBrush>(nameof(Foreground));

        public IBrush HoverBackground
        {
            get { return GetValue(HoverBackgroundProperty); }
            set { SetValue(HoverBackgroundProperty, value); }
        }

        public IBrush HoverForeground
        {
            get { return GetValue(HoverForegroundProperty); }
            set { SetValue(HoverForegroundProperty, value); }
        }

        public Action<string> Command { get; set; }
        public string CommandParameter { get; set; }

        public CHyperlink(IEnumerable<CInline> inlines) : base(inlines)
        {
        }


        protected internal override IEnumerable<CGeometry> Measure(
            double entireWidth,
            double remainWidth)
        {
            var metrics = base.Measure(
                entireWidth,
                remainWidth);

            foreach (CGeometry metry in metrics)
            {
                metry.OnClick = () => Command?.Invoke(CommandParameter);

                metry.OnMousePressed = () =>
                {
                    PseudoClasses.Add(":pressed");
                };

                metry.OnMouseReleased = () =>
                {
                    PseudoClasses.Remove(":pressed");
                };

                metry.OnMouseEnter = () =>
                {
                    PseudoClasses.Add(":pointerover");
                    PseudoClasses.Add(":hover");


                    IEnumerable<TextGeometry> tmetries =
                        (metry is DecoratorGeometry d) ?
                            d.Targets.OfType<TextGeometry>() :
                            new[] { metry as TextGeometry };
                    if (tmetries != null)
                    {
                        foreach (var tmetry in tmetries)
                        {
                            tmetry.TemporaryForeground = HoverForeground;
                            tmetry.TemporaryBackground = HoverBackground;
                        }
                        RequestRender();
                    }
                };

                metry.OnMouseLeave = () =>
                {
                    PseudoClasses.Remove(":pointerover");
                    PseudoClasses.Remove(":hover");

                    IEnumerable<TextGeometry> tmetries =
                        (metry is DecoratorGeometry d) ?
                            d.Targets.OfType<TextGeometry>() :
                            new[] { metry as TextGeometry };
                    if (tmetries != null)
                    {
                        foreach (var tmetry in tmetries)
                        {
                            tmetry.TemporaryForeground = null;
                            tmetry.TemporaryBackground = null;
                        }
                        RequestRender();
                    }
                };

                yield return metry;
            }
        }
    }
}
