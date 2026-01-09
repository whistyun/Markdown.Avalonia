using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Svg;
using Markdown.Avalonia.Svg;
using Markdown.Avalonia.Utils;
using Svg.Model;
using Markdown.Avalonia.Plugins;
using Markdown.Avalonia;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using Markdown.Avalonia.Parsers;
using ColorDocument.Avalonia;
using ColorDocument.Avalonia.DocumentElements;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Threading;
using PuppeteerSharp;
using System.Threading;

namespace Markdown.Avalonia.Mermaid
{
	public class MermaidBlockHandler : IContainerBlockHandler, IMdAvPlugin
	{
		private static readonly AvaloniaAssetLoader _assetLoader = new();
		private static IBrowser? _browser;
		private static readonly SemaphoreSlim _browserLock = new(1, 1);
		private static string? _mermaidJsContent;

		public string Theme { get; set; } = "default";
		public string BackgroundColor { get; set; } = "transparent";

		public void Setup(SetupInfo info)
		{
			var cs = new ContainerSwitch();
			cs["mermaid"] = this;
			info.SetOnce(cs);

			InjectBlockOverride(info);
		}

		private void InjectBlockOverride(SetupInfo info)
		{
			var prop = typeof(SetupInfo).GetProperty("BlockOverrides", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			if (prop != null)
			{
				var list = prop.GetValue(info) as IList;
				if (list != null)
				{
					IBlockOverride? inner = null;
					foreach (var item in list)
					{
						if (item is IBlockOverride ibo && ibo.ParserName == "CodeBlocksWithLangEvaluator")
						{
							inner = ibo;
							break;
						}
					}

					if (inner != null)
					{
						list.Remove(inner);
					}

					// Register replacement (wraps the inner if it existed)
					info.Register(new MermaidFencedBlockOverride(inner, this));
				}
			}
		}

		public Border? ProvideControl(string assetPathRoot, string blockName, string lines)
		{
			if (!blockName.Trim().Equals("mermaid", StringComparison.OrdinalIgnoreCase))
			{
				return null;
			}

			var border = new Border()
			{
				Child = new TextBlock() { Text = "Initializing Mermaid...", FontStyle = FontStyle.Italic, Foreground = Brushes.Gray, Margin = new Thickness(5) }
			};

			// Capture state to avoid thread safety issues if properties change
			var theme = Theme;
			var bgColor = BackgroundColor;

			Task.Run(async () =>
			{
				try
				{
					var browser = await EnsureBrowserAsync(border);
					if (browser == null) return; // Error handled in EnsureBrowserAsync

					Dispatcher.UIThread.Post(() => {
						if (border.Child is TextBlock tb) tb.Text = "Rendering Mermaid Diagram...";
					});

					string svgContent = await RenderMermaidAsync(browser, lines, theme, bgColor);
					if (string.IsNullOrEmpty(svgContent))
					{
						Dispatcher.UIThread.Post(() => border.Child = CreateErrorBorder("Rendered SVG was empty.").Child);
						return;
					}


					bool success = false;
					try
					{
						// We need to write to a stream or parse string directly
						// SvgExtensions.Open expects a stream.
						using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(svgContent)))
						{
							var document = SvgExtensions.Open(stream);
							if (document != null)
							{
								var picture = SvgExtensions.ToModel(document, _assetLoader, out _, out _);
								var svgsrc = new SvgSource() { Picture = picture };
								Dispatcher.UIThread.Post(() =>
								{
									border.Child = new Image
									{
										Source = new VectorImage { Source = svgsrc }
									};
								});
								success = true;
							}
						}
					}
					catch (Exception ex)
					{
						Dispatcher.UIThread.Post(() => border.Child = CreateErrorBorder($"Error parsing SVG: {ex.Message}").Child);
					}

					if (!success && !(border.Child is Image))
					{
						Dispatcher.UIThread.Post(() => border.Child = CreateErrorBorder("Failed to process SVG.").Child);
					}
				}
				catch (Exception ex)
				{
					Dispatcher.UIThread.Post(() => border.Child = CreateErrorBorder($"Error: {ex.Message}").Child);
				}
			});

			return border;
		}

		private async Task<IBrowser?> EnsureBrowserAsync(Border borderContext)
		{
			if (_browser != null && !_browser.IsClosed) return _browser;

			await _browserLock.WaitAsync();
			try
			{
				if (_browser != null && !_browser.IsClosed) return _browser;

				Dispatcher.UIThread.Post(() => {
					if (borderContext.Child is TextBlock tb) tb.Text = "Downloading internal browser engine (one-time setup)...";
				});

				// Download browser if needed
				var browserFetcher = new BrowserFetcher();
				await browserFetcher.DownloadAsync();

				Dispatcher.UIThread.Post(() => {
					if (borderContext.Child is TextBlock tb) tb.Text = "Launching browser engine...";
				});

				_browser = await Puppeteer.LaunchAsync(new LaunchOptions
				{
					Headless = true,
					Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" } 
				});

				return _browser;
			}
			catch (Exception ex)
			{
				Dispatcher.UIThread.Post(() => borderContext.Child = CreateErrorBorder($"Failed to launch browser: {ex.Message}").Child);
				return null;
			}
			finally
			{
				_browserLock.Release();
			}
		}

