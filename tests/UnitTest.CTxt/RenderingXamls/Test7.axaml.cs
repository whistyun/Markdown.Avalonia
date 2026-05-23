using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UnitTest.CTxt.RenderingXamls
{
    public partial class Test7 : UserControl
    {
        public Test7()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
