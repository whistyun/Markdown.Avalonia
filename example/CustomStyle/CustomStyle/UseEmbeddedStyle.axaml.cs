using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CustomStyle
{
    public class UseEmbeddedStyle : UserControl
    {
        public UseEmbeddedStyle()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
