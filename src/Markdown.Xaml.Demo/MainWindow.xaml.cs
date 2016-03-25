using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Markdown.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(NavigationCommands.GoToPage, (sender, e) => Process.Start((string)e.Parameter)));
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            editSource.Text = LoadSample();
        }

        private string LoadSample()
        {
            var subjectType = GetType();
            var subjectAssembly = subjectType.Assembly;
            using (Stream stream = subjectAssembly.GetManifestResourceStream(subjectType.FullName + ".md"))
            {
                if (stream == null)
                {
                    return String.Format("Could not find sample text *{0}*.md", subjectType.FullName);
                }

                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

    }
}
