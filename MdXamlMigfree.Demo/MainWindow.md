# MdXaml #

MdXaml is a modify version of Markdown.Xaml.
Markdown XAML is a port of the popular *MarkdownSharp* Markdown processor, but 
with one very significant difference: Instead of rendering to a string 
containing HTML, it renders to a FlowDocument suitable for embedding into a 
WPF window or usercontrol.

## Features ##

MdXaml has a number of convenient features

* The engine itself is a single file, for easy inclusion in your own projects
* Includes a `MarkdownScrollViewer` to make it easy to bind Markdown text
* MdXaml has some original enhances idiom base on textile.

## The Example Of ... ##

### Text decolation [included original enhance] ###
*italic*, **bold**, ***bold-italic***, ~~strikethrough~~, __underline__ and %{color:red}color%.  
%{color:blue}***~~__Mixing Text__~~***%

### Link ###
Links [Go to Google!](https://www.google.com)  
Links with title [Go to Google!](https://www.google.com "google.")

### Image ###
#### Remote images ####
![image1](http://placehold.it/300x25)  
![imageleft](http://placehold.it/150x25/0000FF "blue")![imageright](http://placehold.it/150x25/00FFFF "cyan")
#### Local and resource images ####
![localimage](LocalPath.png)
![ResourceImage](Asset/ResourceImage.png)

### List ###
#### ul
* one
* two

#### ol
1. one
2. two
#### alphabet-ol [original enhance]
a. one
b. two

#### roman-ol [original enhance]
i, one
ii, two

### Table [included original enhance] ###
|a|b|c|d|
|:-:|:-|-:|
|a1234567890|b1234567890|c1234567890|d1234567890|
|a|/2.b|c|d|
|A|\2.C|
|1|2|3|4|
|あ|い|う|え|

### Code ###
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

### Separator ###
***

### Blockquote ###
> ## Features ##
> MarkDown.Xaml has a number of convenient features
> 
> * The engine itself is a single file, for easy inclusion in your own projects
> * Code for the engine is structured in the same manner as the original MarkdownSharp  
> * Includes a `TextToFlowDocumentConverter` to make it easy to bind Markdown text

### Text Alignment [original enhance] ###
MdXaml parse a head of paragraph. If 'p[<=>].' is found, apply text alignment to it.
> p<. left alignment text
> 
> p>. right alignment text
> 
> p=. center alignment


## What is this Demo? ##

This demo application shows MdXaml in use - as you make changes to the 
left pane, the rendered MarkDown will appear in the right pane.

### Source ###

Review the source for this demo to see how MdXaml works in practice, how to use the MarkdownScrollViewer,
and how to style the output to appear the way you desire.

