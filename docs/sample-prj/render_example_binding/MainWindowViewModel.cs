using System.IO;

namespace render_example_binding
{
    class MainWindowViewModel
    {

        public MainWindowViewModel()
        {
            MarkdownDoc = File.ReadAllText("SampleMarkdown.md");
        }

        /*
         * This isn't exactly a valid ViewModel property.
         * Only property change at the constructor is reflected in the view.
         * 
         * Use INotifyPropertyChanged to reflect in the view when property is changed.
         */
        public string MarkdownDoc { get; }
    }
}
