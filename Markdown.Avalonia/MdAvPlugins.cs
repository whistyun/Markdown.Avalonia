using Markdown.Avalonia.Plugins;
using Markdown.Avalonia.Svg;
using Markdown.Avalonia.SyntaxHigh;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Markdown.Avalonia.Full
{
    public class MdAvPlugins : global::Markdown.Avalonia.MdAvPlugins
    {
        public MdAvPlugins()
        {
        }

        protected override SetupInfo CreateInfo()
        {
            var setupInf = new SetupInfo();

            var hasSyntaxHigh = false;
            var hasSvgFormat = false;

            foreach (var plugin in Plugins)
            {
                plugin.Setup(setupInf);

                hasSyntaxHigh |= plugin is SyntaxHiglight;
                hasSvgFormat |= plugin is SvgFormat;

            }

            if (!hasSyntaxHigh)
            {
                var syntaxPlugin = new SyntaxHiglight();
                syntaxPlugin.Setup(setupInf);
            }

            if (!hasSvgFormat)
            {
                var svgPlugin = new SvgFormat();
                svgPlugin.Setup(setupInf);
            }

            setupInf.Freeze();

            return setupInf;
        }
    }
}
