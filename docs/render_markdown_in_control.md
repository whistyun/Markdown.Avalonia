# Render markdown in Control


## Redner on FlowDocumentScrollViewer

### Case1. Use SampleControlViewModel

MdXaml provide MarkdownScrollViewer which is sample using FlowDocumentScrollViewer.

See MdXaml.Demo/MainWindow.xaml. It is the full sample using FlowDocumentScrollViewer.

**SampleControl.xaml**
```xml
<UserControl x:Class="MdXamlSample.SampleControl"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:mdxam="clr-namespace:MdXaml;assembly=MdXaml"
        xmlns:local="clr-namespace:MdXamlSample">

    <UserControl.DataContext>
        <local:SampleControlViewModel/>
    </UserControl.DataContext>

    <mdxam:MarkdownScrollViewer
            Markdown="{Binding MarkdownDoc}"
            />
</UserControl>
```

**SampleControlViewModel.cs**
```cs
namespace MdXamlSample {
    class SampleControlViewModel{
        public string MarkdownDoc {get; set;}
    }
}
```

### Case2. Use MarkdownXamlConverter

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
