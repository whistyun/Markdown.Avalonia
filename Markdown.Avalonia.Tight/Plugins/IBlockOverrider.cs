using Avalonia.Controls;
using Markdown.Avalonia.Parsers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Markdown.Avalonia.Plugins
{
    public interface IBlockOverride
    {
        string ParserName { get; }

        IEnumerable<Control>? Convert(
            string text, Match firstMatch, ParseStatus status,
            IMarkdownEngine engine,
            out int parseTextBegin, out int parseTextEnd);
    }
}
