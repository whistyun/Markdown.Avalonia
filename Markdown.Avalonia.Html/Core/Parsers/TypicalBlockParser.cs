using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using ColorDocument.Avalonia;
using ColorDocument.Avalonia.DocumentElements;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Markdown.Avalonia.Html.Core.Parsers
{
    public class TypicalBlockParser : IBlockTagParser
    {
        private const string _resource = "Markdown.Avalonia.Html.Core.Parsers.TypicalBlockParser.tsv";
        private TypicalBlockInfo _parser;

        public IEnumerable<string> SupportTag => new[] { _parser.HtmlTagName };

        private TypicalBlockParser(TypicalBlockInfo parser)
        {
            _parser = parser;
        }

        public bool TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<DocumentElement> generated)
        {
            return _parser.TryReplaceBlocks(node, manager, out generated);
        }

        public static IEnumerable<TypicalBlockParser> Load()
        {
            foreach (var info in TypicalBlockInfo.Load(_resource))
            {
                yield return new TypicalBlockParser(info);
            }
        }
    }

    class TypicalBlockInfo
    {
        public string HtmlTagName { get; }
        public string FlowDocumentTagName { get; }
        public string? ElementTag { get; }
        public Action<DocumentElement, HtmlNode, ReplaceManager>? ExtraModifyAction { get; }

        private TypicalBlockInfo(
            string htmlTagName,
            string flowDocumentTagName,
            Tags? tagName,
            string extraModifyName)
        {
            HtmlTagName = htmlTagName;
            FlowDocumentTagName = flowDocumentTagName;
            ElementTag = tagName.HasValue ? tagName.Value.GetClass() : null;

            ExtraModifyAction = ("ExtraModify" + extraModifyName) switch
            {
                nameof(ExtraModifyCenter) => ExtraModifyCenter,
                _ => null
            };
        }

        public bool TryReplaceBlocks(HtmlNode node, ReplaceManager manager, out IEnumerable<DocumentElement> generated)
        {
            switch (FlowDocumentTagName)
            {
                case "#border":
                    var bdr = new BorderedDocumentGroupElement(manager.ParseChildNodes(node));
                    if (ElementTag is not null)
                        bdr.Classes.Add(ElementTag);

                    if (ExtraModifyAction is not null)
                    {
                        ExtraModifyAction(bdr, node, manager);
                    }

                    generated = new[] { bdr };
                    break;


                case "#blocks":
                    generated = manager.ParseChildNodes(node);

                    if (ElementTag is not null)
                    {
                        generated = generated.Select(tag =>
                        {
                            tag.Classes.Add(ElementTag);
                            return tag;
                        });
                    }
                    break;

                default:
                    throw new InvalidOperationException();
            }

            return true;
        }

        public static void ExtraModifyCenter(DocumentElement element, HtmlNode node, ReplaceManager manager)
        {
            var center = (BorderedDocumentGroupElement)element;
            center.HorizontalAlignment = HorizontalAlignment.Center;

            //foreach (var child in ((StackPanel)center.Child!).Children)
            //{
            //    if (child is CTextBlock cbox)
            //    {
            //        cbox.HorizontalAlignment = HorizontalAlignment.Center;
            //    }
            //}
        }

        internal static IEnumerable<TypicalBlockInfo> Load(string resourcePath)
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

                Tags? tagName = string.IsNullOrEmpty(tagNameText) ? null :
                               (Tags)Enum.Parse(typeof(Tags), tagNameText);

                yield return new TypicalBlockInfo(htmlTagName, fdocTypeName, tagName, extraModify);
            }
        }
    }
}
