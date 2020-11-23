using System;
using System.Collections.Generic;
using System.Text;

namespace Markdown.Avalonia
{
    static class NumberToOrder
    {
        public static string ToRoman(int number)
        {
            // roman can treat between 1 and 3999
            if (number < 0 || number >= 4000) return number.ToString();

            if (number >= 1000) return "M" + ToRoman(number - 1000);
            if (number >= 900) return "CM" + ToRoman(number - 900);
            if (number >= 500) return "D" + ToRoman(number - 500);
            if (number >= 400) return "CD" + ToRoman(number - 400);
            if (number >= 100) return "C" + ToRoman(number - 100);
            if (number >= 90) return "XC" + ToRoman(number - 90);
            if (number >= 50) return "L" + ToRoman(number - 50);
            if (number >= 40) return "XL" + ToRoman(number - 40);
            if (number >= 10) return "X" + ToRoman(number - 10);
            if (number >= 9) return "IX" + ToRoman(number - 9);
            if (number >= 5) return "V" + ToRoman(number - 5);
            if (number >= 4) return "IV" + ToRoman(number - 4);
            if (number >= 1) return "I" + ToRoman(number - 1);
            if (number == 0) return "";

            throw new ArgumentOutOfRangeException("something bad happened");
        }

        public static string ToLatin(int number)
        {
            var buff = new StringBuilder();

            while (number > 0)
            {
                var mod = (number - 1) % 26;
                buff.Insert(0, (char)(mod + 'A'));

                number = (number - mod) / 26;
            }

            return buff.ToString();
        }
    }
}
