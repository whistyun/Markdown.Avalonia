using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

#if !MIG_FREE
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
#endif

#if MIG_FREE
namespace Markdown.Xaml
#else
namespace MdXaml
#endif
{
    public class Markdown : DependencyObject, IUriContext
    {
        #region const
        /// <summary>
        /// maximum nested depth of [] and () supported by the transform; implementation detail
        /// </summary>
        private const int _nestDepth = 6;

        /// <summary>
        /// Tabs are automatically converted to spaces as part of the transform  
        /// this constant determines how "wide" those tabs become in spaces  
        /// </summary>
        private const int _tabWidth = 4;

        private const string TagHeading1 = "Heading1";
        private const string TagHeading2 = "Heading2";
        private const string TagHeading3 = "Heading3";
        private const string TagHeading4 = "Heading4";
        private const string TagCode = "CodeSpan";
        private const string TagCodeBlock = "CodeBlock";
        private const string TagBlockquote = "Blockquote";
        private const string TagNote = "Note";
        private const string TagTableHeader = "TableHeader";
        private const string TagTableBody = "TableBody";
        private const string TagOddTableRow = "OddTableRow";
        private const string TagEvenTableRow = "EvenTableRow";

        private const string TagBoldSpan = "Bold";
        private const string TagItalicSpan = "Italic";
        private const string TagStrikethroughSpan = "Strikethrough";
        private const string TagUnderlineSpan = "Underline";

        #endregion

        /// <summary>
        /// when true, bold and italic require non-word characters on either side  
        /// WARNING: this is a significant deviation from the markdown spec
        /// </summary>
        public bool StrictBoldItalic { get; set; }

        public bool DisabledTag { get; set; }

        public bool DisabledTootip { get; set; }

        public bool DisabledLazyLoad { get; set; }

        public string AssetPathRoot { get; set; }

        public ICommand HyperlinkCommand { get; set; }

        public Uri BaseUri { get; set; }

        #region dependencyobject property

        // Using a DependencyProperty as the backing store for DocumentStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DocumentStyleProperty =
            DependencyProperty.Register(nameof(DocumentStyle), typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        /// <summary>
        /// top-level flow document style
        /// </summary>
        public Style DocumentStyle
        {
            get { return (Style)GetValue(DocumentStyleProperty); }
            set { SetValue(DocumentStyleProperty, value); }
        }

        #endregion


        #region legacy property

        public Style Heading1Style { get; set; }
        public Style Heading2Style { get; set; }
        public Style Heading3Style { get; set; }
        public Style Heading4Style { get; set; }
        public Style NormalParagraphStyle { get; set; }
        public Style CodeStyle { get; set; }
        public Style CodeBlockStyle { get; set; }
        public Style BlockquoteStyle { get; set; }
        public Style LinkStyle { get; set; }
        public Style ImageStyle { get; set; }
        public Style SeparatorStyle { get; set; }
        public Style TableStyle { get; set; }
        public Style TableHeaderStyle { get; set; }
        public Style TableBodyStyle { get; set; }
        public Style NoteStyle { get; set; }

        #endregion


        #region regex pattern


        #endregion

        public Markdown()
        {
            HyperlinkCommand = NavigationCommands.GoToPage;
            AssetPathRoot = Environment.CurrentDirectory;
        }

        public FlowDocument Transform(string text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            text = Normalize(text);
            var document = Create<FlowDocument, Block>(RunBlockGamut(text, true));

            document.SetBinding(FlowDocument.StyleProperty, new Binding(DocumentStyleProperty.Name) { Source = this });

            return document;
        }

        /// <summary>
        /// convert all tabs to _tabWidth spaces; 
        /// standardizes line endings from DOS (CR LF) or Mac (CR) to UNIX (LF); 
        /// makes sure text ends with a couple of newlines; 
        /// removes any blank lines (only spaces) in the text
        /// </summary>
        private string Normalize(string text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            var output = new StringBuilder(text.Length);
            var line = new StringBuilder();
            bool valid = false;

            for (int i = 0; i < text.Length; i++)
            {
                switch (text[i])
                {
                    case '\n':
                        if (valid)
                            output.Append(line);
                        output.Append('\n');
                        line.Length = 0;
                        valid = false;
                        break;
                    case '\r':
                        if ((i < text.Length - 1) && (text[i + 1] != '\n'))
                        {
                            if (valid)
                                output.Append(line);
                            output.Append('\n');
                            line.Length = 0;
                            valid = false;
                        }
                        break;
                    case '\t':
                        int width = (_tabWidth - line.Length % _tabWidth);
                        for (int k = 0; k < width; k++)
                            line.Append(' ');
                        break;
                    case '\x1A':
                        break;
                    default:
                        if (!valid && text[i] != ' ')
                            valid = true;
                        line.Append(text[i]);
                        break;
                }
            }

            if (valid)
                output.Append(line);
            output.Append('\n');

            // add two newlines to the end before return
            return output.Append("\n\n").ToString();
        }

        /// <summary>
        /// Perform transformations that form block-level tags like paragraphs, headers, and list items.
        /// </summary>
        private IEnumerable<Block> RunBlockGamut(string text, bool supportTextAlignment)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return
                DoCodeBlocks(text,
                    s1 => DoBlockquotes(s1,
                    s2 => DoHeaders(s2,
                    s3 => DoHorizontalRules(s3,
                    s4 => DoLists(s4,
                    s5 => DoTable(s5,
                    s6 => DoNote(s6, supportTextAlignment,
                    sn => FormParagraphs(sn, supportTextAlignment
                    ))))))));

            //text = DoCodeBlocks(text);
            //text = DoBlockQuotes(text);

            //// We already ran HashHTMLBlocks() before, in Markdown(), but that
            //// was to escape raw HTML in the original Markdown source. This time,
            //// we're escaping the markup we've just created, so that we don't wrap
            //// <p> tags around block-level tags.
            //text = HashHTMLBlocks(text);

            //text = FormParagraphs(text);

            //return text;
        }

        /// <summary>
        /// Perform transformations that occur *within* block-level tags like paragraphs, headers, and list items.
        /// </summary>
        private IEnumerable<Inline> RunSpanGamut(string text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return DoCodeSpans(text,
                s0 => DoImages(s0,
                s1 => DoAnchors(s1,
                s2 => DoTextDecorations(s2,
                s3 => DoText(s3)))));

            //text = EscapeSpecialCharsWithinTagAttributes(text);
            //text = EscapeBackslashes(text);

            //// Images must come first, because ![foo][f] looks like an anchor.
            //text = DoImages(text);
            //text = DoAnchors(text);

            //// Must come after DoAnchors(), because you can use < and >
            //// delimiters in inline links like [this](<url>).
            //text = DoAutoLinks(text);

            //text = EncodeAmpsAndAngles(text);
            //text = DoItalicsAndBold(text);
            //text = DoHardBreaks(text);

            //return text;
        }


        #region grammer - paragraph

        private static readonly Regex _align = new Regex(@"^p([<=>])\.", RegexOptions.Compiled);
        private static readonly Regex _newlinesLeadingTrailing = new Regex(@"^\n+|\n+\z", RegexOptions.Compiled);
        private static readonly Regex _newlinesMultiple = new Regex(@"\n{2,}", RegexOptions.Compiled);

        /// <summary>
        /// splits on two or more newlines, to form "paragraphs";    
        /// </summary>
        private IEnumerable<Block> FormParagraphs(string text, bool supportTextAlignment)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            // split on two or more newlines
            string[] grafs = _newlinesMultiple.Split(_newlinesLeadingTrailing.Replace(text, ""));

            foreach (var g in grafs)
            {
                var chip = g;

                TextAlignment? indiAlignment = null;

                if (supportTextAlignment)
                {
                    var alignMatch = _align.Match(chip);
                    if (alignMatch.Success)
                    {
                        chip = chip.Substring(alignMatch.Length);
                        switch (alignMatch.Groups[1].Value)
                        {
                            case "<":
                                indiAlignment = TextAlignment.Left;
                                break;
                            case ">":
                                indiAlignment = TextAlignment.Right;
                                break;
                            case "=":
                                indiAlignment = TextAlignment.Center;
                                break;
                        }
                    }
                }

                var block = Create<Paragraph, Inline>(RunSpanGamut(chip));
                if (NormalParagraphStyle != null)
                {
                    block.Style = NormalParagraphStyle;
                }
                if (indiAlignment.HasValue)
                {
                    block.TextAlignment = indiAlignment.Value;
                }

                yield return block;
            }
        }

        #endregion


        #region grammer - image

        private static readonly Regex _imageInline = new Regex(string.Format(@"
                (                           # wrap whole match in $1
                    !\[
                        ({0})               # link text = $2
                    \]
                    \(                      # literal paren
                        [ ]*
                        ({1})               # href(with title) = $3
                        [ ]*
                    \)
                )", GetNestedBracketsPattern(), GetNestedParensPatternWithWhiteSpace()),
                  RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private static readonly Regex _imageHrefWithTitle = new Regex(@"^
                (                           # wrap whole match in $1
                    (.+?)                   # url = $2
                    [ ]+
                    (['""])                 # quote char = $3
                    (.*?)                   # title = $4
                    \3
                )$", RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        /// <summary>
        /// Turn Markdown images into images
        /// </summary>
        /// <remarks>
        /// ![image alt](url) 
        /// </remarks>
        private IEnumerable<Inline> DoImages(string text, Func<string, IEnumerable<Inline>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return Evaluate(text, _imageInline, ImageInlineEvaluator, defaultHandler);
        }


        private Inline ImageInlineEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            string linkText = match.Groups[2].Value;
            string url = match.Groups[3].Value;
            string title = null;

            var titleMatch = _imageHrefWithTitle.Match(url);
            if (titleMatch.Success)
            {
                url = titleMatch.Groups[2].Value;
                title = titleMatch.Groups[4].Value;
            }

            BitmapImage imgSource = null;

            // check embedded resoruce
            try
            {
                Uri packUri;
                if (!Uri.IsWellFormedUriString(url, UriKind.Absolute) && BaseUri != null)
                {
                    packUri = new Uri(BaseUri, url);
                }
                else
                {
                    packUri = new Uri(url);
                }

                imgSource = MakeImage(packUri);
            }
            catch { }

            // check filesystem
            if (imgSource is null)
            {
                try
                {
                    if (!Uri.IsWellFormedUriString(url, UriKind.Absolute) && !System.IO.Path.IsPathRooted(url))
                    {
                        url = System.IO.Path.Combine(AssetPathRoot ?? string.Empty, url);
                    }

                    imgSource = MakeImage(new Uri(url, UriKind.RelativeOrAbsolute));
                }
                catch { }
            }

            // error
            if (imgSource is null)
            {
                return new Run("!" + url) { Foreground = Brushes.Red };
            }


            Image image = new Image { Source = imgSource, Tag = linkText };
            if (ImageStyle is null)
            {
                image.Margin = new Thickness(0);
            }
            else
            {
                image.Style = ImageStyle;
            }
            if (!DisabledTootip && !string.IsNullOrWhiteSpace(title))
            {
                image.ToolTip = title;
            }

            // Bind size so document is updated when image is downloaded
            if (imgSource.IsDownloading)
            {
                Binding binding = new Binding(nameof(BitmapImage.Width));
                binding.Source = imgSource;
                binding.Mode = BindingMode.OneWay;

                BindingExpressionBase bindingExpression = BindingOperations.SetBinding(image, Image.WidthProperty, binding);
                EventHandler downloadCompletedHandler = null;
                downloadCompletedHandler = (sender, e) =>
                {
                    imgSource.DownloadCompleted -= downloadCompletedHandler;
                    imgSource.Freeze();
                    bindingExpression.UpdateTarget();
                };
                imgSource.DownloadCompleted += downloadCompletedHandler;
            }
            else
            {
                image.Width = imgSource.Width;
            }

            return new InlineUIContainer(image);
        }

        private BitmapImage MakeImage(Uri url)
        {
            if (DisabledLazyLoad)
            {
                return new BitmapImage(url);
            }
            else
            {
                var imgSource = new BitmapImage();
                imgSource.BeginInit();
                imgSource.CacheOption = BitmapCacheOption.None;
                imgSource.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
                imgSource.CacheOption = BitmapCacheOption.OnLoad;
                imgSource.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                imgSource.UriSource = url;
                imgSource.EndInit();

                return imgSource;
            }
        }

        #endregion


        #region grammer - anchor

        private static readonly Regex _anchorInline = new Regex(string.Format(@"
                (                           # wrap whole match in $1
                    \[
                        ({0})               # link text = $2
                    \]
                    \(                      # literal paren
                        [ ]*
                        ({1})               # href = $3
                        [ ]*
                        (                   # $4
                        (['""])             # quote char = $5
                        (.*?)               # title = $6
                        \5                  # matching quote
                        [ ]*                # ignore any spaces between closing quote and )
                        )?                  # title is optional
                    \)
                )", GetNestedBracketsPattern(), GetNestedParensPattern()),
                  RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        /// <summary>
        /// Turn Markdown link shortcuts into hyperlinks
        /// </summary>
        /// <remarks>
        /// [link text](url "title") 
        /// </remarks>
        private IEnumerable<Inline> DoAnchors(string text, Func<string, IEnumerable<Inline>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            // Next, inline-style links: [link text](url "optional title") or [link text](url "optional title")
            return Evaluate(text, _anchorInline, AnchorInlineEvaluator, defaultHandler);
        }

        private Inline AnchorInlineEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            string linkText = match.Groups[2].Value;
            string url = match.Groups[3].Value;
            string title = match.Groups[6].Value;

            var result = Create<Hyperlink, Inline>(RunSpanGamut(linkText));
            result.Command = HyperlinkCommand;
            result.CommandParameter = url;

            if (!DisabledTootip)
            {
                result.ToolTip = string.IsNullOrWhiteSpace(title) ?
                    url :
                    String.Format("\"{0}\"\r\n{1}", title, url);
            }

            if (LinkStyle != null)
            {
                result.Style = LinkStyle;
            }

            return result;
        }

        #endregion


        #region grammer - header

        private static readonly Regex _headerSetext = new Regex(@"
                ^(.+?)
                [ ]*
                \n
                (=+|-+)     # $1 = string of ='s or -'s
                [ ]*
                \n+",
                RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private static readonly Regex _headerAtx = new Regex(@"
                ^(\#{1,6})  # $1 = string of #'s
                [ ]*
                (.+?)       # $2 = Header text
                [ ]*
                \#*         # optional closing #'s (not counted)
                \n+",
                RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        /// <summary>
        /// Turn Markdown headers into HTML header tags
        /// </summary>
        /// <remarks>
        /// Header 1  
        /// ========  
        /// 
        /// Header 2  
        /// --------  
        /// 
        /// # Header 1  
        /// ## Header 2  
        /// ## Header 2 with closing hashes ##  
        /// ...  
        /// ###### Header 6  
        /// </remarks>
        private IEnumerable<Block> DoHeaders(string text, Func<string, IEnumerable<Block>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return Evaluate<Block>(text, _headerSetext, m => SetextHeaderEvaluator(m),
                s => Evaluate<Block>(s, _headerAtx, m => AtxHeaderEvaluator(m), defaultHandler));
        }

        private Block SetextHeaderEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            string header = match.Groups[1].Value;
            int level = match.Groups[2].Value.StartsWith("=") ? 1 : 2;

            //TODO: Style the paragraph based on the header level
            return CreateHeader(level, RunSpanGamut(header.Trim()));
        }

        private Block AtxHeaderEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            string header = match.Groups[2].Value;
            int level = match.Groups[1].Value.Length;
            return CreateHeader(level, RunSpanGamut(header));
        }

        public Block CreateHeader(int level, IEnumerable<Inline> content)
        {
            if (content is null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            var block = Create<Paragraph, Inline>(content);

            switch (level)
            {
                case 1:
                    if (Heading1Style != null)
                    {
                        block.Style = Heading1Style;
                    }
                    if (!DisabledTag)
                    {
                        block.Tag = TagHeading1;
                    }
                    break;

                case 2:
                    if (Heading2Style != null)
                    {
                        block.Style = Heading2Style;
                    }
                    if (!DisabledTag)
                    {
                        block.Tag = TagHeading2;
                    }
                    break;

                case 3:
                    if (Heading3Style != null)
                    {
                        block.Style = Heading3Style;
                    }
                    if (!DisabledTag)
                    {
                        block.Tag = TagHeading3;
                    }
                    break;

                case 4:
                    if (Heading4Style != null)
                    {
                        block.Style = Heading4Style;
                    }
                    if (!DisabledTag)
                    {
                        block.Tag = TagHeading4;
                    }
                    break;
            }

            return block;
        }
        #endregion

        #region grammer - Note
        private static readonly Regex _note = new Regex(@"
                ^(\<)       # $1 = starting marker <
                [ ]*
                (.+?)       # $2 = Header text
                [ ]*
                \>*         # optional closing >'s (not counted)
                \n+
            ", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        /// <summary>
        /// Turn Markdown into HTML paragraphs.
        /// </summary>
        /// <remarks>
        /// < Note
        /// </remarks>
        private IEnumerable<Block> DoNote(string text, bool supportTextAlignment,
                Func<string, IEnumerable<Block>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return Evaluate<Block>(text, _note,
                m => NoteEvaluator(m, supportTextAlignment),
                defaultHandler);
        }

        private Block NoteEvaluator(Match match, bool supportTextAlignment)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            string text = match.Groups[2].Value;

            TextAlignment? indiAlignment = null;

            if (supportTextAlignment)
            {
                var alignMatch = _align.Match(text);
                if (alignMatch.Success)
                {
                    text = text.Substring(alignMatch.Length);
                    switch (alignMatch.Groups[1].Value)
                    {
                        case "<":
                            indiAlignment = TextAlignment.Left;
                            break;
                        case ">":
                            indiAlignment = TextAlignment.Right;
                            break;
                        case "=":
                            indiAlignment = TextAlignment.Center;
                            break;
                    }
                }
            }

            return NoteComment(RunSpanGamut(text), indiAlignment);
        }

        public Block NoteComment(IEnumerable<Inline> content, TextAlignment? indiAlignment)
        {
            if (content is null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            var block = Create<Paragraph, Inline>(content);
            if (NoteStyle != null)
            {
                block.Style = NoteStyle;
            }
            if (!DisabledTag)
            {
                block.Tag = TagNote;
            }
            if (indiAlignment.HasValue)
            {
                block.TextAlignment = indiAlignment.Value;
            }

            return block;
        }
        #endregion

        #region grammer - horizontal rules

        private static readonly Regex _horizontalRules = HorizontalRulesRegex("-");
        private static readonly Regex _horizontalTwoLinesRules = HorizontalRulesRegex("=");
        private static readonly Regex _horizontalBoldRules = HorizontalRulesRegex("*");
        private static readonly Regex _horizontalBoldWithSingleRules = HorizontalRulesRegex("_");
        private static Regex HorizontalRulesRegex(string markers)
        {
            return new Regex(@"
                ^[ ]{0,3}                   # Leading space
                    ([" + markers + @"])    # $1: First marker ([markers])
                    (?>                     # Repeated marker group
                        [ ]{0,2}            # Zero, one, or two spaces.
                        \1                  # Marker character
                    ){2,}                   # Group repeated at least twice
                    [ ]*                    # Trailing spaces
                    $                       # End of line.
                ", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
        }

        /// <summary>
        /// Turn Markdown horizontal rules into HTML hr tags
        /// </summary>
        /// <remarks>
        /// ***  
        /// * * *  
        /// ---
        /// - - -
        /// </remarks>
        private IEnumerable<Block> DoHorizontalRules(string text, Func<string, IEnumerable<Block>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return Evaluate(text, _horizontalRules, RuleEvaluator,
                s1 => Evaluate(s1, _horizontalTwoLinesRules, TwoLinesRuleEvaluator,
                s2 => Evaluate(s2, _horizontalBoldRules, BoldRuleEvaluator,
                s3 => Evaluate(s3, _horizontalBoldWithSingleRules, BoldWithSingleRuleEvaluator, defaultHandler))));
        }

        /// <summary>
        /// Single line separator.
        /// </summary>
        private Block RuleEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var sep = new Separator();
            if (SeparatorStyle != null)
                sep.Style = SeparatorStyle;

            return new BlockUIContainer(sep);
        }

        /// <summary>
        /// Two lines separator.
        /// </summary>
        private Block TwoLinesRuleEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var stackPanel = new StackPanel();
            for (int i = 0; i < 2; i++)
            {
                var sep = new Separator();
                if (SeparatorStyle != null)
                    sep.Style = SeparatorStyle;

                stackPanel.Children.Add(sep);
            }

            var container = new BlockUIContainer(stackPanel);
            return container;
        }

        /// <summary>
        /// Double line separator.
        /// </summary>
        private Block BoldRuleEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var stackPanel = new StackPanel();
            for (int i = 0; i < 2; i++)
            {
                var sep = new Separator()
                {
                    Margin = new Thickness(0)
                };

                if (SeparatorStyle != null)
                    sep.Style = SeparatorStyle;

                stackPanel.Children.Add(sep);
            }

            var container = new BlockUIContainer(stackPanel);
            return container;
        }

        /// <summary>
        /// Two lines separator consisting of a double line and a single line.
        /// </summary>
        private Block BoldWithSingleRuleEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var stackPanel = new StackPanel();
            for (int i = 0; i < 2; i++)
            {
                var sep = new Separator()
                {
                    Margin = new Thickness(0)
                };

                if (SeparatorStyle != null)
                    sep.Style = SeparatorStyle;

                stackPanel.Children.Add(sep);
            }

            var sepLst = new Separator();
            if (SeparatorStyle != null)
                sepLst.Style = SeparatorStyle;

            stackPanel.Children.Add(sepLst);

            var container = new BlockUIContainer(stackPanel);
            return container;
        }

        #endregion


        #region grammer - list
        private const string _markerUL = @"[*+=-]";
        private const string _markerOL = @"\d+[.]|\p{L}+[.,]";

        // Unordered List
        private const string _markerUL_Disc = @"[*]";
        private const string _markerUL_Box = @"[+]";
        private const string _markerUL_Circle = @"[-]";
        private const string _markerUL_Square = @"[=]";

        // Ordered List
        private const string _markerOL_Number = @"\d+[.]";
        private const string _markerOL_LetterLower = @"\p{Ll}+[.]";
        private const string _markerOL_LetterUpper = @"\p{Lu}+[.]";
        private const string _markerOL_RomanLower = @"\p{Ll}+[,]";
        private const string _markerOL_RomanUpper = @"\p{Lu}+[,]";

        private int _listLevel;

        /// <summary>
        /// Maximum number of levels a single list can have.
        /// In other words, _listDepth - 1 is the maximum number of nested lists.
        /// </summary>
        private const int _listDepth = 6;

        private static readonly string _wholeList = string.Format(@"
            (                               # $1 = whole list
              (                             # $2
                [ ]{{0,{1}}}
                ({0})                       # $3 = first list item marker
                [ ]+
              )
              (?s:.+?)
              (                             # $4
                  \z
                |
                  \n{{2,}}
                  (?=\S)
                  (?!                       # Negative lookahead for another list item marker
                    [ ]*
                    {0}[ ]+
                  )
              )
            )", string.Format("(?:{0}|{1})", _markerUL, _markerOL), _listDepth - 1);

        private static readonly Regex _listNested = new Regex(@"^" + _wholeList,
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private static readonly Regex _listTopLevel = new Regex(@"(?:(?<=\n\n)|\A\n?)" + _wholeList,
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        /// <summary>
        /// Turn Markdown lists into HTML ul and ol and li tags
        /// </summary>
        private IEnumerable<Block> DoLists(string text, Func<string, IEnumerable<Block>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            // We use a different prefix before nested lists than top-level lists.
            // See extended comment in _ProcessListItems().
            if (_listLevel > 0)
                return Evaluate(text, _listNested, ListEvaluator, defaultHandler);
            else
                return Evaluate(text, _listTopLevel, ListEvaluator, defaultHandler);
        }

        private Block ListEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            string list = match.Groups[1].Value;
            string listType = Regex.IsMatch(match.Groups[3].Value, _markerUL) ? "ul" : "ol";

            // Set text marker style.
            TextMarkerStyle textMarker = GetTextMarkerStyle(listType, match);

            // Turn double returns into triple returns, so that we can make a
            // paragraph for the last item in a list, if necessary:
            list = Regex.Replace(list, @"\n{2,}", "\n\n\n");

            var resultList = Create<List, ListItem>(ProcessListItems(list, listType == "ul" ? _markerUL : _markerOL));

            resultList.MarkerStyle = textMarker;

            return resultList;
        }

        /// <summary>
        /// Process the contents of a single ordered or unordered list, splitting it
        /// into individual list items.
        /// </summary>
        private IEnumerable<ListItem> ProcessListItems(string list, string marker)
        {
            // The listLevel global keeps track of when we're inside a list.
            // Each time we enter a list, we increment it; when we leave a list,
            // we decrement. If it's zero, we're not in a list anymore.

            // We do this because when we're not inside a list, we want to treat
            // something like this:

            //    I recommend upgrading to version
            //    8. Oops, now this line is treated
            //    as a sub-list.

            // As a single paragraph, despite the fact that the second line starts
            // with a digit-period-space sequence.

            // Whereas when we're inside a list (or sub-list), that line will be
            // treated as the start of a sub-list. What a kludge, huh? This is
            // an aspect of Markdown's syntax that's hard to parse perfectly
            // without resorting to mind-reading. Perhaps the solution is to
            // change the syntax rules such that sub-lists must start with a
            // starting cardinal number; e.g. "1." or "a.".

            _listLevel++;
            try
            {
                // Trim trailing blank lines:
                list = Regex.Replace(list, @"\n{2,}\z", "\n");

                string pattern = string.Format(
                  @"(\n)?                  # leading line = $1
                (^[ ]*)                    # leading whitespace = $2
                ({0}) [ ]+                 # list marker = $3
                ((?s:.+?)                  # list item text = $4
                (\n{{1,2}}))      
                (?= \n* (\z | \2 ({0}) [ ]+))", marker);

                var regex = new Regex(pattern, RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
                var matches = regex.Matches(list);
                foreach (Match m in matches)
                {
                    yield return ListItemEvaluator(m);
                }
            }
            finally
            {
                _listLevel--;
            }
        }

        private ListItem ListItemEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            string item = match.Groups[4].Value;
            string leadingLine = match.Groups[1].Value;

            if (!String.IsNullOrEmpty(leadingLine) || Regex.IsMatch(item, @"\n{2,}"))
                // we could correct any bad indentation here..
                return Create<ListItem, Block>(RunBlockGamut(item, false));
            else
            {
                // recursion for sub-lists
                return Create<ListItem, Block>(RunBlockGamut(item, false));
            }
        }

        /// <summary>
        /// Get the text marker style based on a specific regex.
        /// </summary>
        /// <param name="listType">Specify what kind of list: ul, ol.</param>
        private static TextMarkerStyle GetTextMarkerStyle(string listType, Match match)
        {
            switch (listType)
            {
                case "ul":
                    if (Regex.IsMatch(match.Groups[3].Value, _markerUL_Disc))
                    {
                        return TextMarkerStyle.Disc;
                    }
                    else if (Regex.IsMatch(match.Groups[3].Value, _markerUL_Box))
                    {
                        return TextMarkerStyle.Box;
                    }
                    else if (Regex.IsMatch(match.Groups[3].Value, _markerUL_Circle))
                    {
                        return TextMarkerStyle.Circle;
                    }
                    else if (Regex.IsMatch(match.Groups[3].Value, _markerUL_Square))
                    {
                        return TextMarkerStyle.Square;
                    }
                    break;
                case "ol":
                    if (Regex.IsMatch(match.Groups[3].Value, _markerOL_Number))
                    {
                        return TextMarkerStyle.Decimal;
                    }
                    else if (Regex.IsMatch(match.Groups[3].Value, _markerOL_LetterLower))
                    {
                        return TextMarkerStyle.LowerLatin;
                    }
                    else if (Regex.IsMatch(match.Groups[3].Value, _markerOL_LetterUpper))
                    {
                        return TextMarkerStyle.UpperLatin;
                    }
                    else if (Regex.IsMatch(match.Groups[3].Value, _markerOL_RomanLower))
                    {
                        return TextMarkerStyle.LowerRoman;
                    }
                    else if (Regex.IsMatch(match.Groups[3].Value, _markerOL_RomanUpper))
                    {
                        return TextMarkerStyle.UpperRoman;
                    }
                    break;
            }
            return TextMarkerStyle.None;
        }

        #endregion


        #region grammer - table

        private static readonly Regex _table = new Regex(@"
            (                               # whole table
                [ \r\n]*
                (?<hdr>                     # table header
                    ([^\r\n\|]*\|[^\r\n]+)
                )
                [ ]*\r?\n[ ]*
                (?<col>                     # column style
                    \|?([ ]*:?-+:?[ ]*(\||$))+
                )
                (?<row>                     # table row
                    (
                        [ ]*\r?\n[ ]*
                        ([^\r\n\|]*\|[^\r\n]+)
                    )+
                )
            )",
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        public IEnumerable<Block> DoTable(string text, Func<string, IEnumerable<Block>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return Evaluate(text, _table, TableEvalutor, defaultHandler);
        }

        private Block TableEvalutor(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var headerTxt = match.Groups["hdr"].Value.Trim();
            var styleTxt = match.Groups["col"].Value.Trim();
            var rowTxt = match.Groups["row"].Value.Trim();

            string ExtractCoverBar(string txt)
            {
                if (txt[0] == '|')
                    txt = txt.Substring(1);

                if (String.IsNullOrEmpty(txt))
                    return txt;

                if (txt[txt.Length - 1] == '|')
                    txt = txt.Substring(0, txt.Length - 1);

                return txt;
            }

            var mdtable = new MdTable(
                ExtractCoverBar(headerTxt).Split('|'),
                ExtractCoverBar(styleTxt).Split('|').Select(txt => txt.Trim()).ToArray(),
                rowTxt.Split('\n').Select(ritm =>
                {
                    var trimRitm = ritm.Trim();
                    return ExtractCoverBar(trimRitm).Split('|');
                }).ToList());

            // table
            var table = new Table();
            if (TableStyle != null)
            {
                table.Style = TableStyle;
            }

            // table columns
            while (table.Columns.Count < mdtable.ColCount)
                table.Columns.Add(new TableColumn());

            // table header
            var tableHeaderRG = new TableRowGroup();
            if (TableHeaderStyle != null)
            {
                tableHeaderRG.Style = TableHeaderStyle;
            }
            if (!DisabledTag)
            {
                tableHeaderRG.Tag = TagTableHeader;
            }

            var tableHeader = CreateTableRow(mdtable.Header);
            tableHeaderRG.Rows.Add(tableHeader);
            table.RowGroups.Add(tableHeaderRG);

            // row
            var tableBodyRG = new TableRowGroup();
            if (TableBodyStyle != null)
            {
                tableBodyRG.Style = TableBodyStyle;
            }
            if (!DisabledTag)
            {
                tableBodyRG.Tag = TagTableBody;
            }

            foreach (int rowIdx in Enumerable.Range(0, mdtable.Details.Count))
            {
                var tableBody = CreateTableRow(mdtable.Details[rowIdx]);
                if (!DisabledTag)
                {
                    tableBody.Tag = (rowIdx & 1) == 0 ? TagOddTableRow : TagEvenTableRow;
                }

                tableBodyRG.Rows.Add(tableBody);
            }
            table.RowGroups.Add(tableBodyRG);

            return table;
        }

        private TableRow CreateTableRow(IList<MdTableCell> mdcells)
        {
            var tableRow = new TableRow();

            foreach (var mdcell in mdcells)
            {
                TableCell cell = mdcell.Text is null ?
                    new TableCell() :
                    new TableCell(Create<Paragraph, Inline>(RunSpanGamut(mdcell.Text)));

                if (mdcell.Horizontal.HasValue)
                    cell.TextAlignment = mdcell.Horizontal.Value;

                if (mdcell.RowSpan != 1)
                    cell.RowSpan = mdcell.RowSpan;

                if (mdcell.ColSpan != 1)
                    cell.ColumnSpan = mdcell.ColSpan;

                tableRow.Cells.Add(cell);
            }

            return tableRow;
        }

        #endregion


        #region grammer - code block

        private static Regex _codeBlock = new Regex(@"
                    (?<=\n)          # Character before opening
                    [ \r\n]*
                    (`+)             # $1 = Opening run of `
                    ([^\r\n`]*)      # $2 = The code lang
                    \r?\n
                    ((.|\n)+?)       # $3 = The code block
                    \n[ ]*
                    \1
                    (?!`)[\r\n]+", RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.Compiled);

        private static Regex _codeBlockFirst = new Regex(@"
                    ^          # Character before opening
                    (`+)             # $1 = Opening run of `
                    ([^\r\n`]*)      # $2 = The code lang
                    \r?\n
                    ((.|\n)+?)       # $3 = The code block
                    \n[ ]*
                    \1
                    (?!`)[\r\n]+", RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.Compiled);

        private IEnumerable<Block> DoCodeBlocks(string text, Func<string, IEnumerable<Block>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return Evaluate(
                text, _codeBlockFirst, CodeBlocksEvaluator,
                sn => Evaluate(sn, _codeBlock, CodeBlocksEvaluator, defaultHandler)
            );
        }

#if MIG_FREE
        private Block CodeBlocksEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            string lang = match.Groups[2].Value;
            string code = match.Groups[3].Value;

            var text = new Run(code);
            var result = new Paragraph(text);
            if (CodeBlockStyle != null)
            {
                result.Style = CodeBlockStyle;
            }
            if (!DisabledTag)
            {
                result.Tag = TagCodeBlock;
            }

            return result;
        }
#else
        private Block CodeBlocksEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            string lang = match.Groups[2].Value;
            string code = match.Groups[3].Value;

            var txtEdit = new TextEditor();
            var highlight = HighlightingManager.Instance.GetDefinitionByExtension("." + lang);
            txtEdit.SyntaxHighlighting = highlight;

            txtEdit.Text = code;
            txtEdit.HorizontalAlignment = HorizontalAlignment.Stretch;
            txtEdit.IsReadOnly = true;

            var result = new BlockUIContainer(txtEdit);
            if (CodeBlockStyle != null)
            {
                result.Style = CodeBlockStyle;
            }
            if (!DisabledTag)
            {
                result.Tag = TagCodeBlock;
            }

            return result;
        }
#endif

        #endregion


        #region grammer - code

        private static Regex _codeSpan = new Regex(@"
                    (?<!\\)   # Character before opening ` can't be a backslash
                    (`+)      # $1 = Opening run of `
                    (.+?)     # $2 = The code block
                    (?<!`)
                    \1
                    (?!`)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// Turn Markdown `code spans` into HTML code tags
        /// </summary>
        private IEnumerable<Inline> DoCodeSpans(string text, Func<string, IEnumerable<Inline>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            //    * You can use multiple backticks as the delimiters if you want to
            //        include literal backticks in the code span. So, this input:
            //
            //        Just type ``foo `bar` baz`` at the prompt.
            //
            //        Will translate to:
            //
            //          <p>Just type <code>foo `bar` baz</code> at the prompt.</p>
            //
            //        There's no arbitrary limit to the number of backticks you
            //        can use as delimters. If you need three consecutive backticks
            //        in your code, use four for delimiters, etc.
            //
            //    * You can use spaces to get literal backticks at the edges:
            //
            //          ... type `` `bar` `` ...
            //
            //        Turns to:
            //
            //          ... type <code>`bar`</code> ...         
            //

            return Evaluate(text, _codeSpan, CodeSpanEvaluator, defaultHandler);
        }

        private Inline CodeSpanEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            string span = match.Groups[2].Value;
            span = Regex.Replace(span, @"^[ ]*", ""); // leading whitespace
            span = Regex.Replace(span, @"[ ]*$", ""); // trailing whitespace

            var result = new Run(span);
            if (CodeStyle != null)
            {
                result.Style = CodeStyle;
            }
            if (!DisabledTag)
            {
                result.Tag = TagCode;
            }

            return result;
        }

        #endregion


        #region grammer - textdecorations

        private static readonly Regex _strictBold = new Regex(@"([\W_]|^) (\*\*|__) (?=\S) ([^\r]*?\S[\*_]*) \2 ([\W_]|$)",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex _strictItalic = new Regex(@"([\W_]|^) (\*|_) (?=\S) ([^\r\*_]*?\S) \2 ([\W_]|$)",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex _strikethrough = new Regex(@"(~~) (?=\S) (.+?) (?<=\S) \1",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex _underline = new Regex(@"(__) (?=\S) (.+?) (?<=\S) \1",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex _color = new Regex(@"%\{[ \t]*color[ \t]*:([^\}]+)\}", RegexOptions.Compiled);

        /// <summary>
        /// Turn Markdown *italics* and **bold** into HTML strong and em tags
        /// </summary>
        private IEnumerable<Inline> DoTextDecorations(string text, Func<string, IEnumerable<Inline>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            // <strong> must go first, then <em>
            if (StrictBoldItalic)
            {
                return Evaluate<Inline>(text, _strictBold, m => BoldEvaluator(m, 3),
                    s1 => Evaluate<Inline>(s1, _strictItalic, m => ItalicEvaluator(m, 3),
                    s2 => Evaluate<Inline>(s2, _strikethrough, m => StrikethroughEvaluator(m, 2),
                    s3 => Evaluate<Inline>(s3, _underline, m => UnderlineEvaluator(m, 2),
                    s4 => defaultHandler(s4)))));
            }
            else
            {
                var rtn = new List<Inline>();

                var buff = new StringBuilder();

                for (var i = 0; i < text.Length; ++i)
                {
                    var ch = text[i];
                    switch (ch)
                    {
                        default:
                            buff.Append(ch);
                            break;

                        case '\\': // escape
                            if (++i < text.Length)
                            {
                                switch (text[i])
                                {
                                    default:
                                        buff.Append('\\').Append(text[i]);
                                        break;

                                    case '\\': // escape
                                    case ':': // bold? or italic
                                    case '*': // bold? or italic
                                    case '~': // strikethrough?
                                    case '_': // underline?
                                    case '%': // color?
                                        buff.Append(text[i]);
                                        break;
                                }
                            }
                            else
                                buff.Append('\\');

                            break;

                        case ':': // emoji?
                            {
                                var nxtI = text.IndexOf(':', i + 1);
                                if (nxtI != -1 && EmojiTable.TryGet(text.Substring(i + 1, nxtI - i - 1), out var emoji))
                                {
                                    buff.Append(emoji);
                                    i = nxtI;
                                }
                                else buff.Append(':');
                                break;
                            }

                        case '*': // bold? or italic
                            {
                                var oldI = i;
                                var inline = ParseAsBoldOrItalic(text, ref i);
                                if (inline == null)
                                {
                                    buff.Append(text, oldI, i - oldI + 1);
                                }
                                else
                                {
                                    rtn.AddRange(defaultHandler(buff.ToString()));
                                    rtn.Add(inline);
                                    buff.Clear();
                                }
                                break;
                            }

                        case '~': // strikethrough?
                            {
                                var oldI = i;
                                var inline = ParseAsStrikethrough(text, ref i);
                                if (inline == null)
                                {
                                    buff.Append(text, oldI, i - oldI + 1);
                                }
                                else
                                {
                                    rtn.AddRange(defaultHandler(buff.ToString()));
                                    rtn.Add(inline);
                                    buff.Clear();
                                }
                                break;
                            }

                        case '_': // underline?
                            {
                                var oldI = i;
                                var inline = ParseAsUnderline(text, ref i);
                                if (inline == null)
                                {
                                    buff.Append(text, oldI, i - oldI + 1);
                                }
                                else
                                {
                                    rtn.AddRange(defaultHandler(buff.ToString()));
                                    rtn.Add(inline);
                                    buff.Clear();
                                }
                                break;
                            }

                        case '%': // color?
                            {
                                var oldI = i;
                                var inline = ParseAsColor(text, ref i);
                                if (inline == null)
                                {
                                    buff.Append(text, oldI, i - oldI + 1);
                                }
                                else
                                {
                                    rtn.AddRange(defaultHandler(buff.ToString()));
                                    rtn.Add(inline);
                                    buff.Clear();
                                }
                                break;
                            }
                    }
                }

                if (buff.Length > 0)
                {
                    rtn.AddRange(defaultHandler(buff.ToString()));
                }

                return rtn;
            }
        }

        private Inline ParseAsUnderline(string text, ref int start)
        {
            var bgnCnt = CountRepeat(text, start, '_');

            int last = EscapedIndexOf(text, start + bgnCnt, '_');

            int endCnt = last >= 0 ? CountRepeat(text, last, '_') : -1;

            if (endCnt >= 2 && bgnCnt >= 2)
            {
                int cnt = 2;
                int bgn = start + cnt;
                int end = last;

                start = end + cnt - 1;
                var span = Create<Underline, Inline>(RunSpanGamut(text.Substring(bgn, end - bgn)));
                if (!DisabledTag)
                {
                    span.Tag = TagUnderlineSpan;
                }
                return span;
            }
            else
            {
                start += bgnCnt - 1;
                return null;
            }
        }

        private Inline ParseAsStrikethrough(string text, ref int start)
        {
            var bgnCnt = CountRepeat(text, start, '~');

            int last = EscapedIndexOf(text, start + bgnCnt, '~');

            int endCnt = last >= 0 ? CountRepeat(text, last, '~') : -1;

            if (endCnt >= 2 && bgnCnt >= 2)
            {
                int cnt = 2;
                int bgn = start + cnt;
                int end = last;

                start = end + cnt - 1;
                var span = Create<Span, Inline>(RunSpanGamut(text.Substring(bgn, end - bgn)));
                span.TextDecorations = TextDecorations.Strikethrough;

                if (!DisabledTag)
                {
                    span.Tag = TagStrikethroughSpan;
                }
                return span;
            }
            else
            {
                start += bgnCnt - 1;
                return null;
            }
        }

        private Inline ParseAsBoldOrItalic(string text, ref int start)
        {
            // count asterisk (bgn)
            var bgnCnt = CountRepeat(text, start, '*');

            int last = EscapedIndexOf(text, start + bgnCnt, '*');

            int endCnt = last >= 0 ? CountRepeat(text, last, '*') : -1;

            if (endCnt >= 1)
            {
                int cnt = Math.Min(bgnCnt, endCnt);
                int bgn = start + cnt;
                int end = last;

                switch (cnt)
                {
                    case 1: // italic
                        {
                            start = end + cnt - 1;

                            var span = Create<Italic, Inline>(RunSpanGamut(text.Substring(bgn, end - bgn)));
                            if (!DisabledTag)
                            {
                                span.Tag = TagItalicSpan;
                            }
                            return span;
                        }
                    case 2: // bold
                        {
                            start = end + cnt - 1;
                            var span = Create<Bold, Inline>(RunSpanGamut(text.Substring(bgn, end - bgn)));
                            if (!DisabledTag)
                            {
                                span.Tag = TagBoldSpan;
                            }
                            return span;
                        }

                    default: // >3; bold-italic
                        {
                            bgn = start + 3;
                            start = end + 3 - 1;

                            var inline = Create<Italic, Inline>(RunSpanGamut(text.Substring(bgn, end - bgn)));
                            if (!DisabledTag)
                            {
                                inline.Tag = TagItalicSpan;
                            }

                            var span = new Bold(inline);
                            if (!DisabledTag)
                            {
                                span.Tag = TagBoldSpan;
                            }
                            return span;
                        }
                }
            }
            else
            {
                start += bgnCnt - 1;
                return null;
            }
        }

        private Inline ParseAsColor(string text, ref int start)
        {
            var mch = _color.Match(text, start);

            if (mch.Success && start == mch.Index)
            {
                int bgnIdx = start + mch.Value.Length;
                int endIdx = EscapedIndexOf(text, bgnIdx, '%');

                Span span;
                if (endIdx == -1)
                {
                    endIdx = text.Length - 1;
                    span = Create<Span, Inline>(
                        RunSpanGamut(text.Substring(bgnIdx)));
                }
                else
                {
                    span = Create<Span, Inline>(
                        RunSpanGamut(text.Substring(bgnIdx, endIdx - bgnIdx)));
                }

                var colorLbl = mch.Groups[1].Value;

                try
                {
                    var color = colorLbl.StartsWith("#") ?
                        (SolidColorBrush)new BrushConverter().ConvertFrom(colorLbl) :
                        (SolidColorBrush)new BrushConverter().ConvertFromString(colorLbl);

                    span.Foreground = color;
                }
                catch { }

                start = endIdx;
                return span;
            }
            else return null;
        }


        private int EscapedIndexOf(string text, int start, char target)
        {
            for (var i = start; i < text.Length; ++i)
            {
                var ch = text[i];
                if (ch == '\\') ++i;
                else if (ch == target) return i;
            }
            return -1;
        }
        private int CountRepeat(string text, int start, char target)
        {
            var count = 0;

            for (var i = start; i < text.Length; ++i)
            {
                if (text[i] == target) ++count;
                else break;
            }

            return count;
        }


        private Inline ItalicEvaluator(Match match, int contentGroup)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var content = match.Groups[contentGroup].Value;
            var span = Create<Italic, Inline>(RunSpanGamut(content));
            if (!DisabledTag)
            {
                span.Tag = TagItalicSpan;
            }
            return span;
        }

        private Inline BoldEvaluator(Match match, int contentGroup)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var content = match.Groups[contentGroup].Value;
            var span = Create<Bold, Inline>(RunSpanGamut(content));
            if (!DisabledTag)
            {
                span.Tag = TagBoldSpan;
            }
            return span;
        }

        private Inline StrikethroughEvaluator(Match match, int contentGroup)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var content = match.Groups[contentGroup].Value;

            var span = Create<Span, Inline>(RunSpanGamut(content));
            span.TextDecorations = TextDecorations.Strikethrough;
            if (!DisabledTag)
            {
                span.Tag = TagStrikethroughSpan;
            }
            return span;
        }

        private Inline UnderlineEvaluator(Match match, int contentGroup)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var content = match.Groups[contentGroup].Value;
            var span = Create<Underline, Inline>(RunSpanGamut(content));
            if (!DisabledTag)
            {
                span.Tag = TagUnderlineSpan;
            }
            return span;
        }

        #endregion


        #region grammer - text

        private static Regex _eoln = new Regex("\\s+");
        private static Regex _lbrk = new Regex(@"\ {2,}\n");

        public IEnumerable<Inline> DoText(string text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            var lines = _lbrk.Split(text);
            bool first = true;
            foreach (var line in lines)
            {
                if (first)
                    first = false;
                else
                    yield return new LineBreak();
                var t = _eoln.Replace(line, " ");
                yield return new Run(t);
            }
        }

        #endregion

        #region grammer - blockquote

        private static Regex _blockquote = new Regex(@"
            (?<=\n)
            [\r\n]*
            ([>].*)
            (\r?\n[>].*)*
            [\r\n]*
            ", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private static Regex _blockquoteFirst = new Regex(@"
            ^
            ([>].*)
            (\r?\n[>].*)*
            [\r\n]*
            ", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private IEnumerable<Block> DoBlockquotes(string text, Func<string, IEnumerable<Block>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return Evaluate(
                text, _blockquoteFirst, BlockquotesEvaluator,
                sn => Evaluate(sn, _blockquote, BlockquotesEvaluator, defaultHandler)
            );
        }

        private Section BlockquotesEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            // trim '>'
            var ln = new Regex("\r?\n");
            var trimmedTxt = string.Join(
                    "\n",
                    ln.Split(match.Value.Trim())
                        .Select(txt =>
                        {
                            if (txt.Length <= 1) return string.Empty;
                            var trimmed = txt.Substring(1);
                            if (trimmed.FirstOrDefault() == ' ') trimmed = trimmed.Substring(1);
                            return trimmed;
                        })
                        .ToArray()
            );

            var blocks = RunBlockGamut(Normalize(trimmedTxt), true);
            var result = Create<Section, Block>(blocks);
            if (BlockquoteStyle != null)
            {
                result.Style = BlockquoteStyle;
            }
            if (!DisabledTag)
            {
                result.Tag = TagBlockquote;
            }

            return result;
        }


        #endregion

        #region helper - make regex

        private static string _nestedBracketsPattern;

        /// <summary>
        /// Reusable pattern to match balanced [brackets]. See Friedl's 
        /// "Mastering Regular Expressions", 2nd Ed., pp. 328-331.
        /// </summary>
        private static string GetNestedBracketsPattern()
        {
            // in other words [this] and [this[also]] and [this[also[too]]]
            // up to _nestDepth
            if (_nestedBracketsPattern is null)
                _nestedBracketsPattern =
                    RepeatString(@"
                    (?>              # Atomic matching
                       [^\[\]]+      # Anything other than brackets
                     |
                       \[
                           ", _nestDepth) + RepeatString(
                    @" \]
                    )*"
                    , _nestDepth);
            return _nestedBracketsPattern;
        }

        private static string _nestedParensPattern;

        /// <summary>
        /// Reusable pattern to match balanced (parens). See Friedl's 
        /// "Mastering Regular Expressions", 2nd Ed., pp. 328-331.
        /// </summary>
        private static string GetNestedParensPattern()
        {
            // in other words (this) and (this(also)) and (this(also(too)))
            // up to _nestDepth
            if (_nestedParensPattern is null)
                _nestedParensPattern =
                    RepeatString(@"
                    (?>              # Atomic matching
                       [^()\s]+      # Anything other than parens or whitespace
                     |
                       \(
                           ", _nestDepth) + RepeatString(
                    @" \)
                    )*"
                    , _nestDepth);
            return _nestedParensPattern;
        }

        private static string _nestedParensPatternWithWhiteSpace;

        /// <summary>
        /// Reusable pattern to match balanced (parens), including whitespace. See Friedl's 
        /// "Mastering Regular Expressions", 2nd Ed., pp. 328-331.
        /// </summary>
        private static string GetNestedParensPatternWithWhiteSpace()
        {
            // in other words (this) and (this(also)) and (this(also(too)))
            // up to _nestDepth
            if (_nestedParensPatternWithWhiteSpace is null)
                _nestedParensPatternWithWhiteSpace =
                    RepeatString(@"
                    (?>              # Atomic matching
                       [^()]+      # Anything other than parens
                     |
                       \(
                           ", _nestDepth) + RepeatString(
                    @" \)
                    )*"
                    , _nestDepth);
            return _nestedParensPatternWithWhiteSpace;
        }

        /// <summary>
        /// this is to emulate what's evailable in PHP
        /// </summary>
        private static string RepeatString(string text, int count)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            var sb = new StringBuilder(text.Length * count);
            for (int i = 0; i < count; i++)
                sb.Append(text);
            return sb.ToString();
        }

        #endregion


        #region helper - parse

        private TResult Create<TResult, TContent>(IEnumerable<TContent> content)
            where TResult : IAddChild, new()
        {
            var result = new TResult();
            foreach (var c in content)
            {
                result.AddChild(c);
            }

            return result;
        }

        private IEnumerable<T> Evaluate<T>(string text, Regex expression, Func<Match, T> build, Func<string, IEnumerable<T>> rest)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            var matches = expression.Matches(text);
            var index = 0;
            foreach (Match m in matches)
            {
                if (m.Index > index)
                {
                    var prefix = text.Substring(index, m.Index - index);
                    foreach (var t in rest(prefix))
                    {
                        yield return t;
                    }
                }

                yield return build(m);

                index = m.Index + m.Length;
            }

            if (index < text.Length)
            {
                var suffix = text.Substring(index, text.Length - index);
                foreach (var t in rest(suffix))
                {
                    yield return t;
                }
            }
        }

        #endregion
    }
}