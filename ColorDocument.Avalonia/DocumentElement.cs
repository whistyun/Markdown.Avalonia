using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using System;
using System.Collections.Generic;
using System.Text;

namespace ColorDocument.Avalonia
{
    public abstract class DocumentElement
    {
        private ISelectionRenderHelper? _helper;
        private readonly DocumentElementClassCollection _classes;
        private HorizontalAlignment? _alignment;
        private readonly List<string> _classNames = new();


        protected DocumentElement()
        {
            _classes = new DocumentElementClassCollection(this);
        }

        /// <summary>
        /// Style classes for this element. Adding does not traverse child <see cref="DocumentElement"/> nodes.
        /// </summary>
        public DocumentElementClassCollection Classes => _classes;

        /// <summary>
        /// Horizontal alignment for this block's root control. Buffered until the root is created.
        /// </summary>
        public HorizontalAlignment? HorizontalAlignment
        {
            get => _alignment;
            set
            {
                _alignment = value;
                if (_alignment.HasValue)
                    OnHorizontalAlignmentChanged(_alignment.Value);
            }
        }

        /// <summary>
        /// Called after a class name is appended to the accumulated class list (and after live target update when applicable).
        /// </summary>
        protected virtual void OnClassAdded(string className) { }

        /// <summary>
        /// Called after <see cref="HorizontalAlignment"/> is assigned through the property setter.
        /// Override to propagate alignment to wrapped elements; subscribe via <see cref="OnHorizontalAlignmentUpdated"/> when preferred.
        /// </summary>
        protected virtual void OnHorizontalAlignmentChanged(HorizontalAlignment value) { }

        internal void AddClassName(string item)
        {
            if (string.IsNullOrEmpty(item))
                return;
            _classNames.Add(item);
            OnClassAdded(item);
        }

        protected void ApplyEffects(Control target)
        {
            foreach (var c in _classNames)
            {
                if (!target.Classes.Contains(c))
                    target.Classes.Add(c);
            }

            if (_alignment.HasValue)
            {
                target.HorizontalAlignment = _alignment.Value;
            }
        }

        public abstract Control Control { get; }
        public abstract IEnumerable<DocumentElement> Children { get; }

        public ISelectionRenderHelper? Helper
        {
            get => _helper;
            set
            {
                _helper = value;
                foreach (var child in Children)
                    child.Helper = value;
            }
        }

        public Rect GetRect(Layoutable anchor) => Control.GetRectInDoc(anchor).GetValueOrDefault();
        public abstract void Select(Point from, Point to);
        public abstract void UnSelect();

        public virtual string GetSelectedText()
        {
            var builder = new StringBuilder();
            ConstructSelectedText(builder);
            return builder.ToString();
        }

        public abstract void ConstructSelectedText(StringBuilder stringBuilder);
    }

    /// <summary>
    /// Buffered style classes for a <see cref="DocumentElement"/>.
    /// <see cref="Add"/> does not materialize lazy controls until the owning element applies them to a live <see cref="StyledElement"/>.
    /// </summary>
    public sealed class DocumentElementClassCollection
    {
        private readonly DocumentElement _owner;

        internal DocumentElementClassCollection(DocumentElement owner)
        {
            _owner = owner;
        }

        public void Add(string item) => _owner.AddClassName(item);
    }

    public interface ISelectionRenderHelper
    {
        void Register(Control control);
        void Unregister(Control control);
    }
}
