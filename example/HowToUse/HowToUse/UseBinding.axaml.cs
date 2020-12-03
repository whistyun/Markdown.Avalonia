using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace HowToUse
{
    public class UseBinding : UserControl
    {
        public UseBinding()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
