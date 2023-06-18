using Avalonia.Controls;
using Avalonia.Controls.Shapes;
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
    public interface IPathResolver
    {
        string? AssetPathRoot { set; }
        Uri? SourceBasePath { set; }
        IEnumerable<string>? CallerAssemblyNames { set; }

        /// <summary>
        /// hyperlinkに関するパスを絶対化します。
        /// </summary>
        /// <param name="relativeOrAbsolutePath">hyperlink先のパス</param>
        /// <returns>絶対化されたパス。解決できない場合やリンクを有効にしたくない場合はnull</returns>
        string? ResolveHyperlink(string relativeOrAbsolutePath);

        /// <summary>
        /// 画像ファイルへのパスを絶対化します。
        /// </summary>
        /// <param name="relativeOrAbsolutePath">画像のパス</param>
        /// <returns>
        /// 絶対化されたパス。
        /// リンクを有効にしたくない場合はnull。
        /// 非同期で検索した結果解決できなかった場合はResult=nullのTask</returns>
        Task<Stream?>? ResolveImageResource(string relativeOrAbsolutePath);
    }

    public interface IBitmapResolver
    {
        Bitmap? Load(Stream stream);
    }


    public class DefaultPathResolver : IPathResolver
    {
        private static readonly HttpClient s_httpclient = new();

        private IAssetLoader? _loader;

        public string? AssetPathRoot { set; private get; }
        public Uri? SourceBasePath { set; private get; }
        public IEnumerable<string>? CallerAssemblyNames { set; private get; }

        public string? ResolveHyperlink(string relativeOrAbsolutePath)
        {
            if (Uri.TryCreate(relativeOrAbsolutePath, UriKind.Absolute, out var _))
            {
                return relativeOrAbsolutePath;
            }

            if (File.Exists(relativeOrAbsolutePath))
            {
                return relativeOrAbsolutePath;
            }

            if (SourceBasePath is not null)
            {
                return (new Uri(SourceBasePath, relativeOrAbsolutePath)).AbsoluteUri;
            }

            if (AssetPathRoot is not null)
            {
                return (new Uri(new Uri(AssetPathRoot), relativeOrAbsolutePath)).AbsoluteUri;
            }

            return null;
        }

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

            if (SourceBasePath is not null)
            {
                var path = new Uri(SourceBasePath, relativeOrAbsolutePath);
                var stream = await OpenStream(path);
                if (stream is not null)
                    return stream;
            }

            if (AssetPathRoot is not null)
            {
                var path = new Uri(new Uri(AssetPathRoot), relativeOrAbsolutePath);
                var stream = await OpenStream(path);
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
                    if (_loader is null)
                    {
                        _loader = Helper.GetAssetLoader();

                        if (_loader is null)
                            return null;
                    }

                    if (!_loader.Exists(url))
                        return null;

                    return _loader.Open(url);

                default:
                    throw new InvalidDataException($"unsupport scheme '{url.Scheme}'");
            }
        }
    }

}
