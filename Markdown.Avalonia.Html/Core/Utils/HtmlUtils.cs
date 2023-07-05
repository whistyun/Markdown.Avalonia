using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Markdown.Avalonia.Html.Core.Utils
{
    class HtmlUtils
    {
        private static readonly HashSet<string> s_blockTags = new()
        {
            "address",
            "article",
            "aside",
            "base",
            "basefont",
            "blockquote",
            "caption",
            "center",
            "col",
            "colgroup",
            "dd",
            "details",
            "dialog",
            "dir",
            "div",
            "dl",
            "dt",
            "fieldset",
            "figcaption",
            "figure",
            "footer",
            "form",
            "frame",
            "frameset",
            "h1",
            "h2",
            "h3",
            "h4",
            "h5",
            "h6",
            "head",
            "header",
            "hr",
            "html",
            "iframe",
            "legend",
            "li",
            "link",
            "main",
            "menu",
            "menuitem",
            "nav",
            "noframes",
            "ol",
            "optgroup",
            "option",
            "p",
            "param",
            "pre",
            "script",
            "section",
            "source",
            "style",
            "summary",
            "table",
            "textarea",
            "tbody",
            "td",
            "tfoot",
            "th",
            "thead",
            "title",
            "tr",
            "track",
            "ul",
        };

        public static bool IsBlockTag(string tagName) => s_blockTags.Contains(tagName.ToLower());
    }
}
