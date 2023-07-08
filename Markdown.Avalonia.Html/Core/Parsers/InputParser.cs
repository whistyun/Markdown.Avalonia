using Avalonia;
using Avalonia.Controls;
using ColorTextBlock.Avalonia;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;

namespace Markdown.Avalonia.Html.Core.Parsers
{
    public class InputParser : IInlineTagParser
    {
        public IEnumerable<string> SupportTag => new[] { "input" };

        bool ITagParser.TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<StyledElement> generated)
        {
            var rtn = TryReplace(node, manager, out var list);
            generated = list;
            return rtn;
        }

        public bool TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<CInline> generated)
        {
            var type = node.Attributes["type"]?.Value ?? "text";

            double? width = null;
            var widAttr = node.Attributes["width"];
            var sizAttr = node.Attributes["size"];

            if (widAttr is not null)
            {
                if (double.TryParse(widAttr.Value, out var v))
                    width = v;
            }
            if (sizAttr is not null)
            {
                if (int.TryParse(sizAttr.Value, out var v))
                    width = v * 7;
            }

            CInlineUIContainer inline;
            switch (type)
            {
                default:
                case "text":
                    var txt = new TextBox()
                    {
                        Text = node.Attributes["value"]?.Value ?? "",
                        IsReadOnly = true,
                    };
                    if (width.HasValue) txt.Width = width.Value;
                    else if (String.IsNullOrEmpty(txt.Text)) txt.Width = 100;


                    inline = new CInlineUIContainer(txt);
                    break;


                case "button":
                case "reset":
                case "submit":
                    var btn = new Button()
                    {
                        Content = node.Attributes["value"]?.Value ?? "",
                        IsEnabled = false,
                    };
                    if (width.HasValue) btn.Width = width.Value;
                    else if (String.IsNullOrEmpty(btn.Content.ToString())) btn.Width = 100;

                    inline = new CInlineUIContainer(btn);
                    break;


                case "radio":
                    var radio = new RadioButton()
                    {
                        IsEnabled = false,
                    };
                    if (node.Attributes["checked"] != null) radio.IsChecked = true;

                    inline = new CInlineUIContainer(radio);
                    break;


                case "checkbox":
                    var chk = new CheckBox()
                    {
                        IsEnabled = false
                    };
                    if (node.Attributes["checked"] != null)
                        chk.IsChecked = true;

                    inline = new CInlineUIContainer(chk);
                    break;


                case "range":
                    var slider = new Slider()
                    {
                        IsEnabled = false,
                        Minimum = 0,
                        Value = 50,
                        Maximum = 100,
                        Width = 100,
                    };

                    var minAttr = node.Attributes["min"];
                    if (minAttr is not null && double.TryParse(minAttr.Value, out var minVal))
                    {
                        slider.Minimum = minVal;
                    }

                    var maxAttr = node.Attributes["max"];
                    if (maxAttr is not null && double.TryParse(maxAttr.Value, out var maxVal))
                    {
                        slider.Maximum = maxVal;
                    }

                    var valAttr = node.Attributes["value"];
                    if (valAttr is not null && double.TryParse(valAttr.Value, out var val))
                    {
                        slider.Value = val;
                    }
                    else
                    {
                        slider.Value = (slider.Minimum + slider.Maximum) / 2;
                    }

                    inline = new CInlineUIContainer(slider);
                    break;
            }

            generated = new[] { inline };
            return true;
        }
    }
}
