using Avalonia;
using Avalonia.Controls;
using System;

namespace Markdown.Avalonia
{
    public class CascadeDictionary
    {
        public IResourceDictionary Owner { get; set; } = new ResourceDictionary();

        public WeakReference<StyledElement>? Parent { get; set; }

        public void SetParent(StyledElement element)
        {
            Parent = new WeakReference<StyledElement>(element);
        }

        public bool TryGet(object key, out object val)
        {
            if (Owner.TryGetResource(key, null, out val))
                return true;

            StyledElement? node;

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