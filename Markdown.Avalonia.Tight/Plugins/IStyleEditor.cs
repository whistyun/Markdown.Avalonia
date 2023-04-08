using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Text;

namespace Markdown.Avalonia.Plugins
{
    public interface IStyleEdit
    {
        void Edit(string styleName, Styles style);
    }
}
