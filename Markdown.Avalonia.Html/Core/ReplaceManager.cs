using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Markdown.Avalonia.Html.Core.Parsers;
using Markdown.Avalonia.Html.Core.Utils;
using Markdown.Avalonia.Html.Core.Parsers.MarkdigExtensions;
using System.Linq;
using System.Text;
using Markdown.Avalonia;
using Avalonia.Controls;
using Avalonia;
using ColorTextBlock.Avalonia;
using Markdown.Avalonia.Parsers;
using Markdown.Avalonia.Plugins;
using Markdown.Avalonia.SyntaxHigh;

namespace Markdown.Avalonia.Html.Core
{
    public class ReplaceManager
    {
        private readonly Dictionary<string, List<IInlineTagParser>> _inlineBindParsers;
        private readonly Dictionary<string, List<IBlockTagParser>> _blockBindParsers;
        private readonly Dictionary<string, List<ITagParser>> _bindParsers;

        private TextNodeParser textParser;

        public ReplaceManager(SyntaxHighlight highlight, SetupInfo info)
        {
            _inlineBindParsers = new();
            _blockBindParsers = new();
            _bindParsers = new();

            UnknownTags = UnknownTagsOption.Drop;

            Register(new TagIgnoreParser());
            Register(new CommentParsre());
            Register(new ImageParser(info));
            Register(new CodeBlockParser(highlight));
            //Register(new CodeSpanParser());
            Register(new OrderListParser());
            Register(new UnorderListParser());
            Register(textParser = new TextNodeParser());
            Register(new HorizontalRuleParser());
            Register(new FigureParser());
            Register(new GridTableParser());
            Register(new InputParser());
            Register(new ButtonParser());
            Register(new TextAreaParser());
            Register(new ProgressParser());
            Register(new DetailsParser());

            foreach (var parser in TypicalBlockParser.Load())
                Register(parser);

            foreach (var parser in TypicalInlineParser.Load())
                Register(parser);
        }

        public IEnumerable<string> InlineTags => _inlineBindParsers.Keys.Where(tag => !tag.StartsWith("#"));
        public IEnumerable<string> BlockTags => _blockBindParsers.Keys.Where(tag => !tag.StartsWith("#"));

        public bool MaybeSupportBodyTag(string tagName)
            => _blockBindParsers.ContainsKey(tagName.ToLower());

        public bool MaybeSupportInlineTag(string tagName)
            => _inlineBindParsers.ContainsKey(tagName.ToLower());

        public UnknownTagsOption UnknownTags { get; set; }

        public IMarkdownEngine Engine { get; set; }

        public ICommand? HyperlinkCommand => Engine.HyperlinkCommand;

        public string? AssetPathRoot => Engine.AssetPathRoot;

        public void Register(ITagParser parser)
        {

            if (parser is IInlineTagParser inlineParser)
            {
                PrivateRegister(inlineParser, _inlineBindParsers);
            }
            if (parser is IBlockTagParser blockParser)
            {
                PrivateRegister(blockParser, _blockBindParsers);
            }

            PrivateRegister(parser, _bindParsers);

            static void PrivateRegister<T>(T parser, Dictionary<string, List<T>> bindParsers) where T : ITagParser
            {
                foreach (var tag in parser.SupportTag)
                {
                    if (!bindParsers.TryGetValue(tag.ToLower(), out var list))
                    {
                        list = new();
                        bindParsers.Add(tag.ToLower(), list);
                    }

                    int parserPriority = GetPriority(parser);

                    int i = 0;
                    int count = list.Count;
                    for (; i < count; ++i)
                        if (parserPriority <= GetPriority(list[i]))
                            break;

                    list.Insert(i, parser);
                }
            }

            static int GetPriority(object? p)
                => p is IHasPriority prop ? prop.Priority : HasPriority.DefaultPriority;
        }

        /// <summary>
        /// Convert a html tag list to an element of markdown.
        /// </summary>
        public IEnumerable<Control> Parse(string htmldoc)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmldoc);

