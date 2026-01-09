This package add Mermaid diagram support to Markdown.

## **How to use**  
In XAML, add MermaidBlockHandler to MdAvPlugins.

```xml
<Window 
	xmlns:mermaid="clr-namespace:Markdown.Avalonia.Mermaid;assembly=Markdown.Avalonia.Mermaid"
>

...another xaml...

<md:MarkdownScrollViewer>
  <md:MarkdownScrollViewer.Plugins>
	<md:MdAvPlugins>
	  <mermaid:MermaidBlockHandler />
	</md:MdAvPlugins>
  </md:MarkdownScrollViewer.Plugins>
</md:MarkdownScrollViewer>
```

## theme support
Mermaid diagrams support light and dark theme. You need to set Theme property in MermaidBlockHandler.

```xml
<mermaid:MermaidBlockHandler Theme="dark" />
```
You can also set BackgroundColor property to make the background transparent.
```xml
<mermaid:MermaidBlockHandler BackgroundColor="transparent" />
```

## In C#
```csharp
var viewer = new MarkdownViewer();
viewer.Extensions.Add(new MarkdownAvalonia.Mermaid.MermaidExtension());
```

## ⚠️ First Run Requirement
The first time a Mermaid diagram is rendered, the library will automatically download a standalone Chromium browser instance (managed by PuppeteerSharp). This one-time setup requires an active internet connection and may take a few moments depending on your network speed.

## Markdown example
### 
	```mermaid
	graph TD
		A[Start] --> B{Decision}
		B -->|Yes| C[Do X]
		B -->|No| D[Do Y]
		C --> E[End]
		D --> E
	``` 
###
dependencies:
- Markdown.Avalonia.Tight
- Markdown.Avalonia.Svg
- PuppeteerSharp