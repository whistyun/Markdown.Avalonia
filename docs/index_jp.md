# MdXaml

MdXamlは、Markdown.Xamlをフォークし、独自に編集を加えたものです。
Markdown.Xamlと同様に、Markdown形式の文字列からFlowDocumentを生成します。

## 概要

Markdown.Xamlについては、[本家を参照してください。](https://github.com/theunrepentantgeek/Markdown.XAML).

MdXamlは[MarkdownSharp](http://code.google.com/p/markdownsharp/)のようなMarkdown処理系の一つです。一点違うのは、HTMLではなく、[FlowDocument](http://msdn.microsoft.com/en-us/library/system.windows.documents.flowdocument.aspx)を出力します。これは、WPF上で表示するのに適しています。

MdXamlでは、Markdown.Xamlの機能に加えて

* Xaml上でのスタイル指定を1行で纏められるように
* リソースからの画像ファイル読み込み
* Markdownに下記の独自拡張を実装
    * table-colspan/rowspan
    * other list mark(alphabet, italic)
    * テキストデコレーション(取り消し線、下線、色付き)
    * 左揃え、中央揃え、右揃え

## 例

自身で試したい場合は、[デモアプリ](MdXaml_Demo.zip)をダウンロードしてください。

### table
![table-fowdoc.png    ](img/table-fowdoc.png)

### list
![list-flowdoc.png    ](img/list-flowdoc.png)

### text-decoration and text-align
![textdeco-flowdoc.png](img/textdeco-flowdoc.png)

## Nuget

1. [https://www.nuget.org/packages/MdXaml/](https://www.nuget.org/packages/MdXaml/)
2. [https://www.nuget.org/packages/MdXaml_migfree/](https://www.nuget.org/packages/MdXaml_migfree/)

2番目のリンクのパッケージは、namespaceをMarkdown.Xamlとおなじようにしています。Markdown.Xamlから移行を直ぐに行いたい場合はこちらを使用してください。


## サンプルコード

### Stringから、FlowDocumentに変換

```cs
// using MdXaml;
// using System.Windows.Documents;

Markdown engine = new Markdown();

string markdownTxt = System.IO.File.ReadAllText("example.md");

FlowDocument document = engine.Transform(markdownTxt);
```

### Control上に描画

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

    <!--
    <mdxam:MarkdownScrollViewer
            MarkdownStyle="{Binding DocStyle}"
            Markdown="{Binding MarkdownDoc}"
            />
    -->

    <mdxam:MarkdownScrollViewer
            Markdown="{Binding MarkdownDoc}"
            />
</UserControl>
```

**SampleControlViewModel.cs**
```cs
namespace MdXamlSample {
    class SampleControlViewModel{
        // public Style DocStyle {get; set;}
        public string MarkdownDoc {get; set;}
    }
}
```


## ライセンス

MdXaml is licensed under the MIT license.