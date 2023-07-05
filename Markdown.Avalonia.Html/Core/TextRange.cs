using System;
using System.Collections.Generic;
using System.Text;

namespace Markdown.Avalonia.Html.Core
{
    public struct TextRange
    {
        public int Start { get; }
        public int End { get; }
        public int Length => End - Start;

        public TextRange(int start, int end)
        {
            Start = start;
            End = end;
        }
    }
}
