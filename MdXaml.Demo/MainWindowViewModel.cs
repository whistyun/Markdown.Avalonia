using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace MdXaml.Demo
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

            Styles.Add(new StyleInfo("Plain", null));
            Styles.Add(new StyleInfo("Standard", MarkdownStyle.Standard));
            Styles.Add(new StyleInfo("Compact", MarkdownStyle.Compact));

            SelectedStyleInfo = Styles[1];

            foreach (var rscNm in resources.Keys)
            {
                if (rscNm is string && resources[rscNm] is Style)
                {
                    Styles.Add(new StyleInfo((string)rscNm, (Style)resources[rscNm]));
                }
            }

            var subjectType = typeof(MainWindow);
            var subjectAssembly = GetType().Assembly;
            using (Stream stream = subjectAssembly.GetManifestResourceStream(subjectType.FullName + ".md"))
            {

                if (stream == null)
                {
                    Text = String.Format("Could not find sample text *{0}*.md", subjectType.FullName);
                }

                using (StreamReader reader = new StreamReader(stream))
                {
                    Text = reader.ReadToEnd();
                }
            }

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

        public string _text;
        public string Text
        {
            get { return _text; }
            set
            {
                if (_text == value) return;
                _text = value;

                if (TextChangeEvent == null || TextChangeEvent.Status >= TaskStatus.RanToCompletion)
                {
                    TextChangeEvent = Task.Run(() =>
                    {
                        Task.Delay(100);
                    retry:
                        var oldVal = _text;

                        Thread.MemoryBarrier();
                        FirePropertyChanged(nameof(Text));

                        Thread.MemoryBarrier();
                        if (oldVal != _text) goto retry;
                    });
                }
            }
        }

        private Task TextChangeEvent;


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

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object val)
        {
            if (val is StyleInfo sf)
            {
                return Name == sf.Name;
            }
            else return false;
        }

        public static bool operator ==(StyleInfo left, StyleInfo right)
        {
            if (Object.ReferenceEquals(left, right)) return true;
            if (Object.ReferenceEquals(left, null)) return false;
            return left.Equals(right);
        }

        public static bool operator !=(StyleInfo left, StyleInfo right)
        {
            return !(left == right);
        }
    }
}