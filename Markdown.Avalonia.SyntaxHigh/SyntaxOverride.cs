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
using ColorDocument.Avalonia;
using ColorDocument.Avalonia.DocumentElements;

namespace Markdown.Avalonia.SyntaxHigh
{
    internal class SyntaxOverride : BlockOverride2
    {
        private SyntaxHighlightProvider _provider;
        private SetupInfo _info;

        public SyntaxOverride(ObservableCollection<Alias> aliases, SetupInfo info) : base("CodeBlocksWithLangEvaluator")
        {
            _provider = new SyntaxHighlightProvider(aliases);
            _info = info;
        }

        public override IEnumerable<DocumentElement>? Convert2(
            string text,
            Match match,
            ParseStatus status,
            IMarkdownEngine2 engine,
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

        private IEnumerable<DocumentElement> Convert(string lang, string code)
        {
            if (String.IsNullOrEmpty(lang))
            {
                yield return new PlainCodeBlockElement(code);
            }
            else
            {
                // check wheither style is set
                if (!ThemeDetector.IsAvalonEditSetup)
                {
                    SetupStyle();
                }

                yield return new CodeBlockElement(_provider, lang, code);
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
