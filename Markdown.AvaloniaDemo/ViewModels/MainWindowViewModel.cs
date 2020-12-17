using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
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

        private StyleViewModel _selectedStyle;
        public StyleViewModel SelectedStyle
        {
            get => _selectedStyle;
            set => this.RaiseAndSetIfChanged(ref _selectedStyle, value);
        }

        private ThemeViewModel _selectedTheme;
        public ThemeViewModel SelectedTheme
        {
            get => _selectedTheme;
            set => this.RaiseAndSetIfChanged(ref _selectedTheme, value);
        }

        public List<StyleViewModel> Styles { set; get; }
        public List<ThemeViewModel> Themes { set; get; }

        public MainWindowViewModel()
        {
            using (var stream = new FileStream("MainWindow.md", FileMode.Open))
            using (var reader = new StreamReader(stream))
            {
                Text = reader.ReadToEnd();
            }

            Styles = new List<StyleViewModel>();
            Styles.Add(new StyleViewModel() { Name = nameof(MarkdownStyle.Standard) });
            Styles.Add(new StyleViewModel() { Name = nameof(MarkdownStyle.GithubLike) });

            SelectedStyle = Styles[0];


            Themes = new List<ThemeViewModel>();
            Themes.Add(new ThemeViewModel()
            {
                Name = "BaseLight",
                Source = new Uri("resm:Avalonia.Themes.Default.Accents.BaseLight.xaml?assembly=Avalonia.Themes.Default")
            });

            Themes.Add(new ThemeViewModel()
            {
                Name = "BaseDark",
                Source = new Uri("resm:Avalonia.Themes.Default.Accents.BaseDark.xaml?assembly=Avalonia.Themes.Default")
            });

            SelectedTheme = Themes[0];
        }
    }

    public class StyleViewModel
    {
        public string Name { get; set; }
    }

    public class ThemeViewModel
    {
        public string Name { get; set; }
        public Uri Source { get; set; }
    }
}
