# MdXaml #

MdXaml is a modify version of Markdown.Xaml.
Markdown XAML is a port of the popular *MarkdownSharp* Markdown processor, but 
with one very significant difference: Instead of rendering to a string 
containing HTML, it renders to a FlowDocument suitable for embedding into a 
WPF window or usercontrol.

## Features ##

MarkDown.Xaml has a number of convenient features

* The engine itself is a single file, for easy inclusion in your own projects
* Code for the engine is structured in the same manner as the original MarkdownSharp  
If there are any bug fixes to the regular expressions in MarkdownSharp, merging those fixes in the Markdown.Xaml should be straightforward
* Includes a `TextToFlowDocumentConverter` to make it easy to bind Markdown text


## Markdown capabilities and customizables styles ##

* Links [Go to Google!](https://www.google.com)
* Links with title [Go to Google!](https://www.google.com "google.")
* Remote images

![image1](http://placehold.it/350x150)

![imageleft](http://placehold.it/100x150/0000FF "blue")![imageright](http://placehold.it/100x150/00FFFF "cyan")

* Local images

![localimage](LocalPath.png)

* Resource image

![ResourceImage](Asset/ResourceImage.png)

* Table

table begin string
|a|b|c|d|
|:-:|:-|-:|
|a1234567890|b1234567890|c1234567890|d1234567890|
|a|/2.b|c|d|
|A|\2.C|
|1|2|3|4|
|あ|い|う|え|
table end string

* Code

Markdown.Xaml support ```inline code ``` and block code.

```c
#include <stdio.h>
int main()
{
   // printf() displays the string inside quotation
   printf("Hello, World!");
   return 0;
}
```

* Separator
***

* Blockquote

> ## Features ##
> MarkDown.Xaml has a number of convenient features
> 
> * The engine itself is a single file, for easy inclusion in your own projects
> * Code for the engine is structured in the same manner as the original MarkdownSharp  
> * Includes a `TextToFlowDocumentConverter` to make it easy to bind Markdown text


## What is this Demo? ##

This demo application shows MdXaml in use - as you make changes to the 
left pane, the rendered MarkDown will appear in the right pane.

### Source ###

Review the source for this demo to see how MdXaml works in practice, how to use the MarkdownScrollViewer,
and how to style the output to appear the way you desire.

