# Render markdown in Control

MdXaml provide MarkdownScrollViewer which is the control to view markdown from string.
If you want flowdocument which is converted from markdown, use MarkdownXamlConverter.

## MarkdownScrollViewer

### Use Code-behind

You can download the full-example from [here](./render_example_codebehind.zip).

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

### Use Binding

You can download the full-example from [here](./render_example_binding.zip)

**MainWindow.xaml**
```xml
<Window x:Class="render_example_binding.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:mdxam="clr-namespace:MdXaml;assembly=MdXaml"
        xmlns:local="clr-namespace:render_example_binding"
        mc:Ignorable="d"
        Title="MainWindow" Height="450" Width="800">

    <Window.DataContext>
        <local:MainWindowViewModel/>
    </Window.DataContext>

    <mdxam:MarkdownScrollViewer
            Markdown="{Binding MarkdownDoc}"/>
</Window>
```

**MainWindowViewModel.cs**
```cs
using System.IO;

namespace render_example_binding
{
    class MainWindowViewModel
    {

        public MainWindowViewModel()
        {
            MarkdownDoc = File.ReadAllText("SampleMarkdown.md");
        }

        public string MarkdownDoc { get; }
    }
}
```


## MarkdownXamlConverter

If you want only FlowDocument object but don't want visual element, you can use MarkdownXamlConverter.

```xml
<UserControl x:Class="MdXamlSample.SampleControl"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:mdxam="clr-namespace:MdXaml;assembly=MdXaml"
        xmlns:local="clr-namespace:MdXamlSample">

    <UserControl.Resources>
        <mdxam:TextToFlowDocumentConverter x:Key="MdConverter"/>
    </UserControl.Resources>

    <UserControl.DataContext>
        <local:SampleControlViewModel/>
    </UserControl.DataContext>

    <FlowDocumentScrollViewer 
            Markdown="{Binding 
                Path=MarkdownDoc, 
                Converter={StaticResource MdConverter}
            }"
            />

</UserControl>
```
## change render style

You can change a markdown style with 
MarkdownScrollViewer.MarkdownStyle or with Markdown.DocumentStyle

**with MarkdownScrollViewer.MarkdownStyle**
```xml
    <mdxam:MarkdownScrollViewer
            MarkdownStyle="{Binding HugaStyleProperty}"
            Markdown="{Binding MarkdownDoc}"
            />
```

**with Markdown.DocumentStyle**
```xml
    <UserControl.Resources>

        <mdxam:Markdown
                x:Key="MdEngine"
                DocumentStyle="{Binding HugaStyleProperty}"
                />

        <mdxam:TextToFlowDocumentConverter
                x:Key="MdConverter"
                Markdown="{StaticResource MdEngine}"
                />

    </UserControl.Resources>
```
