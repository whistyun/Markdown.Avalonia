using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace HowToUse
{
    public class UseBindingViewModel : ReactiveObject
    {
        private string _text;
        public string Text
        {
            get => _text;
            set => this.RaiseAndSetIfChanged(ref _text, value);
        }


        public UseBindingViewModel()
        {
            Text = "# Heading1\n\nHello markdown.\n\n* listitem1\n* listitem2\n\n| col1 | col2 | col3 |\n|------|------|------|\n| one  |------|------|\n| two  |------|------|\n\n> p>. and enhance syntax";
        }
    }
}
