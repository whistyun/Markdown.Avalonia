using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xml;

namespace MdXamlMigfree.Demo
{
    public class MarkdownXamlConverter : DependencyObject, IValueConverter
    {
        // Using a DependencyProperty as the backing store for Markdown.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MarkdownProperty =
            DependencyProperty.Register("Markdown",
                typeof(Markdown.Xaml.Markdown),
                typeof(MarkdownXamlConverter),
                new PropertyMetadata(null));

        private Lazy<Markdown.Xaml.Markdown> mMarkdown;

        public MarkdownXamlConverter()
        {
            mMarkdown = new Lazy<Markdown.Xaml.Markdown>(MakeMarkdown);
        }

        public Markdown.Xaml.Markdown Markdown
        {
            get { return (Markdown.Xaml.Markdown)GetValue(MarkdownProperty); }
            set { SetValue(MarkdownProperty, value); }
        }

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            var text = (string)value;

            var engine = Markdown ?? mMarkdown.Value;

            return AsXaml(engine.Transform(text));
        }

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private Markdown.Xaml.Markdown MakeMarkdown()
        {
            var markdown = new Markdown.Xaml.Markdown();
            return markdown;
        }

        private string AsXaml(object instance)
        {
            using (var writer = new StringWriter())
            {
                var settings = new XmlWriterSettings { Indent = true };
                using (var xmlWriter = XmlWriter.Create(writer, settings))
                {
                    XamlWriter.Save(instance, xmlWriter);
                }

                writer.WriteLine();
                return writer.ToString();
            }
        }
    }
}
