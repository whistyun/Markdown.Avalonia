using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ColorTextBlock.Avalonia
{
    public class StringToRunConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            var txt = (string)value;
            txt = Regex.Replace(txt, "[\r\n \t]+", " ");
            return new CRun() { Text = String.IsNullOrEmpty(txt) ? " " : txt };
        }
    }
}
