using Avalonia.Media;
using Avalonia.Layout;

namespace Markdown.Avalonia.Tables
{
    interface ITableCell
    {
        int ColumnIndex { get; }

        string? RawText { get; }
        string? Text { get; }
        int RowSpan { get; }
        int ColSpan { get; }
        TextAlignment? Horizontal { get; }
        VerticalAlignment? Vertical { get; }
    }
}
