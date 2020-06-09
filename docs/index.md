# MdXaml

MdXaml is a modify version of Markdown.Xaml.
It can convert Markdown to FlowDocument of WPF.
[(for japanese|日本語)](index_jp.md)

## What are Markdown.Xaml and MdXaml ?

About Markdown.Xaml, [see the origin project page](https://github.com/theunrepentantgeek/Markdown.XAML).

Markdown.Xaml( and MdXaml ) is a port of the popular 
[MarkdownSharp](http://code.google.com/p/markdownsharp/) Markdown processor, but with one very 
significant difference: Instead of rendering to a string containing HTML, it renders to a 
[FlowDocument](http://msdn.microsoft.com/en-us/library/system.windows.documents.flowdocument.aspx) 
suitable for embedding into a WPF window or usercontrol.

Additionary, MdXaml add the bellow.

* Put some styles that are scattered in Markdown.Xaml together.
* Embedded Image resource.
* Custom extension of Markdown
    * table-colspan/rowspan
    * other list mark(alphabet, italic)
    * text-decoration(strikethrough, underline, color)
    * text-align

## Sample

If you want to try yourself, please download [Demo-application](MdXaml_Demo.zip) and execute it.

![sc1](img.demo/sc1.png)
![sc2](img.demo/sc2.png)
![sc3](img.demo/sc3.png)
![sc4](img.demo/sc4.png)
![sc5](img.demo/sc5.png)

## Nuget

1. [https://www.nuget.org/packages/MdXaml/](https://www.nuget.org/packages/MdXaml/)
2. [https://www.nuget.org/packages/MdXaml_migfree/](https://www.nuget.org/packages/MdXaml_migfree/)

If you want to change dependency library from Markdown.Xaml to MdXaml quickly. use the second link. It keeps the namespace to "Markdown.Xaml".

## Quick start 

### Transform markdown to flowdocument

```cs
// using MdXaml;
// using System.Windows.Documents;

Markdown engine = new Markdown();

string markdownTxt = System.IO.File.ReadAllText("example.md");

FlowDocument document = engine.Transform(markdownTxt);
```

### Render markdown in Control

**MainWindow.xaml**
```xml
<Window x:Class="render_example_codebehind.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:mdxam="clr-namespace:MdXaml;assembly=MdXaml"
        xmlns:local="clr-namespace:render_example_codebehind"
        mc:Ignorable="d"
        Title="MainWindow" Height="450" Width="800">

    <mdxam:MarkdownScrollViewer x:Name="Markdownview"/>
</Window>
```

**MainWindow.xaml.cs**
```cs
using System.IO;
using System.Windows;

namespace render_example_codebehind
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ReadMarkdownAndSetViewer();
        }


        private void ReadMarkdownAndSetViewer()
        {
            Markdownview.Markdown = File.ReadAllText("SampleMarkdown.md");
        }
    }
}
```

### More document
* [Render markdown in Control](render_markdown_in_control.md)
* [Image reading priority order](image_load_priority.md)
* [enhance(list, table, text-decoration)](original_enhance.md)

## License

MdXaml is licensed under the MIT license.