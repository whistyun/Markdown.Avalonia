using Avalonia;
using Avalonia.Controls;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

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

#if NET6_0_OR_GREATER
        public bool TryGet(object key, [MaybeNullWhen(false)] out object? val)
#else
        public bool TryGet(object key, out object val)
#endif
        {
            if (Owner.TryGetResource(key, null, out var ownerRsc))
            {
                val = ownerRsc!;
                return true;
            }

            StyledElement? node;

            if (Parent is null || !Parent.TryGetTarget(out node))
            {
                val = null!;
                return false;
            }

            while (node is object)
            {
                if (node.TryGetResource(key, out var rsc))
                {
                    val = rsc!;
                    return true;
                }

                node = node.Parent;
            }

            val = null!;
            return false;
        }
    }
}