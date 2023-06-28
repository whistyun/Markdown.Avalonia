using Avalonia.Media;
using System;

namespace Markdown.Avalonia.SyntaxHigh.Extensions
{
    struct HSV
    {
        public int Hue;
        public byte Saturation;
        public byte Value;

        public HSV(Color color)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));
            int div = max - min;

            if (div == 0)
            {
                Hue = 0;
                Saturation = 0;
            }
            else
            {
                Hue =
                        (min == color.B) ? 60 * (color.G - color.R) / div + 60 :
                        (min == color.R) ? 60 * (color.B - color.G) / div + 180 :
                                           60 * (color.R - color.B) / div + 300;
                Saturation = (byte)div;
            }

            Value = (byte)max;
        }

        public Color ToColor()
        {
            if (Hue == 0 && Saturation == 0)
            {
                return Color.FromRgb(Value, Value, Value);
            }

            //byte c = Saturation;

            // int HueInt = Hue / 60;

            int x = (int)(Saturation * (1 - Math.Abs((Hue / 60f) % 2 - 1)));

            static Color FromRgb(int r, int g, int b)
                => Color.FromRgb((byte)r, (byte)g, (byte)b);


            return (Hue / 60) switch
            {
                1 => FromRgb(Value - Saturation + x, Value, Value - Saturation),
                2 => FromRgb(Value - Saturation, Value, Value - Saturation + x),
                3 => FromRgb(Value - Saturation, Value - Saturation + x, Value),
                4 => FromRgb(Value - Saturation + x, Value - Saturation, Value),
                5 or 6 => FromRgb(Value, Value - Saturation, Value - Saturation + x),
                _ => FromRgb(Value, Value - Saturation + x, Value - Saturation),
            };
        }
    }
}
