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
using Avalonia.Styling;

namespace Markdown.Avalonia.Mermaid
{
	public class MermaidBlockHandler : IContainerBlockHandler, IMdAvPlugin
	{
		private static readonly AvaloniaAssetLoader _assetLoader = new();
		private static IBrowser? _browser;
		private static readonly SemaphoreSlim _browserLock = new(1, 1);
        public static bool EnableMermaidRendering { get; set; } = true;
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
				if (prop.GetValue(info) is IList list)
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

			if (string.Equals(theme, "default", StringComparison.OrdinalIgnoreCase))
			{
				if (Application.Current?.ActualThemeVariant == ThemeVariant.Dark)
				{
					theme = "dark";
				}
			}

            if (!EnableMermaidRendering)
            {
                return border;
            }

			Task.Run(async () =>
			{
				try
				{
					var browser = await EnsureBrowserAsync(border);
					if (browser == null) return; // Error handled in EnsureBrowserAsync

					Dispatcher.UIThread.Post(() =>
					{
						if (border.Child is TextBlock tb) tb.Text = "Rendering Mermaid Diagram...";
					});

					string svgContent = await RenderMermaidAsync(browser, lines, theme, bgColor);
					if (string.IsNullOrEmpty(svgContent))
					{
						Dispatcher.UIThread.Post(() => border.Child = CreateErrorControl("Rendered SVG was empty."));
						return;
					}

					if (svgContent.StartsWith("ERROR:"))
					{
						Dispatcher.UIThread.Post(() => border.Child = CreateErrorControl(svgContent));
						return;
					}

					await Dispatcher.UIThread.InvokeAsync(() =>
					{
						try
						{
							using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(svgContent));
							var document = SvgExtensions.Open(stream);
							if (document != null)
							{
								var picture = SvgExtensions.ToModel(document, _assetLoader, out _, out _);
								var svgsrc = new SvgSource() { Picture = picture };
								border.Child = new Image
								{
									Source = new VectorImage { Source = svgsrc }
								};
							}
							else
							{
								border.Child = CreateErrorControl("Failed to process SVG.");
							}
						}
						catch (Exception ex)
						{
							border.Child = CreateErrorControl($"Error parsing SVG: {ex.Message}");
						}
					});
				}
				catch (Exception ex)
				{
					Dispatcher.UIThread.Post(() => border.Child = CreateErrorControl($"Error: {ex.Message}"));
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

				Dispatcher.UIThread.Post(() =>
				{
					if (borderContext.Child is TextBlock tb) tb.Text = "Downloading internal browser engine (one-time setup)...";
				});

				// Download browser to a stable location in LocalAppData to avoid bin folder issues
				var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				var downloadPath = Path.Combine(appData, "MarkdownAvalonia", "Chromium");

				var browserFetcher = new BrowserFetcher(new BrowserFetcherOptions { Path = downloadPath });
				var revisionInfo = await browserFetcher.DownloadAsync();

				Dispatcher.UIThread.Post(() =>
				{
					if (borderContext.Child is TextBlock tb) tb.Text = $"Launching browser engine... ({revisionInfo.GetExecutablePath()})";
				});

				_browser = await Puppeteer.LaunchAsync(new LaunchOptions
				{
					Headless = true,
					ExecutablePath = revisionInfo.GetExecutablePath(),
					Args = new[] {
						"--no-sandbox",
						"--disable-setuid-sandbox",
						"--disable-gpu",
						"--disable-dev-shm-usage"
					}
				});

				return _browser;
			}
			catch (Exception ex)
			{
				Dispatcher.UIThread.Post(() => borderContext.Child = CreateErrorControl($"Failed to launch browser: {ex.Message}"));
				return null;
			}
			finally
			{
				_browserLock.Release();
			}
		}

		private async Task<string> RenderMermaidAsync(IBrowser browser, string code, string theme, string bgColor)
		{
			using var page = await browser.NewPageAsync();

			string htmlCrumbs = $@"
				<!DOCTYPE html>
				<html>
					<head>
						<meta charset='utf-8'>
						<meta name='viewport' content='width=device-width, initial-scale=1'>
						<title>Mermaid Renderer</title>
						<script src='https://cdn.jsdelivr.net/npm/mermaid@10.9.5/dist/mermaid.min.js'></script>
					</head>
					<body>
						<div id='graphDiv'></div>
					</body>
				</html>";

			await page.SetContentAsync(htmlCrumbs);

			// Wait for mermaid to load
			await page.WaitForFunctionAsync("() => typeof mermaid !== 'undefined'");

			// Now eval
			return await page.EvaluateFunctionAsync<string>(@"
			async (code, theme, bgColor) => {
                try {
                    mermaid.initialize({ startOnLoad: true, theme: theme, securityLevel: 'loose', suppressErrors: true, flowchart: { useMaxWidth: false, htmlLabels: true }});
                    const { svg } = await mermaid.render('graphDiv', code);
                    return svg;
                } catch (e) {
                    return 'ERROR: ' + e.message;
                }
            }", code, theme, bgColor);
		}

		private Control CreateErrorControl(string message)
		{
			return new TextBlock { Text = message, Foreground = Brushes.Red, TextWrapping = TextWrapping.Wrap };
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
