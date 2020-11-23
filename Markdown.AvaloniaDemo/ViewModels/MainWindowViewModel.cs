using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Markdown.AvaloniaDemo.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _text;
        public string Text
        {
            get => _text;
            set => this.RaiseAndSetIfChanged(ref _text, value);
        }

        public MainWindowViewModel()
        {
            Text = "Avalonia  \r\n**two**";
        }
    }
}
