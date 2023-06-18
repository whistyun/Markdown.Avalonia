using System;
using System.Collections.Generic;
using System.Text;

namespace Markdown.Avalonia.Parsers
{
    public struct ParseStatus
    {
        public static readonly ParseStatus Init = new ParseStatus
        {
            SupportTextAlignment = true
        };

        public bool SupportTextAlignment { get; internal set; }
    }
}
