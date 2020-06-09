using System.IO;
using System.Windows;

namespace render_example_codebehind
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ReadMarkdownAndSetViewer();
        }


        private void ReadMarkdownAndSetViewer()
        {
            Markdownview.Markdown = File.ReadAllText("SampleMarkdown.md");
        }
    }
}
