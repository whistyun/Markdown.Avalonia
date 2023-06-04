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

        private string _ErrorInfo;
        public string ErrorInfo
        {
            get => _ErrorInfo;
            set => this.RaiseAndSetIfChanged(ref _ErrorInfo, value);
        }

        private string _AssetPathRootText;
        public string AssetPathRootText
        {
            get => _AssetPathRootText;
            set => this.RaiseAndSetIfChanged(ref _AssetPathRootText, value);
        }

        private string _SourceText;
        public string SourceText
        {
            get => _SourceText;
            set => this.RaiseAndSetIfChanged(ref _SourceText, value);
        }

        private string _AssetPathRoot;
        public string AssetPathRoot
        {
            get => _AssetPathRoot;
            set
            {
                _AssetPathRoot = value;
                this.RaisePropertyChanged();
            }
        }

        private Uri _Source;
        public Uri Source
        {
            get => _Source;
            set
            {
                _Source = value;
                this.RaisePropertyChanged();
            }
        }

        public void XamlParseResult(string result) => ErrorInfo = result;

        public void TryParse() => AppendStyleXamlText = EdittingStyleXamlText;

        public MainWindowViewModel()
        {
            using (var stream = new FileStream("MainWindow.md", FileMode.Open))
            using (var reader = new StreamReader(stream))
            {
                Text = reader.ReadToEnd();
            }

            using (var strm = AssetLoader.Open(new Uri("avares://Markdown.AvaloniaFluentDemo/Assets/XamlTemplate.txt")))
            using (var reader = new StreamReader(strm))
            {
                EdittingStyleXamlText = reader.ReadToEnd();
            }
        }

        public void ApplyAssetPathRoot()
            => AssetPathRoot = AssetPathRootText;

        public void ApplySource()
            => Source = new Uri(SourceText);
    }
}
