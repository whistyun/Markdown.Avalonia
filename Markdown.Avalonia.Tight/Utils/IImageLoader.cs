using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Media;

namespace Markdown.Avalonia.Utils
{
    public interface IImageLoader
    {
        /// <summary>
        /// local file root path or url, the default is current directory.
        /// </summary>
        string AssetPathRoot { set; }

        IImage? Get(string urlTxt);
    }
}
