# Code sample

## Convert markdown to FlowDocument

Use Markdown.Transform

```cs
// using System.Windows.Documents;
// using Markdown.Xaml;

/** plain flow document: without style */
FlowDocument ConvertMarkdownToFlowDocument(string markdownTxt)
{
    var md = new Markdown.Xaml.Markdown();
    FlowDocument doc =  md.Transform(markdownTxt);
    return doc;
}

/** styled flow document */
FlowDocument ConvertMarkdownToStyledFlowDocument(string markdownTxt)
{
    var md = new Markdown.Xaml.Markdown();
    md.DocumentStyle = MarkdownStyle.Compact;
    FlowDocument doc = md.Transform(markdownTxt);
    return doc;
}
```

## View markdown on your app.

### Xaml
```xml
<UserControl x:Class="foo.View"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:local="clr-namespace:foo"
        xmlns:md="clr-namespace:Markdown.Xaml;assembly=Markdown.Xaml">

    <UserControl.DataContext>
        <local:ViewModel/>
    </UserControl.DataContext>

    <UserControl.Resources>
        <md:Markdown
            x:Key="Markdown"
            DocumentStyle="{x:Static md:MarkdownStyle.Standard}"/>

        <md:TextToFlowDocumentConverter
            x:Key="MarkdownConverter" 
            Markdown="{StaticResource Markdown}"/>
    </UserControl.Resources>

    <FlowDocumentScrollViewer
        VerticalAlignment="Stretch"
        HorizontalAlignment="Stretch"
        VerticalScrollBarVisibility="Auto"
        Document="{Binding MdText, Converter={StaticResource MarkdownConverter}}"/>
</UserControl>
```

### ViewModel
```cs
//using System.ComponentModel;
//using System.Runtime.CompilerServices;

class ViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void FirePropertyChanged([CallerMemberName]string propertyName = null)
    {
        var handler = PropertyChanged;
        if (handler != null)
            handler(this, new PropertyChangedEventArgs(propertyName));
    }

    private string _mdText;
    public string MdText
    {
        get { return _mdText; }
        set
        {
            if (_mdText == value) return;
            _mdText = value;
            FirePropertyChanged();
        }
    }
}
```