using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Markdown.Avalonia.Utils
{
    public class DefaultPathResolver : IPathResolver
    {
        private static readonly HttpClient s_httpclient = new();

        public string? AssetPathRoot { set; private get; }
        public IEnumerable<string>? CallerAssemblyNames { set; private get; }

        public async Task<Stream?>? ResolveImageResource(string relativeOrAbsolutePath)
        {
            // absolute path?
            // ( http://... and C:\... )
            if (Uri.TryCreate(relativeOrAbsolutePath, UriKind.Absolute, out var aburl))
            {
                var stream = await OpenStream(aburl);
                if (stream is not null)
                    return stream;
            }

            if (CallerAssemblyNames is not null)
            {
                foreach (var asmNm in CallerAssemblyNames)
                {
                    var assetUrl = new Uri($"avares://{asmNm}/{relativeOrAbsolutePath}");

                    var stream = await OpenStream(assetUrl);
                    if (stream is not null)
                        return stream;
                }
            }

            if (AssetPathRoot is not null)
            {
                Uri pathUri;
                if (Path.IsPathRooted(AssetPathRoot))
                {
                    pathUri = new Uri(Path.Combine(AssetPathRoot, relativeOrAbsolutePath));
                }
                else
                {
                    pathUri = new Uri(new Uri(AssetPathRoot), relativeOrAbsolutePath);
                }

                var stream = await OpenStream(pathUri);
                if (stream is not null)
                    return stream;
            }

            throw new NotImplementedException();
        }

        private async Task<Stream?> OpenStream(Uri url)
        {
            switch (url.Scheme)
            {
                case "http":
                case "https":
                    var response = await s_httpclient.GetAsync(url);
                    return await response.Content.ReadAsStreamAsync();

                case "file":
                    if (!File.Exists(url.LocalPath)) return null;
                    return File.OpenRead(url.LocalPath);

                case "avares":
                    if (!AssetLoader.Exists(url))
                        return null;

                    return AssetLoader.Open(url);

                default:
                    throw new InvalidDataException($"unsupport scheme '{url.Scheme}'");
            }
        }
    }

}
