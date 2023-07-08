using Avalonia.Controls;
using Avalonia.Layout;
using AvaloniaEdit;
using ColorTextBlock.Avalonia;
using Markdown.Avalonia.SyntaxHigh;
using Markdown.Avalonia.SyntaxHigh.Extensions;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Markdown.Avalonia.Html.Core.Utils
{
    static class DocUtils
    {
        public static Control CreateCodeBlock(string? lang, string code, ReplaceManager manager, SyntaxHighlightProvider provider)
        {
            var txtEdit = new TextEditor();

            if (!String.IsNullOrEmpty(lang))
            {
                txtEdit.Tag = lang;
                txtEdit.SetValue(SyntaxHighlightWrapperExtension.ProviderProperty, provider);
            }

            txtEdit.Text = code;
            txtEdit.HorizontalAlignment = HorizontalAlignment.Stretch;
            txtEdit.IsReadOnly = true;

            var result = new Border();
            result.Classes.Add(Tags.TagCodeBlock.GetClass());
            result.Child = txtEdit;

            return result;
        }

        public static void TrimStart(CInline? inline)
        {
            if (inline is null) return;

            if (inline is CSpan span)
            {
                TrimStart(span.Content.FirstOrDefault());
            }
            else if (inline is CRun run)
            {
                run.Text = run.Text.TrimStart();
            }
        }

        public static void TrimEnd(CInline? inline)
        {
            if (inline is null) return;

            if (inline is CSpan span)
            {
                TrimEnd(span.Content.LastOrDefault());
            }
            else if (inline is CRun run)
            {
                run.Text = run.Text.TrimEnd();
            }
        }
    }
}
