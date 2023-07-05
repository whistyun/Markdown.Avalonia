namespace Markdown.Avalonia.Html.Core
{
    /// <summary>
    /// Behavior options about unknown tag.
    /// </summary>
    public enum UnknownTagsOption
    {
        /// <summary>
        /// Unknown tag is outputed as is.
        /// </summary>
        PassThrough,

        /// <summary>
        /// Unknown tag is removed from the result.
        /// </summary>
        Drop,

        /// <summary>
        /// The unknown tag itself is ignored.
        /// Only the content of the tag is evaluated.
        /// </summary>
        Bypass,

        /// <summary>
        /// Throw UnknownTagException.
        /// </summary>
        Raise,
    }
}
