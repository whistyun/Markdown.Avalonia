namespace Markdown.Avalonia.StyleCollections
{
    internal interface INamedStyle
    {
        string Name { get; }
        bool IsEditted { get; set; }
    }
}
