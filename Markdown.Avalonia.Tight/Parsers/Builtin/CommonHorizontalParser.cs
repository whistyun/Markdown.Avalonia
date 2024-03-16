using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Markdown.Avalonia.Parsers.Builtin
{
    internal class CommonHorizontalParser : AbstractHorizontalParser
    {
        private static readonly Regex _horizontalCommonRules = new(@"
                ^[ ]{0,3}                   # Leading space
                    ([-*_])                 # $1: First marker ([markers])
                    (?>                     # Repeated marker group
                        [ ]{0,2}            # Zero, one, or two spaces.
                        \1                  # Marker character
                    ){2,}                   # Group repeated at least twice
                    [ ]*                    # Trailing spaces
                    \n                      # End of line.
                ", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        public CommonHorizontalParser() : base(_horizontalCommonRules)
        {
        }
    }
}
