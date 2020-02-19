using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace MdXaml
{
    public class MarkdownScrollViewer : FlowDocumentScrollViewer
    {
        public static readonly DependencyProperty MarkdownProperty =
            DependencyProperty.Register(
                nameof(Markdown),
                typeof(string),
                typeof(MarkdownScrollViewer),
                new PropertyMetadata("", UpdateMarkdown));


        public static readonly DependencyProperty MarkdownStyleProperty =
            DependencyProperty.Register(
                nameof(MarkdownStyle),
                typeof(Style),
                typeof(MarkdownScrollViewer),
                new PropertyMetadata(MdXaml.MarkdownStyle.Standard, UpdateStyle));

        public static readonly DependencyProperty MarkdownStyleNameProperty =
            DependencyProperty.Register(
            nameof(MarkdownStyleName),
            typeof(string),
            typeof(MarkdownScrollViewer),
            new PropertyMetadata(nameof(MdXaml.MarkdownStyle.Standard), UpdateStyleName));

        private static void UpdateMarkdown(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MarkdownScrollViewer owner)
            {
                var doc = owner.engine.Transform((string)e.NewValue ?? "");
                owner.SetCurrentValue(DocumentProperty, doc);
            }
        }

        private static void UpdateStyle(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MarkdownScrollViewer owner)
            {
                owner.engine.DocumentStyle = (Style)e.NewValue;
            }
        }

        private static void UpdateStyleName(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MarkdownScrollViewer owner)
            {
                var newName = (string)e.NewValue;

                if (newName == null) return;

                var prop = typeof(MarkdownStyle).GetProperty(newName);
                if (prop == null) return;

                owner.MarkdownStyle = (Style)prop.GetValue(null);
            }
        }

        private Markdown engine = new Markdown();

        public string Markdown
        {
            get { return (string)GetValue(MarkdownProperty); }
            set { SetValue(MarkdownProperty, value); }
        }

        public Style MarkdownStyle
        {
            get { return (Style)GetValue(MarkdownStyleProperty); }
            set { SetValue(MarkdownStyleProperty, value); }
        }

        public string MarkdownStyleName
        {
            get { return (string)GetValue(MarkdownStyleNameProperty); }
            set { SetValue(MarkdownStyleNameProperty, value); }
        }

        public MarkdownScrollViewer()
        {
            engine = new Markdown();
        }
    }
}
