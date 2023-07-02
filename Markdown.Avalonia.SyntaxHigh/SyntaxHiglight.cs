using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Metadata;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit;
using Markdown.Avalonia.Plugins;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Styling;
using Markdown.Avalonia.SyntaxHigh.StyleCollections;
using Avalonia.Collections;
using System.Collections.ObjectModel;

namespace Markdown.Avalonia.SyntaxHigh
{
    public class SyntaxHighlight : IMdAvPlugin
    {
        [Content]
        public ObservableCollection<Alias> Aliases { get; } = new ObservableCollection<Alias>();

        public void Setup(SetupInfo info)
        {
            info.Register(new SyntaxOverride(Aliases, info));
            info.Register(new StyleEdit());
        }
    }
}
