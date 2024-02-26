using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Markdown.Avalonia.SyntaxHigh
{
    internal class CodePad : Panel
    {
        private bool _isMenuVisible;
        private Control? _content;
        private Control? _alwaysShowMenu;
        private Control? _expandableMenu;

        public Control? Content
        {
            get => _content; set
            {
                if (_content is not null)
                {
                    Children.Remove(_content);
                }
                _content = value;
                if (_content is not null)
                {
                    Children.Insert(0, _content);
                }
            }
        }

        public Control? ExandableMenu
        {
            get => _expandableMenu;
            set
            {
                if (_expandableMenu is not null)
                {
                    Children.Remove(_expandableMenu);
                }

                _expandableMenu = value;

                if (_expandableMenu is not null && _isMenuVisible)
                {
                    Children.Add(_expandableMenu);
                }
            }
        }

        public Control? AlwaysShowMenu
        {
            get => _alwaysShowMenu;
            set
            {
                if (_alwaysShowMenu is not null)
                {
                    Children.Remove(_alwaysShowMenu);
                }

                _alwaysShowMenu = value;

                if (_alwaysShowMenu is not null)
                {
                    Children.Add(_alwaysShowMenu);
                }
            }
        }

        public CodePad()
        {
        }

        protected override void OnPointerEntered(PointerEventArgs args)
        {
            if (!_isMenuVisible)
            {
                _isMenuVisible = true;

                if (_expandableMenu is not null)
                {
                    if (!Children.Contains(_expandableMenu))
                        Children.Add(_expandableMenu);
                    else
                    {
                        _expandableMenu.IsVisible = true;
                    }
                }
            }
        }

        protected override void OnPointerExited(PointerEventArgs e)
        {

            if (_isMenuVisible && _expandableMenu is not null)
            {
                if (Children.Contains(_expandableMenu))
                {
                    _expandableMenu.IsVisible = false;
                }

                _isMenuVisible = false;
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var reqSz = base.MeasureOverride(availableSize);

            double addHeight =
                _alwaysShowMenu is not null ? _alwaysShowMenu.DesiredSize.Height : 0d;

            double height =
                _expandableMenu is not null ? _expandableMenu.DesiredSize.Height : 0d;

            return new Size(reqSz.Width, addHeight + Math.Max(reqSz.Height, height));

        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var yoffset = _alwaysShowMenu is not null ? _alwaysShowMenu.DesiredSize.Height : 0d;

            if (_content is not null)
            {
                _content.Arrange(new Rect(0, yoffset / 2, finalSize.Width, finalSize.Height));
            }

            if (_alwaysShowMenu is not null)
            {
                var menuSz = _alwaysShowMenu.DesiredSize;
                _alwaysShowMenu.Arrange(new Rect(finalSize.Width - menuSz.Width, 0, menuSz.Width, menuSz.Height));
            }

            if (_expandableMenu is not null)
            {
                var menuSz = _expandableMenu.DesiredSize;
                _expandableMenu.Arrange(new Rect(finalSize.Width - menuSz.Width, yoffset, menuSz.Width, menuSz.Height));
            }

            return finalSize;
        }

    }
}
