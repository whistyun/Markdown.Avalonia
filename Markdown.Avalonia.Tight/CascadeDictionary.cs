using Avalonia;
using Avalonia.Controls;
using System;

namespace Markdown.Avalonia
{
    internal class CascadeDictionary
    {
        public IResourceDictionary Owner { get; set; } = new ResourceDictionary();

        public WeakReference<IStyledElement> Parent { get; set; }

        public void SetParent(IStyledElement element)
        {
            Parent = new WeakReference<IStyledElement>(element);
        }

        public bool TryGet(object key, out object val)
        {
            if (Owner.TryGetResource(key, out val))
                return true;

            IStyledElement node;

            if (Parent is null)
                return false;

            if (!Parent.TryGetTarget(out node))
                return false;

            while (node is object)
            {
                if (node.TryGetResource(key, out val))
                    return true;

                node = node.Parent;
            }

            return false;
        }
    }
}
