using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ColorTextBlock.Avalonia;

namespace UnitTest.CTxt.Xamls
{
    public partial class CTextBlockData : UserControl
    {
        public CTextBlockData()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
