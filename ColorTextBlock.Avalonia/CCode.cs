using System.Collections.Generic;
using Avalonia.Media;
using Avalonia;
using ColorTextBlock.Avalonia.Fonts;

namespace ColorTextBlock.Avalonia
{
    public class CCode : CSpan
    {
        public static readonly StyledProperty<FontFamily> MonospaceFontFamilyProperty =
            AvaloniaProperty.Register<CCode, FontFamily>(
                nameof(MonospaceFontFamily),
                defaultValue: FontFamilyCollecter.TryGetMonospace() ?? FontFamily.Default,
                inherits: true);

        public CCode() { }

        public CCode(IEnumerable<CInline> inlines) : base(inlines)
        {
            var obsvr = this.GetObservable(MonospaceFontFamilyProperty);
            Bind(FontFamilyProperty, obsvr);
        }


        public FontFamily MonospaceFontFamily
        {
            get { return GetValue(MonospaceFontFamilyProperty); }
            set { SetValue(MonospaceFontFamilyProperty, value); }
        }
    }
}
