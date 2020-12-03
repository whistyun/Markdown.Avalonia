using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace HowToUse
{
    public class WriteMarkdownInXaml : UserControl
    {
        public WriteMarkdownInXaml()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