		private async Task<string> RenderMermaidAsync(IBrowser browser, string code, string theme, string bgColor)
		{
			// Load mermaid js content if not loaded
			if (_mermaidJsContent == null)
			{
				var assembly = Assembly.GetExecutingAssembly();
				var resourceName = "Markdown.Avalonia.Mermaid.Assets.mermaid.min.js";
				// Depending on how it's embedded, might need full name check
				// But given namespace and folder, likely correct.
				using var stream = assembly.GetManifestResourceStream(resourceName);
				if (stream != null)
				{
					using var reader = new StreamReader(stream);
					_mermaidJsContent = await reader.ReadToEndAsync();
				}
				else
				{
					// Fallback: check resource names
					var names = assembly.GetManifestResourceNames();
					throw new FileNotFoundException($"Embedded mermaid.min.js not found. Available: {string.Join(", ", names)}");
				}
			}

			using var page = await browser.NewPageAsync();

			string htmlCrumbs = $@"
<!DOCTYPE html>
<html>
<head>
</head>
<body>
    <div id='graphDiv'></div>
    <script>
        {_mermaidJsContent}
    </script>
</body>
</html>";
			
			await page.SetContentAsync(htmlCrumbs);

			// Now eval
			return await page.EvaluateFunctionAsync<string>(@"async (code, theme) => {
                try {
                    mermaid.initialize({ startOnLoad: false, theme: theme });
                    const { svg } = await mermaid.render('graphDiv', code);
                    return svg;
                } catch (e) {
                    return 'ERROR: ' + e.message;
                }
            }", code, theme);
		}

		private Border CreateErrorBorder(string message)
		{
			return new Border
			{
				Child = new TextBlock { Text = message, Foreground = Brushes.Red, TextWrapping = TextWrapping.Wrap }
			};
		}

		public class MermaidFencedBlockOverride : BlockOverride2
		{
			private IBlockOverride? _inner;
			private MermaidBlockHandler _handler;

			public MermaidFencedBlockOverride(IBlockOverride? inner, MermaidBlockHandler handler)
				: base("CodeBlocksWithLangEvaluator")
			{
				_inner = inner;
				_handler = handler;
			}

			public override IEnumerable<DocumentElement>? Convert2(string text, Match firstMatch, ParseStatus status, IMarkdownEngine2 engine, out int parseTextBegin, out int parseTextEnd)
			{
				string lang = firstMatch.Groups[2].Value.Trim();

				if (string.Equals(lang, "mermaid", StringComparison.OrdinalIgnoreCase))
				{
					var closeTagPattern = new Regex($"\\n[ ]*{Regex.Escape(firstMatch.Groups[1].Value)}[ ]*\\n");
					var closeTagMatch = closeTagPattern.Match(text, firstMatch.Index + firstMatch.Length);

					int codeEndIndex;
					if (closeTagMatch.Success)
					{
						codeEndIndex = closeTagMatch.Index;
						parseTextEnd = closeTagMatch.Index + closeTagMatch.Length;
					}
					else
					{
						// Fallback close?
						parseTextEnd = text.Length;
						codeEndIndex = text.Length;
					}

					parseTextBegin = firstMatch.Index;
					string code = text.Substring(firstMatch.Index + firstMatch.Length, codeEndIndex - (firstMatch.Index + firstMatch.Length));

					string assetPath = engine.AssetPathRoot ?? "";
					var border = _handler.ProvideControl(assetPath, "mermaid", code);
					if (border != null)
					{
						return new[] { new UnBlockElement(border) };
					}
				}

				if (_inner is BlockOverride2 override2)
				{
					return override2.Convert2(text, firstMatch, status, engine, out parseTextBegin, out parseTextEnd);
				}

				// Re-implement default logic if inner is null or incompatible
				// Based on FencedCodeBlockParser
				var closeTagPattern2 = new Regex($"\\n[ ]*{Regex.Escape(firstMatch.Groups[1].Value)}[ ]*\\n");
				var closeTagMatch2 = closeTagPattern2.Match(text, firstMatch.Index + firstMatch.Length);

				int codeEndIndex2;
				if (closeTagMatch2.Success)
				{
					codeEndIndex2 = closeTagMatch2.Index;
					parseTextEnd = closeTagMatch2.Index + closeTagMatch2.Length;
				}
				else
				{
					// FencedCodeBlockParser behavior for unclosed blocks (depends on enablePreRenderingCodeBlock)
					// We don't have access to that flag easily here, assume strict? 
					// Or actually lenient because 'text' might be the whole doc.
					// Actually FencedCodeBlockParser returns null if strict.
					parseTextBegin = -1;
					parseTextEnd = -1;
					return null;
				}

				parseTextBegin = firstMatch.Index;
				string code2 = text.Substring(firstMatch.Index + firstMatch.Length, codeEndIndex2 - (firstMatch.Index + firstMatch.Length));

				var ctxt = new TextBlock() { Text = code2, TextWrapping = TextWrapping.NoWrap };
				ctxt.Classes.Add("CodeBlock");
				var scrl = new ScrollViewer();
				scrl.Classes.Add("CodeBlock");
				scrl.Content = ctxt;
				scrl.HorizontalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto;
				var border2 = new Border();
				border2.Classes.Add("CodeBlock");
				border2.Child = scrl;

				return new[] { new UnBlockElement(border2) };
			}
		}
	}
}
