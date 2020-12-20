using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Platform;
using Avalonia.Styling;
using Markdown.Avalonia;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;

namespace Markdown.AvaloniaFluentDemo.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _text;
        public string Text
        {
            get => _text;
            set => this.RaiseAndSetIfChanged(ref _text, value);
        }

        private string _edittingStyleXamlText;
        public string EdittingStyleXamlText
        {
            get => _edittingStyleXamlText;
            set => this.RaiseAndSetIfChanged(ref _edittingStyleXamlText, value);
        }

        private string _appendStyleXamlText;
        public string AppendStyleXamlText
        {
            get => _appendStyleXamlText;
            set => this.RaiseAndSetIfChanged(ref _appendStyleXamlText, value);
        }

        private ThemeViewModel _selectedTheme;
        public ThemeViewModel SelectedTheme
        {
            get => _selectedTheme;
            set => this.RaiseAndSetIfChanged(ref _selectedTheme, value);
        }

        private string _ErrorInfo;
        public string ErrorInfo
        {
            get => _ErrorInfo;
            set => this.RaiseAndSetIfChanged(ref _ErrorInfo, value);
        }

        public List<ThemeViewModel> Themes { set; get; }

        public void XamlParseResult(string result) => ErrorInfo = result;

        public void TryParse() => AppendStyleXamlText = EdittingStyleXamlText;

        public MainWindowViewModel()
        {
            using (var stream = new FileStream("MainWindow.md", FileMode.Open))
            using (var reader = new StreamReader(stream))
            {
                Text = reader.ReadToEnd();
            }

            Themes = new List<ThemeViewModel>();
            Themes.Add(new ThemeViewModel()
            {
                Name = "FluentLight",
                Source = new Uri("avares://Avalonia.Themes.Fluent/FluentLight.xaml")
            });

            Themes.Add(new ThemeViewModel()
            {
                Name = "FluentDark",
                Source = new Uri("avares://Avalonia.Themes.Fluent/FluentDark.xaml")
            });

            var loader = AvaloniaLocator.Current.GetService<IAssetLoader>();
            using (var strm = loader.Open(new Uri("avares://Markdown.AvaloniaFluentDemo/Assets/XamlTemplate.txt")))
            using (var reader = new StreamReader(strm))
            {
                EdittingStyleXamlText = reader.ReadToEnd();
            }
        }
    }

    public class ThemeViewModel
    {
        public string Name { get; set; }
        public Uri Source { get; set; }
    }
}
