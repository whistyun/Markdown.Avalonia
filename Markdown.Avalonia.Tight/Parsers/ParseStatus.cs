using System;
using System.Collections.Generic;
using System.Text;

namespace Markdown.Avalonia.Parsers
{
    struct ParseStatus
    {
        public static readonly ParseStatus Init = new ParseStatus
        {
            SupportTextAlignment = true
        };

        public bool SupportTextAlignment { get; set; }
    }
}
