using BenchmarkDotNet.Loggers;
using System.Text;

namespace MdAvBench
{
    internal class StringLogger : ILogger
    {
        private StringBuilder _builder = new StringBuilder();

        public string Id => "StringLogger";
        public int Priority => 0;

        public StringLogger() { }

        public void Flush()
        {
        }

        public void Write(LogKind logKind, string text)
        {
            _builder.Append(text);
        }

        public void WriteLine()
        {
            _builder.AppendLine();
        }

        public void WriteLine(LogKind logKind, string text)
        {
            _builder.AppendLine(text);
        }

        public override string ToString() => _builder.ToString();
    }
}
