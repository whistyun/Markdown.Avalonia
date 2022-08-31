using Avalonia.Controls;

namespace Markdown.Avalonia.Utils
{
  /// <summary>
  /// support for Container blocks
  /// https://talk.commonmark.org/t/generic-directives-plugins-syntax/444
  /// </summary>
  public interface IContainerBlockHandler
  {
    /// <summary>
    /// Custom text parsing & content generation
    /// Based on https://talk.commonmark.org/t/generic-directives-plugins-syntax/444
    /// </summary>
    /// <param name="assetPathRoot">Asset Path Root</param>
    /// <param name="blockName">Block Name</param>
    /// <param name="lines">Text to parse</param>
    /// <returns>Controls; or null when no container founds</returns>
    Border? ProvideControl( string assetPathRoot,
                           string blockName,
                           string lines );
  }
}