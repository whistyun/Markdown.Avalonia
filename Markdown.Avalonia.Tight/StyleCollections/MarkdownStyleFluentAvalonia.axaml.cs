﻿using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Text;

namespace Markdown.Avalonia.StyleCollections
{
    class MarkdownStyleFluentAvalonia : Styles
    {
        public MarkdownStyleFluentAvalonia()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}