            return Parse(doc);
        }

        /// <summary>
        /// Convert a html tag list to an element of markdown.
        /// </summary>
        public IEnumerable<Control> Parse(HtmlDocument doc)
        {
            var contents = new List<HtmlNode>();

            var head = PickBodyOrHead(doc.DocumentNode, "head");
            if (head is not null)
                contents.AddRange(head.ChildNodes.SkipComment());

            var body = PickBodyOrHead(doc.DocumentNode, "body");
            if (body is not null)
                contents.AddRange(body.ChildNodes.SkipComment());

            if (contents.Count == 0)
            {
                var root = doc.DocumentNode.ChildNodes.SkipComment();

                if (root.Count == 1 && string.Equals(root[0].Name, "html", StringComparison.OrdinalIgnoreCase))
                    contents.AddRange(root[0].ChildNodes.SkipComment());
                else
                    contents.AddRange(root);
            }

            var jaggingResult = ParseJagging(contents);

            return Grouping(jaggingResult);
        }

        /// <summary>
        /// Convert html tag children to an element of markdown.
        /// Inline elements are aggreated into paragraph.
        /// </summary>
        public IEnumerable<Control> ParseChildrenAndGroup(HtmlNode node)
        {
            var jaggingResult = ParseChildrenJagging(node);

            return Grouping(jaggingResult);
        }

        /// <summary>
        /// Convert html tag children to an element of markdown.
        /// this result contains a block element and an inline element.
        /// </summary>
        public IEnumerable<StyledElement> ParseChildrenJagging(HtmlNode node)
        {
            return ParseChildrenJagigng(node.ChildNodes);
        }

        public IEnumerable<StyledElement> ParseChildrenJagigng(IEnumerable<HtmlNode> nodes)
        {
            // search empty line
            var empNd = nodes.Select((nd, idx) => new { Node = nd, Index = idx })
                             .Where(tpl => tpl.Node is HtmlTextNode)
                             .Select(tpl => new
                             {
                                 NodeIndex = tpl.Index,
                                 TextIndex = tpl.Node.InnerText.IndexOf("\n\n")
                             })
                             .FirstOrDefault(tpl => tpl.TextIndex != -1);

            if (empNd is null)
            {
                return ParseJagging(nodes);
            }
            else
            {
                return ParseJaggingAndRunBlockGamut(nodes, empNd.NodeIndex, empNd.TextIndex);
            }
        }


        /// <summary>
        /// Convert a html tag to an element of markdown.
        /// this result contains a block element and an inline element.
        /// </summary>
        private IEnumerable<StyledElement> ParseJagging(IEnumerable<HtmlNode> nodes)
        {
            bool isPrevBlock = true;
            StyledElement? lastElement = null;

            foreach (var node in nodes)
            {
                if (node.IsComment())
                    continue;

                // remove blank text between the blocks.
                if (isPrevBlock
                    && node is HtmlTextNode txt
                    && String.IsNullOrWhiteSpace(txt.Text))
                    continue;

                foreach (var element in ParseBlockAndInline(node))
                {
                    lastElement = element;
                    yield return element;
                }

                isPrevBlock = lastElement is Control;
            }
        }

        private IEnumerable<StyledElement> ParseJaggingAndRunBlockGamut(IEnumerable<HtmlNode> nodes, int nodeIdx, int textIdx)
        {
            var parseTargets = new List<HtmlNode>();
            var textBuf = new StringBuilder();
            var mdTextBuf = new StringBuilder();

            foreach (var tpl in nodes.Select((value, i) => new { Node = value, Index = i }))
            {
                if (tpl.Index < nodeIdx)
                {
                    parseTargets.Add(tpl.Node);
                }
                else if (tpl.Index == nodeIdx)
                {
                    var nodeText = tpl.Node.InnerText;

                    textBuf.Append(nodeText.Substring(0, textIdx));
                    mdTextBuf.Append(nodeText.Substring(textIdx + 2));
                }
                else
                {
                    mdTextBuf.Append(tpl.Node.OuterHtml);
                }
            }

            foreach (var elm in ParseJagging(parseTargets))
                yield return elm;

            foreach (var elm in textParser.Replace(textBuf.ToString(), this))
                yield return elm;

            foreach (var elm in Engine.RunBlockGamut(mdTextBuf.ToString(), ParseStatus.Init))
                yield return elm;
        }

        /// <summary>
        /// Convert a html tag to an element of markdown.
        /// Only tag node and text node are accepted.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public IEnumerable<StyledElement> ParseBlockAndInline(HtmlNode node)
        {
            if (_bindParsers.TryGetValue(node.Name.ToLower(), out var binds))
            {
                foreach (var bind in binds)
                {
                    if (bind.TryReplace(node, this, out var parsed))
                    {
                        return parsed;
                    }
                }
            }

            return UnknownTags switch
            {
                UnknownTagsOption.PassThrough
                    => HtmlUtils.IsBlockTag(node.Name) ?
                        new[] { new CTextBlock(new CRun() { Text = node.OuterHtml }) } :
                        new[] { new CRun() { Text = node.OuterHtml } },

                UnknownTagsOption.Drop
                    => EnumerableExt.Empty<StyledElement>(),

                UnknownTagsOption.Bypass
                    => ParseJagging(node.ChildNodes),

                _ => throw new UnknownTagException(node)
            };
        }

        public IEnumerable<Control> ParseBlock(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            foreach (var node in doc.DocumentNode.ChildNodes)
                foreach (var block in ParseBlock(node))
                    yield return block;
        }

        public IEnumerable<CInline> ParseInline(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            foreach (var node in doc.DocumentNode.ChildNodes)
                foreach (var inline in ParseInline(node))
                    yield return inline;
        }

        public IEnumerable<Control> ParseBlock(HtmlNode node)
        {
            if (_blockBindParsers.TryGetValue(node.Name.ToLower(), out var binds))
            {
                foreach (var bind in binds)
                {
                    if (bind.TryReplace(node, this, out var parsed))
                    {
                        return parsed;
                    }
                }
            }

            return UnknownTags switch
            {
                UnknownTagsOption.PassThrough
                    => new[] {
                        new CTextBlock(new CRun() { Text = node.OuterHtml })
                    },

                UnknownTagsOption.Drop
                    => EnumerableExt.Empty<Control>(),

                UnknownTagsOption.Bypass
                    => node.ChildNodes
                           .SkipComment()
                           .SelectMany(nd => ParseBlock(nd)),

                _ => throw new UnknownTagException(node)
            };
        }

        public IEnumerable<CInline> ParseInline(HtmlNode node)
        {
            if (_inlineBindParsers.TryGetValue(node.Name.ToLower(), out var binds))
            {
                foreach (var bind in binds)
                {
                    if (bind.TryReplace(node, this, out var parsed))
                    {
                        return parsed;
                    }
                }
            }

            return UnknownTags switch
            {
                UnknownTagsOption.PassThrough
                    => new[] { new CRun() { Text = node.OuterHtml } },

                UnknownTagsOption.Drop
                    => EnumerableExt.Empty<CInline>(),

                UnknownTagsOption.Bypass
                    => node.ChildNodes
                           .SkipComment()
                           .SelectMany(nd => ParseInline(nd)),

                _ => throw new UnknownTagException(node)
            };
        }

        /// <summary>
        /// Convert IMdElement to IMdBlock.
        /// Inline elements are aggreated into paragraph.
        /// </summary>
        public IEnumerable<Control> Grouping(IEnumerable<StyledElement> elements)
        {
            static CTextBlock? Group(IList<CInline> inlines)
            {
                // trim whiltepace plain

                while (inlines.Count > 0)
                {
                    if (inlines[0] is CRun run
                        && String.IsNullOrWhiteSpace(run.Text))
                    {
                        inlines.RemoveAt(0);
                    }
                    else break;
                }

                while (inlines.Count > 0)
                {
                    if (inlines[inlines.Count - 1] is CRun run
                        && String.IsNullOrWhiteSpace(run.Text))
                    {
                        inlines.RemoveAt(inlines.Count - 1);
                    }
                    else break;
                }

                using (var list = inlines.GetEnumerator())
                {
                    CInline? prev = null;

                    if (list.MoveNext())
                    {
                        prev = list.Current;
                        DocUtils.TrimStart(prev);

                        while (list.MoveNext())
                        {
                            var now = list.Current;

                            if (now is CLineBreak)
                            {
                                DocUtils.TrimEnd(prev);

                                if (list.MoveNext())
                                {
                                    now = list.Current;
                                    DocUtils.TrimStart(now);
                                }
                            }

                            prev = now;
                        }
                    }

                    if (prev is not null)
                        DocUtils.TrimEnd(prev);
                }

                if (inlines.Count > 0)
                {
                    var para = new CTextBlock();
                    para.Content.AddRange(inlines);
                    return para;
                }
                return null;
            }

            List<CInline> stored = new();
            foreach (var e in elements)
            {
                if (e is CInline inline)
                {
                    stored.Add(inline);
                    continue;
                }

                // grouping inlines
                if (stored.Count != 0)
                {
                    var para = Group(stored);
                    if (para is not null) yield return para;
                    stored.Clear();
                }

                yield return (Control)e;
            }

            if (stored.Count != 0)
            {
                var para = Group(stored);
                if (para is not null) yield return para;
                stored.Clear();
            }
        }

        private static HtmlNode? PickBodyOrHead(HtmlNode documentNode, string headOrBody)
        {
            // html?
            foreach (var child in documentNode.ChildNodes)
            {
                if (child.Name == HtmlNode.HtmlNodeTypeNameText
                    || child.Name == HtmlNode.HtmlNodeTypeNameComment)
                    continue;

                switch (child.Name.ToLower())
                {
                    case "html":
                        // body? head?
                        foreach (var descendants in child.ChildNodes)
                        {
                            if (descendants.Name == HtmlNode.HtmlNodeTypeNameText
                                || descendants.Name == HtmlNode.HtmlNodeTypeNameComment)
                                continue;
                            switch (descendants.Name.ToLower())
                            {
                                case "head":
                                    if (headOrBody == "head")
                                        return descendants;
                                    break;

                                case "body":
                                    if (headOrBody == "body")
                                        return descendants;
                                    break;

                                default:
                                    return null;
                            }
                        }
                        break;

                    case "head":
                        if (headOrBody == "head")
                            return child;
                        break;

                    case "body":
                        if (headOrBody == "body")
                            return child;
                        break;

                    default:
                        return null;
                }
            }
            return null;
        }
    }
}
