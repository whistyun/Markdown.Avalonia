using System;
using System.Collections.Generic;
using System.Text;

namespace Markdown.Avalonia.Utils
{
    static class Helper
    {
        public static void ThrowArgNull(string msg)
        {
            throw new ArgumentNullException(msg);
        }

        public static void ThrowInvalidOperation(string msg) {
            throw new InvalidOperationException(msg);
        }
    }
}
