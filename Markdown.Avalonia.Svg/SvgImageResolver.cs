using System;
using System.IO;
using System.Xml;
using Avalonia.Svg;
using Svg.Model;
using Avalonia.Media;
using Markdown.Avalonia.Utils;
using System.Threading.Tasks;

namespace Markdown.Avalonia.Svg
{
    internal class SvgImageResolver : IImageResolver
    {
        private static readonly AvaloniaAssetLoader _svgAssetLoader = new();

        public async Task<IImage?> Load(Stream stream)
        {
            var task = Task.Run(() =>
            {
                if (IsSvgFile(stream))
                {
                    var document = SvgExtensions.Open(stream);
                    var picture = document is { } ? SvgExtensions.ToModel(document, _svgAssetLoader, out _, out _) : default;
                    var svgsrc = new SvgSource() { Picture = picture };
                    return (IImage)new VectorImage() { Source = svgsrc };
                }

                return null;
            });

            return await task;
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
