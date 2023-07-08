using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Markdown.Avalonia.Plugins
{
    public interface IMdAvPlugin
    {
        void Setup(SetupInfo info);
    }

    public interface IMdAvPluginRequestAnother : IMdAvPlugin
    {
        IEnumerable<Type> DependsOn { get; }

        void Inject(IEnumerable<IMdAvPlugin> plugin);
    }
}
