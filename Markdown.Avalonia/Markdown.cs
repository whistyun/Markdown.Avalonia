﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using ColorTextBlock.Avalonia;
using Markdown.Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Markdown.Avalonia
{
    public class Markdown : AvaloniaObject
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

        private const string Heading1Class = "Heading1";
        private const string Heading2Class = "Heading2";
        private const string Heading3Class = "Heading3";
        private const string Heading4Class = "Heading4";
        private const string CodeBlockClass = "CodeBlock";
        private const string BlockquoteClass = "Blockquote";
        private const string NoteClass = "Note";

        private const string TableClass = "Table";
        private const string TableHeaderClass = "TableHeader";
        private const string TableRowOddClass = "OddTableRow";
        private const string TableRowEvenClass = "EvenTableRow";

        private const string ListClass = "List";
        private const string ListMarkerClass = "ListMarker";

        #endregion

        /// <summary>
        /// when true, bold and italic require non-word characters on either side  
        /// WARNING: this is a significant deviation from the markdown spec
        /// </summary>
        public bool StrictBoldItalic { get; set; }

        public bool DisabledTootip { get; set; }

        //public bool DisabledLazyLoad { get; set; }

        public string AssetPathRoot { get; set; }

        public Action<string> HyperlinkCommand { get; set; }

        private IAssetLoader AssetLoader { get; }
        private string[] AssetAssemblyNames { get; }

        #region dependencyobject property

        // Using a DependencyProperty as the backing store for DocumentStyle.  This enables animation, styling, binding, etc...
        public static readonly AvaloniaProperty DocumentStyleProperty =
            AvaloniaProperty.Register<Markdown, Style>(nameof(DocumentStyle), defaultValue: null);

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

        /*
         
         TODO read https://github.com/AvaloniaUI/Avalonia/issues/2765
         
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
        */

        #endregion


        #region regex pattern


        #endregion

        public Markdown()
        {
            HyperlinkCommand = (url) =>
            {
                // https://github.com/dotnet/runtime/issues/17938
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    Process.Start(new ProcessStartInfo(url)
                    {
                        UseShellExecute = true,
                        Verb = "open"
                    });

                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    Process.Start("xdg-open", url);

                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    Process.Start("open", url);
            };

            AssetPathRoot = Environment.CurrentDirectory;

            AssetLoader = AvaloniaLocator.Current.GetService<IAssetLoader>();

            var myasm = Assembly.GetCallingAssembly();
            var stack = new StackTrace();
            AssetAssemblyNames = stack.GetFrames()
                            .Select(frm => frm.GetMethod().DeclaringType.Assembly)
                            .Where(asm => asm != myasm)
                            .Select(asm => asm.GetName().Name)
                            .Distinct()
                            .ToArray();
        }

        public Control Transform(string text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            text = Normalize(text);

            var document = Create<StackPanel, Control>(RunBlockGamut(text, true));
            document.Orientation = Orientation.Vertical;

            // todo implements after
            //            document.SetBinding(FlowDocument.StyleProperty, new Binding(DocumentStyleProperty.Name) { Source = this });

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
        private IEnumerable<Control> RunBlockGamut(string text, bool supportTextAlignment)
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
        private IEnumerable<CInline> RunSpanGamut(string text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return DoCodeSpans(text,
                s0 => DoImagesOrHrefs(s0,
                s1 => DoTextDecorations(s1,
                s2 => DoText(s2))));

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
        private IEnumerable<Control> FormParagraphs(string text, bool supportTextAlignment)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }


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

                var ctbox = new CTextBlock();
                ctbox.Content = RunSpanGamut(chip).ToList();

                if (indiAlignment.HasValue)
                    ctbox.TextAlignment = indiAlignment.Value;

                yield return ctbox;
            }
        }

        #endregion

        #region grammer - image or href

        private static readonly Regex _imageOrHrefInline = new Regex(string.Format(@"
                (                           # wrap whole match in $1
                    (!)?                    # image maker = $2
                    \[
                        ({0})               # link text = $3
                    \]
                    \(                      # literal paren
                        [ ]*
                        ({1})               # href = $4
                        [ ]*
                        (                   # $5
                        (['""])             # quote char = $6
                        (.*?)               # title = $7
                        \6                  # matching quote
                        [ ]*                # ignore any spaces between closing quote and )
                        )?                  # title is optional
                    \)
                )", GetNestedBracketsPattern(), GetNestedParensPattern()),
                  RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);


        private IEnumerable<CInline> DoImagesOrHrefs(string text, Func<string, IEnumerable<CInline>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return Evaluate(text, _imageOrHrefInline, ImageOrHrefInlineEvaluator, defaultHandler);
        }

        private CInline ImageOrHrefInlineEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            if (String.IsNullOrEmpty(match.Groups[2].Value))
            {
                return TreatsAsHref(match);
            }
            else
            {
                return TreatsAsImage(match);
            }
        }

        private CInline TreatsAsHref(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            string linkText = match.Groups[3].Value;
            string url = match.Groups[4].Value;
            string title = match.Groups[7].Value;

            var result = new CHyperlink(RunSpanGamut(linkText));
            result.Command = HyperlinkCommand;
            result.CommandParameter = url;

            return result;
        }

        private CInline TreatsAsImage(Match match)
        {
            string linkText = match.Groups[3].Value;
            string urlTxt = match.Groups[4].Value;
            string title = match.Groups[7].Value;

            Bitmap imgSource = null;

            if (Uri.TryCreate(urlTxt, UriKind.Absolute, out var url))
            {
                try
                {
                    switch (url.Scheme)
                    {
                        case "http":
                        case "https":
                            using (var wc = new System.Net.WebClient())
                            using (var strm = new MemoryStream(wc.DownloadData(url)))
                                imgSource = new Bitmap(strm);
                            break;

                        case "file":
                            using (var strm = File.OpenRead(url.LocalPath))
                                imgSource = new Bitmap(strm);
                            break;
                    }


                }
                catch { }
            }

            // check embedded resoruce
            if (imgSource is null)
            {
                foreach (var asmNm in AssetAssemblyNames)
                {
                    try
                    {
                        var assetUrl = new Uri($"avares://{asmNm}/{urlTxt}");

                        using (var strm = AssetLoader.Open(assetUrl))
                            imgSource = new Bitmap(strm);

                        break;
                    }
                    catch (Exception e)
                    {
                        e.ToString();
                    }
                }
            }

            // check filesystem
            if (imgSource is null && AssetPathRoot != null)
            {
                try
                {
                    using (var strm = File.OpenRead(Path.Combine(AssetPathRoot, urlTxt)))
                        imgSource = new Bitmap(strm);
                }
                catch { }
            }

            // error
            if (imgSource is null)
            {
                return new CRun()
                {
                    Text = "!" + urlTxt,
                    Foreground = Brushes.Red
                };
            }
            else
            {
                return new CImage(imgSource);
            }
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
        private IEnumerable<Control> DoHeaders(string text, Func<string, IEnumerable<Control>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return Evaluate<Control>(text, _headerSetext, m => SetextHeaderEvaluator(m),
                s => Evaluate<Control>(s, _headerAtx, m => AtxHeaderEvaluator(m), defaultHandler));
        }

        private CTextBlock SetextHeaderEvaluator(Match match)
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

        private CTextBlock AtxHeaderEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            string header = match.Groups[2].Value;
            int level = match.Groups[1].Value.Length;
            return CreateHeader(level, RunSpanGamut(header));
        }

        public CTextBlock CreateHeader(int level, IEnumerable<CInline> content)
        {
            if (content is null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            var heading = new CTextBlock() { Content = content.ToList() };

            switch (level)
            {
                case 1:
                    heading.Classes.Add(Heading1Class);
                    break;

                case 2:
                    heading.Classes.Add(Heading2Class);
                    break;

                case 3:
                    heading.Classes.Add(Heading3Class);
                    break;

                case 4:
                    heading.Classes.Add(Heading4Class);
                    break;
            }

            return heading;
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
        private IEnumerable<Control> DoNote(string text, bool supportTextAlignment,
                Func<string, IEnumerable<Control>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return Evaluate<Control>(text, _note,
                m => NoteEvaluator(m, supportTextAlignment),
                defaultHandler);
        }

        private Border NoteEvaluator(Match match, bool supportTextAlignment)
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

        public Border NoteComment(IEnumerable<CInline> content, TextAlignment? indiAlignment)
        {
            if (content is null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            var note = new CTextBlock() { Content = content.ToList() };
            note.Classes.Add(NoteClass);
            if (indiAlignment.HasValue)
            {
                note.TextAlignment = indiAlignment.Value;
            }

            var result = new Border();
            result.Classes.Add(NoteClass);
            result.Child = note;

            return result;
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
        private IEnumerable<Control> DoHorizontalRules(string text, Func<string, IEnumerable<Control>> defaultHandler)
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
        private Rule RuleEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            return new Rule(RuleType.Single);
        }

        /// <summary>
        /// Two lines separator.
        /// </summary>
        private Rule TwoLinesRuleEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            return new Rule(RuleType.TwoLines);
        }

        /// <summary>
        /// Double line separator.
        /// </summary>
        private Rule BoldRuleEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            return new Rule(RuleType.Bold);
        }

        /// <summary>
        /// Two lines separator consisting of a double line and a single line.
        /// </summary>
        private Rule BoldWithSingleRuleEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            return new Rule(RuleType.BoldWithSingle);
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

        private static readonly Regex _listTopLevel = new Regex(@"(?:(?<=\n)|\A\n?)" + _wholeList,
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        /// <summary>
        /// Turn Markdown lists into HTML ul and ol and li tags
        /// </summary>
        private IEnumerable<Control> DoLists(string text, Func<string, IEnumerable<Control>> defaultHandler)
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

        private Control ListEvaluator(Match match)
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

            IEnumerable<Control> listItems = ProcessListItems(list, listType == "ul" ? _markerUL : _markerOL);


            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            foreach (Tuple<Control, int> listItemTpl in listItems.Select((elm, idx) => Tuple.Create(elm, idx)))
            {
                var index = listItemTpl.Item2;
                CTextBlock tbox;

                switch (textMarker)
                {
                    default:
                        goto case TextMarkerStyle.Disc;

                    case TextMarkerStyle.None:
                        tbox = new CTextBlock("");
                        break;

                    case TextMarkerStyle.Disc:
                        tbox = new CTextBlock("•");
                        break;

                    case TextMarkerStyle.Box:
                        tbox = new CTextBlock("▪");
                        break;

                    case TextMarkerStyle.Circle:
                        tbox = new CTextBlock("○");
                        break;

                    case TextMarkerStyle.Square:
                        tbox = new CTextBlock("❏");
                        break;

                    case TextMarkerStyle.Decimal:
                        tbox = new CTextBlock((index + 1).ToString() + ".");
                        break;

                    case TextMarkerStyle.LowerLatin:
                        tbox = new CTextBlock(NumberToOrder.ToLatin(index + 1).ToLower() + ".");
                        break;

                    case TextMarkerStyle.UpperLatin:
                        tbox = new CTextBlock(NumberToOrder.ToLatin(index + 1) + ".");
                        break;

                    case TextMarkerStyle.LowerRoman:
                        tbox = new CTextBlock(NumberToOrder.ToRoman(index + 1).ToLower() + ".");
                        break;

                    case TextMarkerStyle.UpperRoman:
                        tbox = new CTextBlock(NumberToOrder.ToRoman(index + 1) + ".");
                        break;
                }


                var control = listItemTpl.Item1;

                grid.RowDefinitions.Add(new RowDefinition());
                grid.Children.Add(tbox);
                grid.Children.Add(control);

                tbox.TextAlignment = TextAlignment.Right;
                tbox.Classes.Add(ListMarkerClass);
                Grid.SetRow(tbox, index);
                Grid.SetColumn(tbox, 0);

                Grid.SetRow(control, index);
                Grid.SetColumn(control, 1);
            }

            grid.Classes.Add(ListClass);

            return grid;
        }

        /// <summary>
        /// Process the contents of a single ordered or unordered list, splitting it
        /// into individual list items.
        /// </summary>
        private IEnumerable<Control> ProcessListItems(string list, string marker)
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

        private Control ListItemEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            string item = match.Groups[4].Value;
            string leadingLine = match.Groups[1].Value;

            if (!String.IsNullOrEmpty(leadingLine) || Regex.IsMatch(item, @"\n{2,}"))
                // we could correct any bad indentation here..

                return Create<Panel, Control>(RunBlockGamut(item, false));
            else
            {
                // recursion for sub-lists
                return Create<Panel, Control>(RunBlockGamut(item, false));
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

        public IEnumerable<Control> DoTable(string text, Func<string, IEnumerable<Control>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return Evaluate(text, _table, TableEvalutor, defaultHandler);
        }

        private Border TableEvalutor(Match match)
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
            var table = new Grid();

            // table columns
            while (table.ColumnDefinitions.Count < mdtable.ColCount)
                table.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

            // table header
            table.RowDefinitions.Add(new RowDefinition());
            foreach (Border tableHeaderCell in CreateTableRow(mdtable.Header, 0))
            {
                tableHeaderCell.Classes.Add(TableHeaderClass);

                table.Children.Add(tableHeaderCell);
            }

            // table cell
            foreach (int rowIdx in Enumerable.Range(0, mdtable.Details.Count))
            {
                table.RowDefinitions.Add(new RowDefinition());
                foreach (Border cell in CreateTableRow(mdtable.Details[rowIdx], rowIdx + 1))
                {
                    cell.Classes.Add((rowIdx & 1) == 0 ? TableRowOddClass : TableRowEvenClass);

                    table.Children.Add(cell);
                }
            }

            table.Classes.Add(TableClass);

            var result = new Border();
            result.Child = table;
            result.Classes.Add(TableClass);

            return result;
        }

        private IEnumerable<Border> CreateTableRow(IList<MdTableCell> mdcells, int rowIdx)
        {
            foreach (var mdcell in mdcells)
            {
                var cell = new Border();

                if (!(mdcell.Text is null))
                {
                    var txtbx = new CTextBlock() { Content = RunSpanGamut(mdcell.Text).ToList() };
                    cell.Child = txtbx;

                    if (mdcell.Horizontal.HasValue)
                        txtbx.TextAlignment = mdcell.Horizontal.Value;
                }

                Grid.SetRow(cell, rowIdx);
                Grid.SetColumn(cell, mdcell.ColumnIndex);

                if (mdcell.RowSpan != 1)
                    Grid.SetRowSpan(cell, mdcell.RowSpan);

                if (mdcell.ColSpan != 1)
                    Grid.SetColumnSpan(cell, mdcell.ColSpan);

                yield return cell;
            }
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

        private IEnumerable<Control> DoCodeBlocks(string text, Func<string, IEnumerable<Control>> defaultHandler)
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

        private Border CodeBlocksEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            string lang = match.Groups[2].Value;
            string code = match.Groups[3].Value;

            var ctxt = new TextBlock()
            {
                Text = code,
                TextWrapping = TextWrapping.NoWrap
            };
            ctxt.Classes.Add(CodeBlockClass);

            var result = new Border();
            result.Classes.Add(CodeBlockClass);
            result.Child = ctxt;

            return result;
        }
        // Use AvalonEdit
        //        private Block CodeBlocksEvaluator(Match match)
        //        {
        //            if (match is null)
        //            {
        //                throw new ArgumentNullException(nameof(match));
        //            }
        //
        //            string lang = match.Groups[2].Value;
        //            string code = match.Groups[3].Value;
        //
        //            var txtEdit = new TextEditor();
        //            var highlight = HighlightingManager.Instance.GetDefinitionByExtension("." + lang);
        //            txtEdit.SyntaxHighlighting = highlight;
        //
        //            txtEdit.Text = code;
        //            txtEdit.HorizontalAlignment = HorizontalAlignment.Stretch;
        //            txtEdit.IsReadOnly = true;
        //
        //            var result = new BlockUIContainer(txtEdit);
        //            if (CodeBlockStyle != null)
        //            {
        //                result.Style = CodeBlockStyle;
        //            }
        //            if (!DisabledTag)
        //            {
        //                result.Tag = TagCodeBlock;
        //            }
        //
        //            return result;
        //        }


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
        private IEnumerable<CInline> DoCodeSpans(string text, Func<string, IEnumerable<CInline>> defaultHandler)
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

        private CCode CodeSpanEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            string span = match.Groups[2].Value;
            span = Regex.Replace(span, @"^[ ]*", ""); // leading whitespace
            span = Regex.Replace(span, @"[ ]*$", ""); // trailing whitespace

            var result = new CCode(new[] { new CRun() { Text = span } });

            // TODO use style selector
            result.Foreground = new SolidColorBrush(Colors.DarkBlue);
            result.Background = new SolidColorBrush(Colors.LightGray);

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
        private IEnumerable<CInline> DoTextDecorations(string text, Func<string, IEnumerable<CInline>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            // <strong> must go first, then <em>
            if (StrictBoldItalic)
            {
                return Evaluate<CInline>(text, _strictBold, m => BoldEvaluator(m, 3),
                    s1 => Evaluate<CInline>(s1, _strictItalic, m => ItalicEvaluator(m, 3),
                    s2 => Evaluate<CInline>(s2, _strikethrough, m => StrikethroughEvaluator(m, 2),
                    s3 => Evaluate<CInline>(s3, _underline, m => UnderlineEvaluator(m, 2),
                    s4 => defaultHandler(s4)))));
            }
            else
            {
                var rtn = new List<CInline>();

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

        private CUnderline ParseAsUnderline(string text, ref int start)
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
                var span = new CUnderline(RunSpanGamut(text.Substring(bgn, end - bgn)));
                return span;
            }
            else
            {
                start += bgnCnt - 1;
                return null;
            }
        }

        private CStrikethrough ParseAsStrikethrough(string text, ref int start)
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
                var span = new CStrikethrough(RunSpanGamut(text.Substring(bgn, end - bgn)));
                return span;
            }
            else
            {
                start += bgnCnt - 1;
                return null;
            }
        }

        private CInline ParseAsBoldOrItalic(string text, ref int start)
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

                            var span = new CItalic(RunSpanGamut(text.Substring(bgn, end - bgn)));
                            return span;
                        }
                    case 2: // bold
                        {
                            start = end + cnt - 1;
                            var span = new CBold(RunSpanGamut(text.Substring(bgn, end - bgn)));
                            return span;
                        }

                    default: // >3; bold-italic
                        {
                            bgn = start + 3;
                            start = end + 3 - 1;

                            var inline = new CItalic(RunSpanGamut(text.Substring(bgn, end - bgn)));
                            var span = new CBold(new[] { inline });
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

        private CInline ParseAsColor(string text, ref int start)
        {
            var mch = _color.Match(text, start);

            if (mch.Success && start == mch.Index)
            {
                int bgnIdx = start + mch.Value.Length;
                int endIdx = EscapedIndexOf(text, bgnIdx, '%');

                CSpan span;
                if (endIdx == -1)
                {
                    endIdx = text.Length - 1;
                    span = new CSpan(RunSpanGamut(text.Substring(bgnIdx)));
                }
                else
                {
                    span = new CSpan(RunSpanGamut(text.Substring(bgnIdx, endIdx - bgnIdx)));
                }

                var colorLbl = mch.Groups[1].Value;

                try
                {
                    var color = colorLbl.StartsWith("#") ?
                        (IBrush)new BrushConverter().ConvertFrom(colorLbl) :
                        (IBrush)new BrushConverter().ConvertFromString(colorLbl);

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

        private CItalic ItalicEvaluator(Match match, int contentGroup)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var content = match.Groups[contentGroup].Value;

            return new CItalic(RunSpanGamut(content));
        }

        private CBold BoldEvaluator(Match match, int contentGroup)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var content = match.Groups[contentGroup].Value;

            return new CBold(RunSpanGamut(content));
        }

        private CStrikethrough StrikethroughEvaluator(Match match, int contentGroup)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var content = match.Groups[contentGroup].Value;

            return new CStrikethrough(RunSpanGamut(content));
        }

        private CUnderline UnderlineEvaluator(Match match, int contentGroup)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var content = match.Groups[contentGroup].Value;

            return new CUnderline(RunSpanGamut(content));
        }

        #endregion


        #region grammer - text

        private static Regex _eoln = new Regex("\\s+");
        private static Regex _lbrk = new Regex(@"\ {2,}\n");

        public IEnumerable<CRun> DoText(string text)
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
                    yield return new CLineBreak();
                var t = _eoln.Replace(line, " ");
                yield return new CRun() { Text = t };
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

        private IEnumerable<Control> DoBlockquotes(string text, Func<string, IEnumerable<Control>> defaultHandler)
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

        private Border BlockquotesEvaluator(Match match)
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

            var panel = Create<StackPanel, Control>(blocks);
            panel.Orientation = Orientation.Vertical;
            panel.Classes.Add(BlockquoteClass);

            var result = new Border();
            result.Classes.Add(BlockquoteClass);
            result.Child = panel;

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
            where TResult : IPanel, new()
            where TContent : IControl
        {
            var result = new TResult();
            foreach (var c in content)
            {
                result.Children.Add(c);
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