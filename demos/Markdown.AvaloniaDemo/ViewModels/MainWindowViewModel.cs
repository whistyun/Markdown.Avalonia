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
using System.Windows.Input;

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

        public ICommand XamlParseResult { get; }

        public List<StyleViewModel> Styles { set; get; }

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
                new(nameof(MarkdownStyle.Standard)),
                new(nameof(MarkdownStyle.SimpleTheme)),
                new(nameof(MarkdownStyle.GithubLike))
            };

            SelectedStyle = Styles[1];

            var uri = new Uri("avares://Markdown.AvaloniaDemo/Assets/XamlTemplate.txt");
            using (var strm = AssetLoader.Open(uri))
            using (var reader = new StreamReader(strm))
            {
                EdittingStyleXamlText = reader.ReadToEnd();
            }

            XamlParseResult = new ErrorInfoAction(this);
        }
    }

    public class StyleViewModel
    {
        public string Name { get; }

        public StyleViewModel(string name)
        {
            Name = name;
        }
    }

    public class ErrorInfoAction : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private MainWindowViewModel _model;

        public ErrorInfoAction(MainWindowViewModel model)
        {
            this._model = model;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            _model.ErrorInfo = parameter?.ToString() ?? string.Empty;
        }
    }
}
