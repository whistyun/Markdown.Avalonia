using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Avalonia.Svg;
using Svg.Model;
using Avalonia.Metadata;
using Avalonia.Media;

namespace Markdown.Avalonia.Svg
{
    internal class SvgImageLoader
    {
        private static readonly AvaloniaAssetLoader _svgAssetLoader = new();


        public IImage? Load(Stream stream)
        {
            if (IsSvgFile(stream))
            {
                var document = SvgExtensions.Open(stream);
                var picture = document is { } ? SvgExtensions.ToModel(document, _svgAssetLoader, out _, out _) : default;
                return new VectorImage() { Source = new SvgSource() { Picture = picture } };
            }

            return null;
        }

        private static bool IsSvgFile(Stream fileStream)
        {
            try
            {
                using (var xmlReader = XmlReader.Create(fileStream))
                {
                    return xmlReader.MoveToContent() == XmlNodeType.Element &&
                           "svg".Equals(xmlReader.Name, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                fileStream.Seek(0, SeekOrigin.Begin);
            }
        }
    }
}
