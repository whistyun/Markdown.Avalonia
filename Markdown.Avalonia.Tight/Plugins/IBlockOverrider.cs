using Avalonia.Controls;
using Markdown.Avalonia.Parsers;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Markdown.Avalonia.Plugins
{
    /// <summary>
    /// The interface to change the parsing method for block elements.
    /// </summary>
    // ブロック要素の解析方法を変更するためのインターフェイス
    public interface IBlockOverride
    {
        /// <summary>
        /// The target name to override.
        /// </summary>
        string ParserName { get; }

        /// <summary>
        /// Translates markdown to visual elements.
        /// </summary>
        // Markdownを表示要素に変換します。
        IEnumerable<Control>? Convert(
            string text, Match firstMatch, ParseStatus status,
            IMarkdownEngine engine,
            out int parseTextBegin, out int parseTextEnd);
    }
}
