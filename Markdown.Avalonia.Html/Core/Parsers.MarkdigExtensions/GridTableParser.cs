using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Markdown.Avalonia.Html.Core.Utils;
using Avalonia;
using Avalonia.Controls;
using Markdown.Avalonia.Html.Tables;
using Avalonia.Layout;

namespace Markdown.Avalonia.Html.Core.Parsers.MarkdigExtensions
{
    public class GridTableParser : IBlockTagParser, IHasPriority
    {
        public int Priority => HasPriority.DefaultPriority + 1000;

        public IEnumerable<string> SupportTag => new[] { "table" };

        bool ITagParser.TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<StyledElement> generated)
        {
            var rtn = TryReplace(node, manager, out var list);
            generated = list;
            return rtn;
        }

        public bool TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<Control> generated)
        {
            var table = new Table();

            var theadRows = node.SelectNodes("./thead/tr");
            if (theadRows is not null)
            {
                var group = CreateRowGroup(theadRows, manager);

                SetupClass(group, Tags.TagTableHeader.GetClass());

                table.RowGroups.AddRange(group);
            }

            var tbodyRows = new List<HtmlNode>();
            foreach (var child in node.ChildNodes)
            {
                if (string.Equals(child.Name, "tr", StringComparison.OrdinalIgnoreCase))
                    tbodyRows.Add(child);

                if (string.Equals(child.Name, "tbody", StringComparison.OrdinalIgnoreCase))
                    tbodyRows.AddRange(child.ChildNodes.CollectTag("tr"));
            }
            if (tbodyRows.Count > 0)
            {
                var group = CreateRowGroup(tbodyRows, manager);

                table.RowGroups.AddRange(group);

                int idx = 0;
                foreach (var row in group)
                {
                    var useTag = (++idx & 1) == 0 ? Tags.TagEvenTableRow : Tags.TagOddTableRow;
                    var clsNm = useTag.GetClass();

                    foreach (var cell in row)
                        cell.Content.Classes.Add(clsNm);
                }
            }

            var tfootRows = node.SelectNodes("./tfoot/tr");
            if (tfootRows is not null)
            {
                var group = CreateRowGroup(tfootRows, manager);

                SetupClass(group, Tags.TagTableFooter.GetClass());

                table.RowGroups.AddRange(group);
            }

            ParseColumnStyle(node, table);

            table.Structure();

            // table
            var grid = new Grid();

            // table columns
            grid.ColumnDefinitions = new AutoScaleColumnDefinitions(table.ColumnLengths, grid);

            // table rows
            int rowIdx = 0;
            foreach (var row in table.RowGroups)
            {
                grid.RowDefinitions.Add(new RowDefinition());

                foreach (var cell in row)
                {
                    grid.Children.Add(cell.Content);
                    Grid.SetRow(cell.Content, rowIdx);
                    Grid.SetColumn(cell.Content, cell.ColumnIndex);
                    Grid.SetRowSpan(cell.Content, cell.RowSpan);
                    Grid.SetColumnSpan(cell.Content, cell.ColSpan);
                }
                ++rowIdx;
            }

            grid.Classes.Add(global::Markdown.Avalonia.Markdown.TableClass);

            var border = new Border();
            border.Child = grid;
            border.Classes.Add(global::Markdown.Avalonia.Markdown.TableClass);

            if (table.ColumnLengths.All(l => l.Unit != LengthUnit.Percent))
            {
                border.Classes.Add("TightTable");
            }

            var captions = node.SelectNodes("./caption");
            if (captions is not null)
            {
                var section = new DockPanel() { LastChildFill = true };

                foreach (var captionNode in captions)
                {
                    foreach (var caption in manager.ParseChildrenAndGroup(captionNode))
                    {
                        DockPanel.SetDock(caption, Dock.Top);
                        caption.Classes.Add(Tags.TagTableCaption.GetClass());
                        section.Children.Add(caption);
                    }
                }

                section.Children.Add(border);

                generated = new[] { section };
            }
            else
            {
                generated = new[] { border };
            }

            return true;
        }

        private static void SetupClass(List<List<TableCell>> group, string cls)
        {
            foreach (var row in group)
                foreach (var cell in row)
                    cell.Content.Classes.Add(cls);
        }


        private static void ParseColumnStyle(HtmlNode tableTag, Table table)
        {
            var colHolder = tableTag.ChildNodes.HasOneTag("colgroup", out var colgroup) ? colgroup! : tableTag;

            foreach (var col in colHolder.ChildNodes.CollectTag("col"))
            {

                var styleAttr = col.Attributes["style"];
                if (styleAttr is null) continue;

                var mch = Regex.Match(styleAttr.Value, "width[ \t]*:[ \t]*([^;\"]+)(%|em|ex|mm|cm|in|pt|pc|)");
                if (!mch.Success) continue;

                if (!Length.TryParse(mch.Groups[1].Value + mch.Groups[2].Value, out var length))
                    continue;

                table.ColumnLengths.Add(length.Unit switch
                {
                    Unit.Percentage => new LengthInfo(length.Value, LengthUnit.Percent),
                    _ => new LengthInfo(length.ToPoint(), LengthUnit.Pixel)
                });
            }
        }


        private static List<List<TableCell>> CreateRowGroup(
            IEnumerable<HtmlNode> rows,
            ReplaceManager manager)
        {
            var group = new List<List<TableCell>>();

            foreach (var rowTag in rows)
            {
                var row = new List<TableCell>();

                foreach (var cellTag in rowTag.ChildNodes.CollectTag("td", "th"))
                {
                    var cell = new TableCell(manager.ParseChildrenAndGroup(cellTag));

                    int colspan = TryParse(cellTag.Attributes["colspan"]?.Value);
                    int rowspan = TryParse(cellTag.Attributes["rowspan"]?.Value);

                    cell.RowSpan = rowspan;
                    cell.ColSpan = colspan;

                    row.Add(cell);
                }

                group.Add(row);
            }

            return group;

            static int TryParse(string? txt) => int.TryParse(txt, out var v) ? v : 1;
        }
    }
}
