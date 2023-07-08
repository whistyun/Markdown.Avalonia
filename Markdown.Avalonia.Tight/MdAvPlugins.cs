using Avalonia.Metadata;
using Markdown.Avalonia.Plugins;
using Markdown.Avalonia.Utils;
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

namespace Markdown.Avalonia
{
    public class MdAvPlugins
    {
        private SetupInfo? _cache;
        private IPathResolver? _pathResolver;
        private IContainerBlockHandler? _containerBlockHandler;
        private ICommand? _hyperlinkCommand;

        [Content]
        public ObservableCollection<IMdAvPlugin> Plugins { get; }

        public IPathResolver? PathResolver
        {
            get => _pathResolver; set
            {
                _cache = null;
                _pathResolver = value;
            }
        }
        public IContainerBlockHandler? ContainerBlockHandler
        {
            get => _containerBlockHandler;
            set
            {
                _cache = null;
                _containerBlockHandler = value;
            }
        }
        public ICommand? HyperlinkCommand
        {
            get => _hyperlinkCommand;
            set
            {
                _cache = null;
                _hyperlinkCommand = value;
            }
        }

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

            (
                IEnumerable<IMdAvPlugin> orderedPlugin,
                Dictionary<Type, IMdAvPlugin> dic
            ) = ComputeOrderedPlugins();

            foreach (var plugin in orderedPlugin)
            {
                if (plugin is IMdAvPluginRequestAnother another)
                {
                    another.Inject(another.DependsOn.Select(dep => dic[dep]));
                }

                plugin.Setup(setupInf);

                hasBuiltin |= plugin.GetType().FullName == SetupInfo.BuiltinTpNm;
            }

            if (!hasBuiltin)
                setupInf.Builtin();

            if (PathResolver is not null)
                setupInf.SetOnce(PathResolver);

            if (ContainerBlockHandler is not null)
                setupInf.SetOnce(ContainerBlockHandler);

            if (HyperlinkCommand is not null)
                setupInf.SetOnce(HyperlinkCommand);

            setupInf.Freeze();

            return setupInf;
        }



        protected (IEnumerable<IMdAvPlugin>, Dictionary<Type, IMdAvPlugin>) ComputeOrderedPlugins()
        {
            var plugins = new List<IMdAvPlugin>(Plugins.Count);
            var dic = Plugins.ToDictionary(p => p.GetType(), p => p);

            foreach (var plugin in Plugins)
            {
                if (plugin is IMdAvPluginRequestAnother anoth)
                {
                    ComputeDepneds(anoth, plugins, dic);
                }
                plugins.Add(plugin);
            }

            plugins.Sort(new PluginComparer(dic));

            return (plugins, dic);


            void ComputeDepneds(IMdAvPluginRequestAnother anoth, List<IMdAvPlugin> plugins, Dictionary<Type, IMdAvPlugin> dic)
            {
                foreach (var dep in anoth.DependsOn)
                {
                    if (dic.ContainsKey(dep))
                        continue;

                    var depPlugin = Activator.CreateInstance(dep) as IMdAvPlugin;

                    if (depPlugin is null)
                        throw new NullReferenceException("Failed to create an instance of " + dep);

                    if (depPlugin is IMdAvPluginRequestAnother anothanoth)
                        ComputeDepneds(anothanoth, plugins, dic);

                    plugins.Add(depPlugin);
                    dic[dep] = depPlugin;
                }
            }

        }


        class PluginComparer : IComparer<IMdAvPlugin>
        {
            private Dictionary<Type, IMdAvPlugin> _plugins;

            public PluginComparer(Dictionary<Type, IMdAvPlugin> plugins)
            {
                _plugins = plugins;
            }

            public int Compare(IMdAvPlugin? x, IMdAvPlugin? y)
            {
                if (x is IMdAvPluginRequestAnother xanoth && y is IMdAvPluginRequestAnother yanoth)
                {
                    return
                        // dose x request y?
                        ComputeRequest(xanoth, yanoth) ? -1 :
                        // dose y request x?
                        ComputeRequest(yanoth, xanoth) ? 1 :
                        0;
                }

                if (x is not IMdAvPluginRequestAnother && y is not IMdAvPluginRequestAnother)
                    return 0;

                if (x is IMdAvPluginRequestAnother && y is not IMdAvPluginRequestAnother)
                    return 1;

                if (x is not IMdAvPluginRequestAnother && y is IMdAvPluginRequestAnother)
                    return -1;

                throw new NotImplementedException();
            }

            private bool ComputeRequest(IMdAvPluginRequestAnother x, IMdAvPluginRequestAnother y)
            {
                foreach (var depType in x.DependsOn)
                {
                    var depPlugin = _plugins[depType];

                    if (ReferenceEquals(depPlugin, y))
                        return true;

                    if (depPlugin is IMdAvPluginRequestAnother depPluginAnoth && ComputeRequest(depPluginAnoth, y))
                        return true;
                }

                return false;
            }

        }
    }
}
