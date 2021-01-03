using Avalonia.Controls;
using Markdown.Avalonia.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Markdown.Avalonia
{
    public interface IMarkdownEngine
    {
        /// <summary>
        /// The base of relative path.
        /// </summary>
        /// <remarks>
        /// This path is used as base path for load images which are indicated relative path.
        /// When the settter of this property is called, 
        /// the new value should be notified to to BitmapLoader.
        /// </remarks>
        string AssetPathRoot { get; set; }

        /// <summary>
        /// Handles the user action when the hyperlink is clicked.
        /// </summary>
        ICommand HyperlinkCommand { get; set; }

        /// <summary>
        /// Loads image from avares-uri, http(s)-uri, file-uri or file-path.
        /// </summary>
        IBitmapLoader BitmapLoader { get; set; }

        /// <summary>
        /// Transform markdown text to Control.
        /// </summary>
        /// <param name="text">The markdown text</param>
        /// <returns>The result of parsing markdown</returns>
        Control Transform(string text);
    }
}
