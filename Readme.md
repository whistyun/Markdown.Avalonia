# Markdown XAML

## What is it?

Markdown XAML is a port of the popular 
[MarkdownSharp](http://code.google.com/p/markdownsharp/) Markdown processor, but with one very 
significant difference: Instead of rendering to a string containing HTML, it renders to a 
[FlowDocument](http://msdn.microsoft.com/en-us/library/system.windows.documents.flowdocument.aspx) 
suitable for embedding into a WPF window or usercontrol.



## Where would I use this?

I wrote this to use in a WPF application where I was generating paragraphs of text for that 
described the output of a rules engine, and I wanted a richer display than just a column of plain 
text.

Potentially, I could have used MarkdownSharp and an embeded browser or other HTML renderer to 
achieve this (the route taken by [MarkPad](http://code52.org/DownmarkerWPF/), but this didn't 
give me the fine control over appearance that I desired.



## Where shouldn't I use this?

If the Markdown you are processing is going to end up translated to HTML, stick with 
MarkdownSharp or one of the other similar translators, so that your rendering is as accurate as 
possible. On the otherhand, if you are showing the Markdown within your WPF application and not
passing it out to a browser elsewhere, Markdown XAML may be a great fit.



## What differences are there?

Since the output is not HTML, any embedded HTML is going to end up displayed as raw code. This 
also means that there's no way to bypass (or tunnel through) the Markdown engine to achieve 
anything not supported by Markdown directly. Depending on your context this may or may not be a
significant issue.


## License

Markdown XAML is licensed under the MIT license.