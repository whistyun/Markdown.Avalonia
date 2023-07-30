using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Themes.Simple;
using Markdown.Avalonia;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        private StyleViewModel _selectedStyle;
        public StyleViewModel SelectedStyle
        {
            get => _selectedStyle;
            set => this.RaiseAndSetIfChanged(ref _selectedStyle, value);
        }

        private string _ErrorInfo;
        public string ErrorInfo
        {
            get => _ErrorInfo;
            set => this.RaiseAndSetIfChanged(ref _ErrorInfo, value);
        }

        public List<StyleViewModel> Styles { set; get; }

        public void XamlParseResult(string result) => ErrorInfo = result;

        public void TryParse() => AppendStyleXamlText = EdittingStyleXamlText;

        public MainWindowViewModel()
        {
            try
            {
                using (var stream = new FileStream("MainWindow.md", FileMode.Open))
                using (var reader = new StreamReader(stream))
                {
                    Text = reader.ReadToEnd();
                }
            }
            catch { }

            Styles = new List<StyleViewModel>
            {
                new StyleViewModel() { Name = nameof(MarkdownStyle.Standard) },
                new StyleViewModel() { Name = nameof(MarkdownStyle.SimpleTheme) },
                new StyleViewModel() { Name = nameof(MarkdownStyle.GithubLike) }
            };

            SelectedStyle = Styles[1];

            using (var strm = AssetLoader.Open(new Uri("avares://Markdown.AvaloniaDemo/Assets/XamlTemplate.txt")))
            using (var reader = new StreamReader(strm))
            {
                EdittingStyleXamlText = reader.ReadToEnd();
            }
        }
    }

    public class StyleViewModel
    {
        public string Name { get; set; }
    }
}
