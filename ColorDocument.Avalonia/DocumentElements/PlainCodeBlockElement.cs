using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using ColorTextBlock.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorDocument.Avalonia.DocumentElements
{
    public class PlainCodeBlockElement : DocumentElement
    {
        private string _code;
        private Lazy<CTextBlock> _text;
        private Lazy<Border> _border;

        public override Control Control => _border.Value;

        public override IEnumerable<DocumentElement> Children => Array.Empty<DocumentElement>();

        public PlainCodeBlockElement(string code)
        {
            _code = code;
            _border = new Lazy<Border>(CreateBlock);
            _text = new Lazy<CTextBlock>(() => (CTextBlock)(_border.Value.Child!));
        }

        public override SelectDirection Select(Point from, Point to)
        {
            var text = _text.Value;

            var fromPoint = text.CalcuatePointerFrom(from.X, from.Y);
            var toPoint = text.CalcuatePointerFrom(to.X, to.Y);

            text.Selection = new Selection(fromPoint, toPoint);

            return fromPoint <= toPoint ? SelectDirection.Forward : SelectDirection.Backward;
        }

        public override void UnSelect()
        {
            _text.Value.Selection = null;
        }

        public Border CreateBlock()
        {
            var ctxt = new TextBlock()
            {
                Text = _code,
                TextWrapping = TextWrapping.NoWrap
            };
            ctxt.Classes.Add(ClassNames.CodeBlockClass);

            var scrl = new ScrollViewer();
            scrl.Classes.Add(ClassNames.CodeBlockClass);
            scrl.Content = ctxt;
            scrl.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

            var result = new Border();
            result.Classes.Add(ClassNames.CodeBlockClass);
            result.Child = scrl;

            return result;
        }
    }
}
