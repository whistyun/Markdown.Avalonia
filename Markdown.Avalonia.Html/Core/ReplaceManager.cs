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
using ColorDocument.Avalonia;
using ColorDocument.Avalonia.DocumentElements;
using ColorTextBlock.Avalonia;
using Markdown.Avalonia.Parsers;
using Markdown.Avalonia.Plugins;
using Markdown.Avalonia.SyntaxHigh;

namespace Markdown.Avalonia.Html.Core
{
    public class ReplaceManagerSyntax
    {
        private readonly Dictionary<string, List<IInlineTagParser>> _inlineBindParsers;
        private readonly Dictionary<string, List<IBlockTagParser>> _blockBindParsers;
        private readonly bool _inlineMode;
        private TextNodeParser _textParser;
        public UnknownTagsOption UnknownTags { get; set; }

        public IEnumerable<string> InlineTags => _inlineBindParsers.Keys.Where(tag => !tag.StartsWith("#"));
        public IEnumerable<string> BlockTags => _blockBindParsers.Keys.Where(tag => !tag.StartsWith("#"));

        public ReplaceManagerSyntax(SyntaxHighlight highlight, SetupInfo info, bool inlineMode)
        {
            _inlineBindParsers = new(StringComparer.OrdinalIgnoreCase);
            _blockBindParsers = new(StringComparer.OrdinalIgnoreCase);
            _inlineMode = inlineMode;

            UnknownTags = UnknownTagsOption.Drop;

            Register(new TagIgnoreParser());
            Register(new CommentParsre());
            Register(new ImageParser(info));
            Register(new CodeBlockParser(highlight));
            //Register(new CodeSpanParser());
            Register(new OrderListParser());
            Register(new UnorderListParser());
            Register(_textParser = new TextNodeParser());
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

        public void Register(ITagParserBase parser)
        {

            if (parser is IInlineTagParser inlineParser)
            {
                PrivateRegister(inlineParser, _inlineBindParsers);
            }
            if (parser is IBlockTagParser blockParser)
            {
                PrivateRegister(blockParser, _blockBindParsers);
            }

            if (parser is not IInlineTagParser && parser is not IBlockTagParser)
            {
                throw new ArgumentException("Parser must be IInlineTagParser or IBlockTagParser");
            }

            static void PrivateRegister<T>(T parser, Dictionary<string, List<T>> bindParsers) where T : ITagParserBase
            {
                foreach (var tag in parser.SupportTag)
                {
                    if (!bindParsers.TryGetValue(tag, out var list))
                    {
                        list = new();
                        bindParsers.Add(tag, list);
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


        public ReplaceManager Create(IMarkdownEngine2 engine)
            => new ReplaceManager(
                _inlineBindParsers,
                _blockBindParsers,
                _inlineMode,
                _textParser,
                UnknownTags,
                engine);
    }

    public class ReplaceManager
    {
        private readonly Dictionary<string, List<IInlineTagParser>> _inlineBindParsers;
        private readonly Dictionary<string, List<IBlockTagParser>> _blockBindParsers;
        private readonly bool _inlineMode;
        private TextNodeParser _textParser;

        public ReplaceManager(
            Dictionary<string, List<IInlineTagParser>> inlineBindParsers,
            Dictionary<string, List<IBlockTagParser>> blockBindParsers,
            bool inlineMode,
            TextNodeParser textParser,
            UnknownTagsOption unknownTags,
            IMarkdownEngine2 engine)
        {
            _inlineBindParsers = inlineBindParsers;
            _blockBindParsers = blockBindParsers;
            _inlineMode = inlineMode;
            _textParser = textParser;
            UnknownTags = unknownTags;
            Engine = engine;
        }



        #region Properties

        public IEnumerable<string> InlineTags => _inlineBindParsers.Keys.Where(tag => !tag.StartsWith("#"));

        public IEnumerable<string> BlockTags => _blockBindParsers.Keys.Where(tag => !tag.StartsWith("#"));

        public UnknownTagsOption UnknownTags { get; }

        public IMarkdownEngine2 Engine { get; }

        public ICommand? HyperlinkCommand => Engine.HyperlinkCommand;

        public string? AssetPathRoot => Engine.AssetPathRoot;

        #endregion


        #region Supported tags

        public bool MaybeSupportBodyTag(string tagName)
            => _blockBindParsers.ContainsKey(tagName);

        public bool MaybeSupportInlineTag(string tagName)
            => _inlineBindParsers.ContainsKey(tagName);


        #endregion


        /// <summary>
        /// Convert a html tag list to an element of markdown.
        /// </summary>
        public IEnumerable<DocumentElement> Parse(string htmldoc)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmldoc);

            return Parse(doc);
        }

        private IEnumerable<DocumentElement> Parse(HtmlDocument doc)
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

            return Parse(contents);

            static HtmlNode? PickBodyOrHead(HtmlNode documentNode, string headOrBody)
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
                            return PickBodyOrHead(child, headOrBody);

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

        public IEnumerable<DocumentElement> Parse(HtmlNode node)
            => Grouping(CoreParseMixed(node));

        public IEnumerable<DocumentElement> Parse(IEnumerable<HtmlNode> node)
        {
            var jaggingResult = ParseChildrenMixedCore(node);
            return Grouping(jaggingResult);
        }

        public IEnumerable<CInline> ParseInline(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            foreach (var node in doc.DocumentNode.ChildNodes)
                foreach (var inline in CoreParseInline(node))
                    yield return inline;
        }

        /// <summary>
        /// Convert html tag children to an element of markdown.
        /// Inline elements are aggreated into paragraph.
        /// </summary>
        public IEnumerable<DocumentElement> ParseChildNodes(HtmlNode node)
            => Parse(node.ChildNodes);


        public List<CInline> ParseChildInlinesOnly(HtmlNode node)
        {
            var inlines = new List<CInline>();
            foreach (var nd in node.ChildNodes)
            {
                if (nd.IsComment())
                    continue;

                if (_inlineMode && nd is HtmlTextNode textNode)
                {
                    inlines.AddRange(Engine.ParseGamutInline(textNode.Text));
                }
                else if (_blockBindParsers.ContainsKey(nd.Name))
                {
                    if (inlines.Count == 0
                     && CoreParseBlock(nd).FirstOrDefault() is CTextBlockElement simpleblock)
                    {
                        inlines.AddRange(simpleblock.Inlines);
                    }
                    else
                    {
                        return new List<CInline>();
                    }
                }
                else
                {
                    inlines.AddRange(CoreParseInline(nd));
                }
            }

            return inlines;
        }


        /// <summary>
        /// Convert html tag children to mixed elements.
        /// The result contains block elements and inline elements.
        /// </summary>
        //private IEnumerable<object> ParseChildrenMixed(HtmlNode node)
        //{
        //    return ParseChildrenMixed(node.ChildNodes);
        //}


        private IEnumerable<object> ParseChildrenMixedCore(IEnumerable<HtmlNode> nodes)
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
        private IEnumerable<object> ParseJagging(IEnumerable<HtmlNode> nodes)
        {
            bool isPrevBlock = true;
            object? lastElement = null;

            foreach (var node in nodes)
            {
                if (node.IsComment())
                    continue;

                // remove blank text between the blocks.
                if (isPrevBlock
                    && node is HtmlTextNode txt
                    && String.IsNullOrWhiteSpace(txt.Text))
                    continue;

                foreach (var element in CoreParseMixed(node))
                {
                    lastElement = element;
                    yield return element;
                }

                isPrevBlock = lastElement is DocumentElement;
            }
        }

        private IEnumerable<object> ParseJaggingAndRunBlockGamut(IEnumerable<HtmlNode> nodes, int nodeIdx, int textIdx)
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

            foreach (var elm in _textParser.Replace(textBuf.ToString(), this))
                yield return elm;

            foreach (var elm in Engine.ParseGamutElement(mdTextBuf.ToString(), ParseStatus.Init))
                yield return elm;
        }

        /// <summary>
        /// Convert a html tag to an element of markdown.
        /// Only tag node and text node are accepted.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private IEnumerable<object> CoreParseMixed(HtmlNode node)
        {
            if (_blockBindParsers.TryGetValue(node.Name, out var blockBinds))
            {
                foreach (var bind in blockBinds)
                {
                    if (bind.TryReplace(node, this, out var parsed))
                    {
                        return parsed;
                    }
                }
            }

            if (_inlineBindParsers.TryGetValue(node.Name, out var inlineBinds))
            {
                foreach (var bind in inlineBinds)
                {
                    if (bind.TryReplace(node, this, out var parsed))
                    {
                        return parsed.Cast<object>();
                    }
                }
            }

            return UnknownTags switch
            {
                UnknownTagsOption.PassThrough
                    => HtmlUtils.IsBlockTag(node.Name) ?
                        new object[] { new UnBlockElement(new CTextBlock(new CRun() { Text = node.OuterHtml })) } :
                        new object[] { new CRun() { Text = node.OuterHtml } },

                UnknownTagsOption.Drop
                    => EnumerableExt.Empty<object>(),

                UnknownTagsOption.Bypass
                    => ParseJagging(node.ChildNodes),

                _ => throw new UnknownTagException(node)
            };
        }

        private IEnumerable<DocumentElement> CoreParseBlock(HtmlNode node)
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
                    => new DocumentElement[] {
                        new UnBlockElement(new CTextBlock(new CRun() { Text = node.OuterHtml }))
                    },

                UnknownTagsOption.Drop
                    => EnumerableExt.Empty<DocumentElement>(),

                UnknownTagsOption.Bypass
                    => node.ChildNodes
                           .SkipComment()
                           .SelectMany(nd => CoreParseBlock(nd)),

                _ => throw new UnknownTagException(node)
            };
        }

        private IEnumerable<CInline> CoreParseInline(HtmlNode node)
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
                           .SelectMany(nd => CoreParseInline(nd)),

                _ => throw new UnknownTagException(node)
            };
        }

        /// <summary>
        /// Convert Inline list to CTextBlockElement.
        /// </summary>
        private IEnumerable<DocumentElement> Grouping(IEnumerable<object> elements)
        {
            static CTextBlockElement? Group(IList<CInline> inlines)
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
                    return new CTextBlockElement(inlines.ToArray());
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

                if (e is DocumentElement element)
                {
                    yield return element;
                }
            }

            if (stored.Count != 0)
            {
                var para = Group(stored);
                if (para is not null) yield return para;
                stored.Clear();
            }
        }
    }
}
