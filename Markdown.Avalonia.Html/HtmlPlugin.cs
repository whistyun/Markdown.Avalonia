using Markdown.Avalonia.Plugins;
using Markdown.Avalonia.SyntaxHigh;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Markdown.Avalonia.Html
{
    public class HtmlPlugin : IMdAvPluginRequestAnother
    {
        private SyntaxHighlight? _syntax;

        public IEnumerable<Type> DependsOn => new[] { typeof(SyntaxHighlight) };

        public void Inject(IEnumerable<IMdAvPlugin> plugin)
        {
            _syntax = (SyntaxHighlight)plugin.First();
        }

        public void Setup(SetupInfo info)
        {
            if (_syntax is null)
                throw new InvalidOperationException("SyntaxHighlight requried");

            var _block = new HtmlBlockParser(_syntax, info);
            var _inline = new HtmlInlineParser(_syntax, info);

            info.EnableNoteBlock = false;
            info.RegisterTop(_block);
            info.Register(_inline);
        }
    }
}
