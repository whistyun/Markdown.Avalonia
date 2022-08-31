using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Metadata;
using Markdown.Avalonia.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Markdown.Avalonia
{
    public class ContainerSwitch : AvaloniaDictionary<string, IContainerBlockHandler>, IContainerBlockHandler
    {
        public Border? ProvideControl(string assetPathRoot, string blockName, string lines)
        {
            // blockName may be "name [title] (url) {option}".
            // This collect some character until "[", "(" or "{".
            var trimedBlockName = blockName.Trim();
            var match = Regex.Match(trimedBlockName, @"[\(\[\{]");
            if (match.Success)
            {
                trimedBlockName = trimedBlockName.Substring(0, match.Index).Trim();
            }

            if (this.TryGetValue(trimedBlockName, out var processor))
            {
                return processor.ProvideControl(assetPathRoot, blockName, lines);
            }
            else return null;
        }
    }
}
