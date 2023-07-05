using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Markdown.Avalonia.Utils
{
    [Obsolete("see https://github.com/whistyun/Markdown.Avalonia/wiki/How-to-migrages-to-ver11")]
    public class DefaultBitmapLoader : IBitmapLoader
    {
        private static readonly HttpClient _httpclient = new();

        public string AssetPathRoot { set; private get; }
        private string[] AssetAssemblyNames { get; }

        private ConcurrentDictionary<Uri, WeakReference<Bitmap>> Cache;

        public DefaultBitmapLoader()
        {
            AssetPathRoot = Environment.CurrentDirectory;

            var stack = new StackTrace();
            this.AssetAssemblyNames = stack.GetFrames()
                            .Select(frm => frm?.GetMethod()?.DeclaringType?.Assembly?.GetName()?.Name)
                            .OfType<string>()
                            .Where(name => !name.Equals("Markdown.Avalonia"))
                            .Distinct()
                            .ToArray();

            Cache = new ConcurrentDictionary<Uri, WeakReference<Bitmap>>();
        }


        private void Compact()
        {
            foreach (var entry in Cache.ToArray())
            {
                if (!entry.Value.TryGetTarget(out var _))
                {
                    ((IDictionary<Uri, WeakReference<Bitmap>>)Cache).Remove(entry.Key);
                }
            }
        }

        public Task<Bitmap?> GetAsync(string urlTxt)
        {
            return Task.Run(() => Get(urlTxt));
        }

        public Bitmap? Get(string urlTxt)
        {
            Bitmap? imgSource = null;

            // check network
            if (Uri.TryCreate(urlTxt, UriKind.Absolute, out var url))
            {
                imgSource = Get(url);
            }

            // check resources
            if (imgSource is null)
            {
                foreach (var asmNm in AssetAssemblyNames)
                {
                    var assetUrl = new Uri($"avares://{asmNm}/{urlTxt}");
                    imgSource = Get(assetUrl);

                    if (imgSource != null) break;
                }
            }

            // check filesystem
            if (imgSource is null && AssetPathRoot != null)
            {
                try
                {
                    if (Uri.IsWellFormedUriString(AssetPathRoot, UriKind.Absolute))
                    {
                        imgSource = Get(new Uri(new Uri(AssetPathRoot), urlTxt));
                    }
                    else
                    {
                        using (var strm = File.OpenRead(Path.Combine(AssetPathRoot, urlTxt)))
                            imgSource = new Bitmap(strm);
                    }
                }
                catch { }
            }

            return imgSource;
        }

        public Bitmap? Get(Uri url)
        {
            if (Cache.TryGetValue(url, out var reference))
            {
                if (reference.TryGetTarget(out var image))
                {
                    return image;
                }
            }

            Compact();

            Bitmap imgSource;

            try
            {
                switch (url.Scheme)
                {
                    case "http":
                    case "https":
                        using (var res = _httpclient.GetAsync(url).Result)
                        using (var strm = res.Content.ReadAsStreamAsync().Result)
                            imgSource = new Bitmap(strm);
                        break;

                    case "file":
                        if (!File.Exists(url.LocalPath)) return null;

                        using (var strm = File.OpenRead(url.LocalPath))
                            imgSource = new Bitmap(strm);
                        break;

                    case "avares":
                        using (var strm = AssetLoader.Open(url))
                            imgSource = new Bitmap(strm);
                        break;

                    default:
                        throw new InvalidDataException($"unsupport scheme '{url.Scheme}'");
                }
            }
            catch
            {
                return null;
            }

            Cache[url] = new WeakReference<Bitmap>(imgSource);

            return imgSource;
        }
    }
}
