using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Markdown.Avalonia.SyntaxHigh
{
    public class SyntaxSetup
    {
        public IEnumerable<KeyValuePair<string, Func<Match, Control>>> GetOverrideConverters()
        {
            yield return new KeyValuePair<string, Func<Match, Control>>(
                "CodeBlocksWithLangEvaluator",
                CodeBlocksEvaluator);
        }

        private Border CodeBlocksEvaluator(Match match)
        {
            var lang = match.Groups[2].Value;
            var code = match.Groups[3].Value;

            if (String.IsNullOrEmpty(lang))
            {
                var ctxt = new TextBlock()
                {
                    Text = code,
                    TextWrapping = TextWrapping.NoWrap
                };
                ctxt.Classes.Add(Markdown.CodeBlockClass);

                var scrl = new ScrollViewer();
                scrl.Classes.Add(Markdown.CodeBlockClass);
                scrl.Content = ctxt;
                scrl.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

                var result = new Border();
                result.Classes.Add(Markdown.CodeBlockClass);
                result.Child = scrl;

                return result;
            }
            else
            {
                // check wheither style is set
                if (!ThemeDetector.IsAvalonEditSetup)
                {
                    var aeStyle = new StyleInclude(new Uri("avares://Markdown.Avalonia/"))
                    {
                        Source = new Uri("avares://AvaloniaEdit/AvaloniaEdit.xaml")
                    };
                    Application.Current.Styles.Add(aeStyle);
                }

                var txtEdit = new TextEditor();
                var highlight = HighlightingManager.Instance.GetDefinitionByExtension("." + lang);
                txtEdit.Tag = lang;
                //txtEdit.SetValue(TextEditor.SyntaxHighlightingProperty, highlight);

                txtEdit.Text = code;
                txtEdit.HorizontalAlignment = HorizontalAlignment.Stretch;
                txtEdit.IsReadOnly = true;

                var result = new Border();
                result.Classes.Add(Markdown.CodeBlockClass);
                result.Child = txtEdit;

                return result;
            }
        }
    }
}
