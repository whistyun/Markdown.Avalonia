# Markdown.Xaml #

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
* Remote images

![image1](http://placehold.it/350x150)

![imageleft](http://placehold.it/100x150/0000FF)![imageright](http://placehold.it/100x150/00FFFF)

* Local images

![localimage](sampleimage.jpg)

* Table

table begin string
|a|b|c|d|
|:-:|:-|-:|
|a1234567890|b1234567890|c1234567890|d1234567890|
|a|b|c|d|
table end string

* Separator
***


## What is this Demo? ##

This demo application shows MarkDown.Xaml in use - as you make changes to the 
left pane, the rendered MarkDown will appear in the right pane.

### Source ###

Review the source for this demo to see how MarkDown.Xaml works in practice, how to use the TextToFlowDocumentConverter,
and how to style the output to appear the way you desire.


