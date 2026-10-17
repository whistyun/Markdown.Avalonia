using Avalonia;
using Avalonia.Collections;
using ColorTextBlock.Avalonia;
using HtmlAgilityPack;
using Markdown.Avalonia.Html.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Markdown.Avalonia.Html.Core.Parsers
{
    public class TypicalInlineParser : IInlineTagParser
    {
        private const string _resource = "Markdown.Avalonia.Html.Core.Parsers.TypicalInlineParser.tsv";
        private readonly TypicalInlineInfo _parser;

        public IEnumerable<string> SupportTag => new[] { _parser.HtmlTagName };

        private TypicalInlineParser(TypicalInlineInfo parser)
        {
            _parser = parser;
        }

        public bool TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<CInline> generated)
        {
            var rtn = _parser.TryReplaceInlines(node, manager, out var list);
            generated = list.Cast<CInline>();
            return rtn;
        }

        public static IEnumerable<TypicalInlineParser> Load()
        {
            foreach (var info in TypicalInlineInfo.Load(_resource))
            {
                yield return new TypicalInlineParser(info);
            }
        }
    }

    class TypicalInlineInfo
    {
        public string HtmlTagName { get; }
        public Type FlowDocumentTagType { get; }
        public string? ElementTag { get; }
        public Action<CInline, HtmlNode, ReplaceManager>? ExtraModifyAction { get; }

        private TypicalInlineInfo(
            string htmlTagName,
            Type flowDocumentTagType,
            Tags? tagName,
            string extraModifyName)
        {
            HtmlTagName = htmlTagName;
            FlowDocumentTagType = flowDocumentTagType;
            ElementTag = tagName.HasValue ? tagName.Value.GetClass() : null;


            ExtraModifyAction = ("ExtraModify" + extraModifyName) switch
            {
                nameof(ExtraModifyHyperlink) => ExtraModifyHyperlink,
                nameof(ExtraModifyStrikethrough) => ExtraModifyStrikethrough,
                nameof(ExtraModifySubscript) => ExtraModifySubscript,
                nameof(ExtraModifySuperscript) => ExtraModifySuperscript,
                nameof(ExtraModifyAcronym) => ExtraModifyAcronym,
                _ => null
            };
        }

        public bool TryReplaceInlines(HtmlNode node, ReplaceManager manager, out IEnumerable<CInline> generated)
        {
            var tag = (CInline)Activator.CreateInstance(FlowDocumentTagType)!;

            // add content
            if (tag is CCode code)
            {
                var inlines = manager.ParseChildInlinesOnly(node);
                if (inlines.Count == 0)
                {
                    generated = EnumerableExt.Empty<CInline>();
                    return false;
                }

                var codecontent = (AvaloniaList<CInline>)code.Content;
                codecontent.AddRange(inlines);

            }
            else if (tag is CSpan span)
            {
                var inlines = manager.ParseChildInlinesOnly(node);
                if (inlines.Count == 0)
                {
                    generated = EnumerableExt.Empty<CInline>();
                    return false;
                }

                var content = (AvaloniaList<CInline>)span.Content;
                content.AddRange(inlines);
            }

            // apply tag
            if (ElementTag is not null)
            {
                tag.Classes.Add(ElementTag);
            }


            // extra modify
            if (ExtraModifyAction is not null)
            {
                ExtraModifyAction(tag, node, manager);
            }

            generated = new[] { tag };
            return true;
        }

        private static void ExtraModifyHyperlink(CInline inline, HtmlNode node, ReplaceManager manager)
        {
            var link = (CHyperlink)inline;
            var href = node.Attributes["href"]?.Value;

            if (href is not null)
            {
                link.CommandParameter = href;
                link.Command = (urlTxt) =>
                {
                    var command = manager.HyperlinkCommand;
                    if (command != null && command.CanExecute(urlTxt))
                    {
                        command.Execute(urlTxt);
                    }
                };
            }
        }

        private static void ExtraModifyStrikethrough(CInline inline, HtmlNode node, ReplaceManager manager)
        {
            var span = (CSpan)inline;
            span.IsStrikethrough = true;
        }

        private static void ExtraModifySubscript(CInline inline, HtmlNode node, ReplaceManager manager)
        {
            var span = (CSpan)inline;
            // TODO implements Subscript
            //Typography.SetVariants(span, FontVariants.Subscript);
        }

        private static void ExtraModifySuperscript(CInline inline, HtmlNode node, ReplaceManager manager)
        {
            var span = (CSpan)inline;
            // TODO implements Superscript
            //Typography.SetVariants(span, FontVariants.Superscript);
        }

        private static void ExtraModifyAcronym(CInline inline, HtmlNode node, ReplaceManager manager)
        {
            var span = (CSpan)inline;
            // TODO implements Acronym
            //var title = node.Attributes["title"]?.Value;
            //if (title is not null)
            //    span.ToolTip = title;
        }

        internal static IEnumerable<TypicalInlineInfo> Load(string resourcePath)
        {
            var asm = typeof(TypicalBlockParser).Assembly;
            using var stream = asm.GetManifestResourceStream(resourcePath);
            if (stream is null)
                throw new ArgumentException($"resource not found: '{resourcePath}'");

            using var reader = new StreamReader(stream);
            while (reader.ReadLine() is string line)
            {
                if (line.StartsWith("#")) continue;
                var record = line.Split('|').Select(t => t.Trim()).ToArray();

                var htmlTagName = record[0];
                var fdocTypeName = record[1];
                var tagNameText = record[2];
                var extraModify = record[3];

                Type fdocType = AppDomain.CurrentDomain
                                         .GetAssemblies()
                                         .Select(asm => asm.GetType(fdocTypeName))
                                         .OfType<Type>()
                                         .FirstOrDefault()
                                ?? throw new ArgumentException($"Failed to load type '{line[1]}'");

                Tags? tagName = string.IsNullOrEmpty(tagNameText) ? null :
                               (Tags)Enum.Parse(typeof(Tags), tagNameText);

                yield return new TypicalInlineInfo(htmlTagName, fdocType, tagName, extraModify);
            }
        }
    }
}
