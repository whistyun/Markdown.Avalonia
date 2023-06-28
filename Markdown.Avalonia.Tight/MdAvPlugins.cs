using Avalonia.Metadata;
using Markdown.Avalonia.Plugins;
using System.Collections.ObjectModel;

namespace Markdown.Avalonia
{
    public class MdAvPlugins
    {
        private SetupInfo? _cache;

        [Content]
        public ObservableCollection<IMdAvPlugin> Plugins { get; }

        public SetupInfo Info => _cache ??= CreateInfo();

        public MdAvPlugins()
        {
            Plugins = new ObservableCollection<IMdAvPlugin>();
            Plugins.CollectionChanged += (s, e) => _cache = null;
        }

        protected virtual SetupInfo CreateInfo()
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

            setupInf.Freeze();

            return setupInf;
        }
    }
}
