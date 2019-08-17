using Markdown.Xaml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace Markdown.Demo
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            string currentAssembly = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string styleFilePath = Path.Combine(Path.GetDirectoryName(currentAssembly), Path.GetFileNameWithoutExtension(currentAssembly) + ".Styles.Default2.xaml");

            ResourceDictionary resources;
            using (Stream stream = new FileStream(styleFilePath, FileMode.Open, FileAccess.Read))
            {
                resources = (ResourceDictionary)XamlReader.Load(stream);
            }

            Styles = new List<StyleInfo>();

            var standardInf = new StyleInfo("Standard", MarkdownStyle.Standard);
            SelectedStyleInfo = standardInf;

            Styles.Add(standardInf);
            Styles.Add(new StyleInfo("Compact", MarkdownStyle.Compact));

            foreach (var rscNm in resources.Keys)
            {
                if (rscNm is string && resources[rscNm] is Style)
                {
                    Styles.Add(new StyleInfo((string)rscNm, (Style)resources[rscNm]));
                }
            }

            Styles.Add(new StyleInfo("Plain", null));
        }


        public StyleInfo _selectedStyleInfo;
        public StyleInfo SelectedStyleInfo
        {
            get { return _selectedStyleInfo; }
            set
            {
                if (_selectedStyleInfo == value) return;
                _selectedStyleInfo = value;
                FirePropertyChanged();
            }
        }

        public List<StyleInfo> _styles;
        public List<StyleInfo> Styles
        {
            get { return _styles; }
            set
            {
                if (_styles == value) return;
                _styles = value;
                FirePropertyChanged();
            }
        }

        /// <summary> <see cref="INotifyPropertyChanged"/> </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// <see cref="INotifyPropertyChanged"/>のイベント発火用
        /// </summary>
        /// <param name="propertyName"></param>
        protected void FirePropertyChanged([CallerMemberName]string propertyName = null)
        {
            if (PropertyChanged != null && propertyName != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, e);
            }
        }
    }
    public class StyleInfo
    {
        public string Name { set; get; }
        public Style Style { set; get; }

        public StyleInfo(string name, Style style)
        {
            Name = name;
            Style = style;
        }
    }
}
