using Markdown.Avalonia.Plugins;

namespace Markdown.Avalonia.Svg
{
    public class SvgFormat : IMdAvPlugin
    {
        public void Setup(SetupInfo info)
        {
            info.Register(new SvgImageResolver());
        }
    }
}
