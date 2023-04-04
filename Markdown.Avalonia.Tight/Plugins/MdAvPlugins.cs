using Avalonia.Controls;
using Avalonia.Metadata;
using Markdown.Avalonia.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Markdown.Avalonia
{
    public class MdAvPlugins
    {

        [Content]
        public ObservableCollection<IMdAvPlugin> Plugins { get; }

        public MdAvPlugins()
        {
            Plugins = new ObservableCollection<IMdAvPlugin>();
        }


        internal SetupInfo CreateInfo()
        {
            var setupInf = new SetupInfo();
            bool hasBuiltin = false;

            foreach (var plugin in Plugins)
            {
                plugin.Setup(setupInf);

                hasBuiltin |= plugin.GetType().FullName == SetupInfo.BuiltinTpNm;
            }

            if (!hasBuiltin)
                setupInf.Builtin();

            return setupInf;
        }
    }
}
