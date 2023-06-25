using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace Markdown.Avalonia.Utils
{
    [Obsolete("see https://github.com/whistyun/Markdown.Avalonia/wiki/How-to-migrages-to-ver11")]
    public interface IBitmapLoader
    {
        /// <summary>
        /// local file root path or url, the default is current directory.
        /// </summary>
        string AssetPathRoot { set; }

        Bitmap? Get(string urlTxt);
    }
}
