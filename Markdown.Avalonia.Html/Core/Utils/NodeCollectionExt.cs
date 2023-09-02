using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Markdown.Avalonia.Html.Core.Utils
{
    internal static class NodeCollectionExt
    {
        public static List<HtmlNode> SkipComment(this HtmlNodeCollection list)
        {
            var count = list.Count;

            var store = new List<HtmlNode>(count);

            for (var i = 0; i < count; ++i)
            {
                var e = list[i];
                if (e.IsComment()) continue;

                store.Add(e);
            }

            return store;
        }

        public static bool IsElement(this HtmlNode node, string tagName)
        {
            return node.NodeType == HtmlNodeType.Element
                && string.Equals(node.Name, tagName, StringComparison.OrdinalIgnoreCase);
        }
        public static bool IsComment(this HtmlNode node) => node is HtmlCommentNode;

        public static List<HtmlNode> CollectTag(this HtmlNodeCollection list)
        {
            var count = list.Count;

            var store = new List<HtmlNode>(count);

            for (var i = 0; i < count; ++i)
            {
                var e = list[i];
                if (e.NodeType != HtmlNodeType.Element) continue;

                store.Add(e);
            }

            return store;
        }

        public static List<HtmlNode> CollectTag(this HtmlNodeCollection list, string tagName)
        {
            var count = list.Count;

            var store = new List<HtmlNode>(count);

            for (var i = 0; i < count; ++i)
            {
                var e = list[i];
                if (e.IsElement(tagName))
                    store.Add(e);
            }

            return store;
        }

        public static List<HtmlNode> CollectTag(this HtmlNodeCollection list, params string[] tagNames)
        {
            var count = list.Count;

            var store = new List<HtmlNode>(count);

            for (var i = 0; i < count; ++i)
            {
                var e = list[i];
                if (e.NodeType != HtmlNodeType.Element) continue;

                if (tagNames.Any(tagName => e.IsElement(tagName)))
                    store.Add(e);
            }

            return store;
        }

        public static bool HasOneTag(
            this HtmlNodeCollection list,
            string tagName,
#if NET6_0_OR_GREATER
            [MaybeNullWhen(false)]
            out HtmlNode child)
#else
            out HtmlNode child)
#endif
        {
            var children = CollectTag(list, tagName);

            if (children.Count == 1)
            {
                child = children[0];
                return true;
            }
            else
            {
                child = null!;
                return false;
            }
        }

        public static bool TryCastTextNode(this HtmlNodeCollection list, out List<HtmlTextNode> texts)
        {
            var count = list.Count;

            texts = new List<HtmlTextNode>(count);

            for (var i = 0; i < count; ++i)
            {
                var e = list[i];

                if (e is HtmlTextNode txtNd)
                {
                    texts.Add(txtNd);
                    continue;
                }

                if (e.IsComment())
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        public static Tuple<List<HtmlNode>, List<HtmlNode>> Filter(this IEnumerable<HtmlNode> list, Func<HtmlNode, bool> filterFunc)
        {
            var filterIn = new List<HtmlNode>();
            var filterOut = new List<HtmlNode>();

            foreach (var e in list)
            {
                if (filterFunc(e))
                {
                    filterIn.Add(e);
                }
                else
                {
                    filterOut.Add(e);
                }
            }

            return Tuple.Create(filterIn, filterOut);
        }
    }
}
