using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Markdown.Avalonia;
using UnitTest.Base.Utils;

namespace Markdown.AvaloniaFluentDemo.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SetupComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }


        private void SetupComponent()
        {
            var tabitem = this.FindControl<TabItem>("SourceTab");
            var viewer = this.FindControl<MarkdownScrollViewer>("MarkdownViewer");
            var txtbox = this.FindControl<TextBox>("MarkdownSource");

            var engine = new global::Markdown.Avalonia.Markdown();
            var reverter = new BrokenXamlWriter();

            void UpdateMarkdown(string mdtxt)
            {
                var mdctl = engine.Transform(mdtxt);
                txtbox.Text = Util.AsXaml(mdctl);
            }

            TabItem.IsSelectedProperty.Changed.AddClassHandler<TabItem>(
                (ctrl, x) =>
                {
                    if (ctrl == tabitem && tabitem.IsSelected)
                        UpdateMarkdown(viewer.Markdown);
                }
            );

            MarkdownScrollViewer.MarkdownProperty.Changed.AddClassHandler<MarkdownScrollViewer>(
                (ctrl, x) =>
                {
                    if (tabitem.IsSelected)
                        UpdateMarkdown((string)x.NewValue);
                }
            );
        }
    }
}
