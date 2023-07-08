using System;
using System.Collections.Generic;
using System.Text;
using Engine = Markdown.Avalonia.Markdown;

namespace Markdown.Avalonia.Html.Core
{
    public enum Tags
    {
        TagTable,
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
            return tag switch
            {
                Tags.TagHeading1 => Engine.Heading1Class,
                Tags.TagHeading2 => Engine.Heading2Class,
                Tags.TagHeading3 => Engine.Heading3Class,
                Tags.TagHeading4 => Engine.Heading4Class,
                Tags.TagHeading5 => Engine.Heading5Class,
                Tags.TagHeading6 => Engine.Heading6Class,

                Tags.TagTable => Engine.TableClass,
                Tags.TagTableHeader => Engine.TableHeaderClass,
                Tags.TagEvenTableRow => Engine.TableRowEvenClass,
                Tags.TagOddTableRow => Engine.TableRowOddClass,

                Tags.TagBlockquote => Engine.BlockquoteClass,

                Tags.TagCodeBlock => Engine.CodeBlockClass,

                _ => tag.ToString().Substring(3)
            };
        }
    }
}
