using System;
using Markdown.Avalonia.Utils;

namespace Markdown.Avalonia.Mermaid
{
    public static class MermaidExtensions
    {
        public static T UseMermaid<T>(this T engine, string theme = "default", string backgroundColor = "transparent") where T : class, IMarkdownEngine
        {
            var handler = new MermaidBlockHandler
            {
                Theme = theme,
                BackgroundColor = backgroundColor
            };
            engine.Plugins.Plugins.Add(handler);
            return engine;
        }
    }
}
