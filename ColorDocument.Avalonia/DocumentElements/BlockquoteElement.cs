using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using ColorDocument.Avalonia;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ColorDocument.Avalonia.DocumentElements
{
    /// <summary>
    /// The document element for expression of blockquote.
    /// </summary>
    // 引用を表現するためのドキュメント要素
    public class BlockquoteElement : DocumentElement
    {
        private readonly BorderedDocumentGroupElement _wrapped;

        public override Control Control => _wrapped.Control;
        public override IEnumerable<DocumentElement> Children => _wrapped.Children;

        public BlockquoteElement(IEnumerable<DocumentElement> child)
        {
            _wrapped = new BorderedDocumentGroupElement(child);
            Classes.Add(ClassNames.BlockquoteClass);
        }

        protected override void OnClassAdded(string className)
            => _wrapped.Classes.Add(className);

        protected override void OnHorizontalAlignmentChanged(HorizontalAlignment alignment)
            => _wrapped.HorizontalAlignment = alignment;

        public override void Select(Point from, Point to)
        {
            _wrapped.Select(from, to);
        }

        public override void UnSelect()
        {
            _wrapped.UnSelect();
        }

        public override void ConstructSelectedText(StringBuilder builder)
        {
            _wrapped.ConstructSelectedText(builder);
        }
    }
}
