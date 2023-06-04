using Avalonia;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace Markdown.Avalonia.Utils
{
    static class Helper
    {
        public static void ThrowInvalidOperation(string msg)
        {
            throw new InvalidOperationException(msg);
        }
    }
}
