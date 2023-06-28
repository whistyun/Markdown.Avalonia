using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using AvaloniaEdit;
using Markdown.Avalonia.Plugins;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Avalonia;
using System.Collections.ObjectModel;
using Markdown.Avalonia.SyntaxHigh.Extensions;
using Markdown.Avalonia.Parsers;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using Avalonia.Metadata;

namespace Markdown.Avalonia.SyntaxHigh
{
    internal class SyntaxOverride : IBlockOverride
    {
        private SyntaxHighlightProvider _provider;
        private SetupInfo _info;

        public string ParserName => "CodeBlocksWithLangEvaluator";


        public SyntaxOverride(ObservableCollection<Alias> aliases, SetupInfo info)
        {
            _provider = new SyntaxHighlightProvider(aliases);
            _info = info;
        }

        public IEnumerable<Control>? Convert(
            string text,
            Match match,
            ParseStatus status,
            IMarkdownEngine engine,
            out int parseTextBegin, out int parseTextEnd)
        {
            var closeTagPattern = new Regex($"\n[ ]*{match.Groups[1].Value}[ ]*\n");
            var closeTagMatch = closeTagPattern.Match(text, match.Index + match.Length);

            int codeEndIndex;
            if (closeTagMatch.Success)
            {
                codeEndIndex = closeTagMatch.Index;
                parseTextEnd = closeTagMatch.Index + closeTagMatch.Length;
            }
            else if (_info.EnablePreRenderingCodeBlock)
            {
                codeEndIndex = text.Length;
                parseTextEnd = text.Length;
            }
            else
            {
                parseTextBegin = parseTextEnd = -1;
                return null;
            }

            parseTextBegin = match.Index;

            string code = text.Substring(match.Index + match.Length, codeEndIndex - (match.Index + match.Length));
            string lang = match.Groups[2].Value;

            return Convert(lang, code);
        }

        private IEnumerable<Control> Convert(string lang, string code)
        {
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

                yield return result;
            }
            else
            {
                // check wheither style is set
                if (!ThemeDetector.IsAvalonEditSetup)
                {
                    SetupStyle();
                }

                var txtEdit = new TextEditor();
                txtEdit.Tag = lang;
                txtEdit.SetValue(SyntaxHighlightWrapperExtension.ProviderProperty, _provider);

                txtEdit.Text = code;
                txtEdit.HorizontalAlignment = HorizontalAlignment.Stretch;
                txtEdit.IsReadOnly = true;

                var result = new Border();
                result.Classes.Add(Markdown.CodeBlockClass);
                result.Child = txtEdit;

                yield return result;
            }
        }

        private static void SetupStyle()
        {
            if (Application.Current is null)
                return;

            string resourceUriTxt;
            if (ThemeDetector.IsFluentUsed)
                resourceUriTxt = "avares://AvaloniaEdit/Themes/Fluent/AvaloniaEdit.xaml";
            else if (ThemeDetector.IsSimpleUsed)
                resourceUriTxt = "avares://AvaloniaEdit/Themes/Simple/AvaloniaEdit.xaml";
            else
            {
                Debug.Print("Markdown.Avalonia.SyntaxHigh can't add style for AvaloniaEdit. See https://github.com/whistyun/Markdown.Avalonia/wiki/Setup-AvaloniaEdit-for-syntax-hightlighting");
                return;
            }

            var aeStyle = new StyleInclude(new Uri("avares://Markdown.Avalonia/"))
            {
                Source = new Uri(resourceUriTxt)
            };

            Application.Current.Styles.Add(aeStyle);
        }
    }
}
