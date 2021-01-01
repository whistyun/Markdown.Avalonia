using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace Markdown.Avalonia.Utils
{
    public interface IBitmapLoader
    {
        /// <summary>
        /// local file root path, the default is current directory.
        /// </summary>
        string AssetPathRoot { set; }

        Bitmap Get(string urlTxt);
    }
}
