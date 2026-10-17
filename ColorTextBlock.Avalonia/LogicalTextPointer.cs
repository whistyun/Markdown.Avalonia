namespace ColorTextBlock.Avalonia
{
    public sealed class LogicalTextPointer : TextPointer
    {
        public static LogicalTextPointer Home { get; } = new(Kind.Home);
        public static LogicalTextPointer End { get; } = new(Kind.End);

        private enum Kind
        {
            Home,
            End
        }

        private readonly Kind _kind;

        private LogicalTextPointer(Kind kind)
        {
            _kind = kind;
        }

        public override bool Equals(object? obj) => Equals(obj as TextPointer);

        public override bool Equals(TextPointer? other)
            => other is LogicalTextPointer logical && _kind == logical._kind;

        public override int GetHashCode() => _kind.GetHashCode();
    }
}
