using Avalonia.Controls;
using Markdown.Avalonia;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;

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

        private ComboBoxItem _styleName;
        public ComboBoxItem StyleName
        {
            get => _styleName;
            set => this.RaiseAndSetIfChanged(ref _styleName, value);
        }

        public MainWindowViewModel()
        {
            using (var stream = new FileStream("MainWindow.md", FileMode.Open))
            using (var reader = new StreamReader(stream))
            {
                Text = reader.ReadToEnd();
            }

            StyleName = new ComboBoxItem() { Content = nameof(MarkdownStyle.Standard) };
        }
    }
}
