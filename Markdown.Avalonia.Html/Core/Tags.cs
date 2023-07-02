using System;
using System.Collections.Generic;
using System.Text;

namespace Markdonw.Avalonia.Html.Core
{
    public enum Tags
    {
        TagTableHeader,
        TagTableBody,
        TagEvenTableRow,
        TagOddTableRow,
        TagTableFooter,
        TagTableCaption,

        TagBlockquote,
        TagBold,
        TagCite,
        TagFooter,
        TagItalic,
        TagMark,
        TagStrikethrough,
        TagSubscript,
        TagSuperscript,
        TagUnderline,
        TagHyperlink,

        TagFigure,
        TagRuleSingle,

        TagHeading1,
        TagHeading2,
        TagHeading3,
        TagHeading4,
        TagHeading5,
        TagHeading6,

        TagCodeSpan,
        TagCodeBlock,
        TagAddress,
        TagArticle,
        TagAside,
        TagCenter,
        TagAbbr,
        TagBdi
    }

    public static class TagsExt
    {
        public static string GetClass(this Tags tag)
        {
            return tag.ToString().Substring(3);
        }
    }
}
