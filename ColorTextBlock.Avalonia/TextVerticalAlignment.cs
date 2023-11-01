
namespace ColorTextBlock.Avalonia
{
    /// <summary>
    /// The vertical position of text within line
    /// </summary>
    /// <seealso cref="https://github.com/whistyun/Markdown.Avalonia/issues/28"/>
    public enum TextVerticalAlignment
    {
        /// <summary>
        /// Text elements are placed at the top of the line.
        /// </summary>
        Top,
        /// <summary>
        /// Text elements are placed at the middle of the line.
        /// </summary>
        Center,
        /// <summary>
        /// Text elements are placed at the bottom of the line.
        /// </summary>
        Bottom,

        /// <summary>
        /// Text elements are placed at the bottom of the line.
        /// This treats only the height of text element and ignores the padding of element.
        /// Therefore vertical positions of the text will be aligned.
        /// </summary>
        Base,
    }
}
