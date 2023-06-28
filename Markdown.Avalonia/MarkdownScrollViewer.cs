using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Markdown.Avalonia.Full
{
    public class MarkdownScrollViewer : global::Markdown.Avalonia.MarkdownScrollViewer
    {

        public MarkdownScrollViewer()
        {
            Plugins = new MdAvPlugins();
        }
    }
}
