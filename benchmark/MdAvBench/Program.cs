using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReverseMarkdown.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MdAvBench.Apps;
using System.Threading;

namespace MdAvBench
{
    class Program
    {
        public static void Main(string[] args)
        {
            var summaries = BenchmarkRunner.Run(typeof(Program).Assembly);
            var exporter = AsciiDocExporter.Default;

            var logger = new StringLogger();

            foreach (var summary in summaries)
            {
                exporter.ExportToLog(summary, logger);
                logger.WriteLine();
            }
            File.WriteAllText("summary.md", logger.ToString());
        }
    }
}