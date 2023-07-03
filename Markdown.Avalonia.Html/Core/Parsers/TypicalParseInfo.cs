using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Layout;
using ColorTextBlock.Avalonia;
using HtmlAgilityPack;
using Markdonw.Avalonia.Html.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Markdonw.Avalonia.Html.Core.Parsers
{
    public class TypicalParseInfo
    {
        public string HtmlTag { get; }
        public string FlowDocumentTagText { get; }
        public Type? FlowDocumentTag { get; }
        public string? TagNameReference { get; }
        public Tags TagName { get; }
        public string? ExtraModifyName { get; }

        private readonly MethodInfo? _method;

        public TypicalParseInfo(string[] line)
        {
            FlowDocumentTagText = line[1];

            if (FlowDocumentTagText.StartsWith("#"))
            {
                FlowDocumentTag = null;
            }
            else
            {
                Type? elementType = AppDomain.CurrentDomain
                                             .GetAssemblies()
                                             .Select(asm => asm.GetType(FlowDocumentTagText))
                                             .OfType<Type>()
                                             .FirstOrDefault();

                if (elementType is null)
                    throw new ArgumentException($"Failed to load type '{line[1]}'");

                FlowDocumentTag = elementType;
            }


            HtmlTag = line[0];
            TagNameReference = GetArrayAt(line, 2);
            ExtraModifyName = GetArrayAt(line, 3);

            if (TagNameReference is not null)
            {
                TagName = (Tags)Enum.Parse(typeof(Tags), TagNameReference);
            }

            if (ExtraModifyName is not null)
            {
                _method = this.GetType().GetMethod("ExtraModify" + ExtraModifyName);

                if (_method is null)
                    throw new InvalidOperationException("unknown method ExtraModify" + ExtraModifyName);
            }

            static string? GetArrayAt(string[] array, int idx)
            {
                if (idx < array.Length
                    && !string.IsNullOrWhiteSpace(array[idx]))
                {
                    return array[idx];
                }
                return null;
            }
        }

        public bool TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<StyledElement> generated)
        {
            // create instance

            if (FlowDocumentTag is null)
            {
                switch (FlowDocumentTagText)
                {
                    case "#blocks":
                        generated = manager.ParseChildrenAndGroup(node).ToArray();
                        break;

                    case "#jagging":
                        generated = manager.ParseChildrenJagging(node).ToArray();
                        break;

                    case "#inlines":
                        if (manager.ParseChildrenJagging(node).TryCast<CInline>(out var inlines))
                        {
                            generated = inlines.ToArray();
                            break;
                        }
                        else
                        {
                            generated = EnumerableExt.Empty<StyledElement>();
                            return false;
                        }

                    default:
                        throw new InvalidOperationException();
                }

                // for href anchor
                //if (node.Attributes["id"]?.Value is string idval
                //    && generated.FirstOrDefault() is AvaloniaObject tag)
                //{
                //    tag.SetValue(DocumentAnchor.HyperlinkAnchorProperty, idval);
                //}
            }
            else
            {
                var tag = (StyledElement)Activator.CreateInstance(FlowDocumentTag)!;

                // for href anchor
                //if (node.Attributes["id"]?.Value is string idval)
                //{
                //    tag.SetValue(DocumentAnchor.HyperlinkAnchorProperty, idval);
                //}

                if (tag is StackPanel pnl)
                {
                    pnl.Orientation = Orientation.Vertical;
                    var parseResult = manager.ParseChildrenAndGroup(node).ToArray();
                    foreach (var ctrl in parseResult)
                        pnl.Children.Add(ctrl);
                }
                else if (tag is CTextBlock textbox)
                {
                    var parseResult = manager.ParseChildrenJagging(node).ToArray();

                    if (parseResult.TryCast<CInline>(out var parsed))
                    {
                        textbox.Content.AddRange(parsed);
                    }
                    else if (parseResult.Length == 1 && parseResult[0] is CTextBlock)
                    {
                        tag = parseResult[0];
                    }
                    else
                    {
                        generated = EnumerableExt.Empty<StyledElement>();
                        return false;
                    }
                }
                else if (tag is CSpan span)
                {
                    var content = (AvaloniaList<CInline>)span.Content;

                    var parseResult = manager.ParseChildrenJagging(node).ToArray();

                    if (parseResult.TryCast<CInline>(out var parsed))
                    {
                        content.AddRange(parsed);
                    }
                    else if (tag is CSpan && manager.Grouping(parseResult).TryCast<CTextBlock>(out var paragraphs))
                    {
                        // FIXME: Markdonw.Avalonia can't bubbling a block element in a inline element.
                        foreach (var para in paragraphs)
                            foreach (var inline in para.Content.ToArray())
                                content.Add(inline);
                    }
                    else
                    {
                        generated = EnumerableExt.Empty<StyledElement>();
                        return false;
                    }
                }
                else if (tag is not CLineBreak)
                {
                    throw new InvalidOperationException();
                }

                generated = new[] { tag };
            }

            // apply tag

            if (TagNameReference is not null)
            {
                foreach (var tag in generated)
                {
                    tag.Classes.Add(TagName.GetClass());
                }
            }

            // extra modify
            if (_method is not null)
            {
                foreach (var tag in generated)
                {
                    _method.Invoke(this, new object[] { tag, node, manager });
                }
            }

            return true;
        }


        public void ExtraModifyHyperlink(CHyperlink link, HtmlNode node, ReplaceManager manager)
        {
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

        public void ExtraModifyStrikethrough(CSpan span, HtmlNode node, ReplaceManager manager)
        {
            span.IsStrikethrough = true;
        }

        public void ExtraModifySubscript(CSpan span, HtmlNode node, ReplaceManager manager)
        {
            // TODO implements Subscript
            //Typography.SetVariants(span, FontVariants.Subscript);
        }

        public void ExtraModifySuperscript(CSpan span, HtmlNode node, ReplaceManager manager)
        {
            // TODO implements Superscript
            //Typography.SetVariants(span, FontVariants.Superscript);
        }

        public void ExtraModifyAcronym(CSpan span, HtmlNode node, ReplaceManager manager)
        {
            // TODO implements Acronym
            //var title = node.Attributes["title"]?.Value;
            //if (title is not null)
            //    span.ToolTip = title;
        }

        public void ExtraModifyCenter(StackPanel center, HtmlNode node, ReplaceManager manager)
        {
            center.HorizontalAlignment = HorizontalAlignment.Center;

            foreach (var child in center.Children)
            {
                if (child is CTextBlock cbox)
                {
                    cbox.HorizontalAlignment = HorizontalAlignment.Center;
                }
            }
        }

        public static IEnumerable<TypicalParseInfo> Load(string resourcePath)
        {
            var asm = Assembly.GetExecutingAssembly();
            using var stream = asm.GetManifestResourceStream(resourcePath);

            if (stream is null)
                throw new ArgumentException($"resource not found: '{resourcePath}'");

            using var reader = new StreamReader(stream!);
            while (reader.ReadLine() is string line)
            {
                if (line.StartsWith("#")) continue;

                var elements = line.Split('|').Select(t => t.Trim()).ToArray();
                yield return new TypicalParseInfo(elements);
            }
        }
    }
}
