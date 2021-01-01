using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UnitTest.CTxt.Xamls
{
    public class Test5 : UserControl
    {
        public Test5()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
