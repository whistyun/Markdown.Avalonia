using System.Collections.Generic;

namespace Markdown.Avalonia.Tables
{
    interface ITable
    {
        List<ITableCell> Header { get; }
        List<List<ITableCell>> Details { get; }
        int ColCount { get; }
        int RowCount { get; }
    }
}

