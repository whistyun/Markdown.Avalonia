using Avalonia.Platform;
using Avalonia;
using AvaloniaEdit.Highlighting.Xshd;
using AvaloniaEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Xml;
using System.Linq;

namespace Markdown.Avalonia.SyntaxHigh
{
    public class SyntaxHighlightProvider
    {
        private ObservableCollection<Alias> _aliases;

        private Dictionary<string, string> _nameSolver;
        private Dictionary<string, IHighlightingDefinition> _definitions;

        public SyntaxHighlightProvider(ObservableCollection<Alias> aliases)
        {
            _aliases = aliases;
            _nameSolver = new Dictionary<string, string>();
            _definitions = new Dictionary<string, IHighlightingDefinition>();

            _aliases.CollectionChanged += (s, e) => AliasesCollectionChanged(e);
            AliasesCollectionChanged(null);
        }

        public IHighlightingDefinition Solve(string lang)
        {
            for (var i = 0; i < 10; ++i)
                if (_nameSolver.TryGetValue(lang.ToLower(), out var realName))
                    lang = realName;
                else
                    break;

            if (_definitions.TryGetValue(lang, out var def))
                return def;

            return HighlightingManager.Instance.GetDefinitionByExtension("." + lang);
        }

        private void AliasesCollectionChanged(NotifyCollectionChangedEventArgs? arg)
        {
            IEnumerable<Alias> adding;

            if (arg is null || arg.OldItems != null)
            {
                _nameSolver.Clear();
                _definitions.Clear();
                SetupForBuiltIn();

                adding = _aliases;
            }
            else if (arg?.NewItems != null)
            {
                adding = arg.NewItems.Cast<Alias>();
            }
            else
                adding = Array.Empty<Alias>();


            foreach (var alias in adding)
            {
                if (alias.Name is null) continue;

                if (!String.IsNullOrEmpty(alias.RealName))
                {
                    _nameSolver[alias.Name] = alias.RealName;
                }
                else if (alias.XSHD != null)
                {
                    var definition = Load(alias.XSHD);

                    if (definition is null)
                        throw new ArgumentException($"Failed loading: {alias.XSHD}");

                    _definitions[alias.Name] = definition;
                }
            }
        }

        private void SetupForBuiltIn()
        {
            // https://github.com/AvaloniaUI/AvaloniaEdit/blob/master/src/AvaloniaEdit/Highlighting/Resources/Resources.cs

            _nameSolver["c#"] = "cs";
            _nameSolver["csharp"] = "cs";
            _nameSolver["javascript"] = "js";
            _nameSolver["coco"] = "atg";
            _nameSolver["c++"] = "c";
            _nameSolver["powershell"] = "ps1";
            _nameSolver["python"] = "py";
            _nameSolver["markdown"] = "md";
        }

        private IHighlightingDefinition? Load(Uri source)
        {
            switch (source.Scheme)
            {
                case "file":
                    return File.Exists(source.LocalPath) ?
                        Open(File.OpenRead(source.LocalPath)) :
                        null;

                case "avares":
                    return AssetLoader.Exists(source) ?
                        Open(AssetLoader.Open(source)) :
                        null;

                default:
                    throw new ArgumentException($"unsupport scheme '{source.Scheme}'");
            }

            IHighlightingDefinition Open(Stream stream)
            {
                try
                {
                    using (var reader = XmlReader.Create(stream))
                        return HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
                finally
                {
                    stream.Close();
                }
            }
        }
    }
}
