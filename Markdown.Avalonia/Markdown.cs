using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using ColorTextBlock.Avalonia;
using Markdown.Avalonia.Controls;
using Markdown.Avalonia.Tables;
using Markdown.Avalonia.Utils;
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
using System.Threading.Tasks;
using System.Windows.Input;

namespace Markdown.Avalonia
{
    public class Markdown : AvaloniaObject, IMarkdownEngine
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

        public const string Heading1Class = "Heading1";
        public const string Heading2Class = "Heading2";
        public const string Heading3Class = "Heading3";
        public const string Heading4Class = "Heading4";
        public const string Heading5Class = "Heading5";
        public const string Heading6Class = "Heading6";

        public const string CodeBlockClass = "CodeBlock";
        public const string BlockquoteClass = "Blockquote";
        public const string NoteClass = "Note";

        public const string ParagraphClass = "Paragraph";

        public const string TableClass = "Table";
        public const string TableHeaderClass = "TableHeader";
        public const string TableRowOddClass = "OddTableRow";
        public const string TableRowEvenClass = "EvenTableRow";

        public const string ListClass = "List";
        public const string ListMarkerClass = "ListMarker";

        #endregion

        /// <summary>
        /// when true, bold and italic require non-word characters on either side  
        /// WARNING: this is a significant deviation from the markdown spec
        /// </summary>
        public bool StrictBoldItalic { get; set; }

        private string _assetPathRoot;
        /// <inheritdoc/>
        public string AssetPathRoot
        {
            get => _assetPathRoot;
            set => BitmapLoader.AssetPathRoot = _assetPathRoot = value;
        }

        /// <inheritdoc/>
        public ICommand HyperlinkCommand { get; set; }

        private IBitmapLoader _loader;
        /// <inheritdoc/>
        public IBitmapLoader BitmapLoader
        {
            get => _loader;
            set
            {
                _loader = value;
                if (_loader != null)
                {
                    _loader.AssetPathRoot = _assetPathRoot;
                }
            }
        }

        private Bitmap ImageNotFound { get; }

        #region dependencyobject property

        public static readonly DirectProperty<Markdown, ICommand> HyperlinkCommandProperty =
            AvaloniaProperty.RegisterDirect<Markdown, ICommand>(nameof(HyperlinkCommand),
                mdEng => mdEng.HyperlinkCommand,
                (mdEng, command) => mdEng.HyperlinkCommand = command);

        public static readonly DirectProperty<Markdown, IBitmapLoader> BitmapLoaderProperty =
            AvaloniaProperty.RegisterDirect<Markdown, IBitmapLoader>(nameof(BitmapLoader),
                mdEng => mdEng.BitmapLoader,
                (mdEng, loader) => mdEng.BitmapLoader = loader);

        #endregion

        public Markdown()
        {
            _assetPathRoot = Environment.CurrentDirectory;

            HyperlinkCommand = new DefaultHyperlinkCommand();
            BitmapLoader = new DefaultBitmapLoader();

            var assetLoader = AvaloniaLocator.Current.GetService<IAssetLoader>();
            using (var strm = assetLoader.Open(new Uri($"avares://Markdown.Avalonia/Assets/ImageNotFound.bmp")))
                ImageNotFound = new Bitmap(strm);
        }

        /// <inheritdoc/>
        public Control Transform(string text)
        {
            if (text is null)
            {
                Helper.ThrowArgNull(nameof(text));
            }

            text = TextUtil.Normalize(text, _tabWidth);

            var document = Create<StackPanel, Control>(RunBlockGamut(text, true));
            document.Orientation = Orientation.Vertical;

            return document;
        }

        /// <summary>
        /// Perform transformations that form block-level tags like paragraphs, headers, and list items.
        /// </summary>
        private IEnumerable<Control> RunBlockGamut(string text, bool supportTextAlignment)
        {
            if (text is null)
            {
                Helper.ThrowArgNull(nameof(text));
            }

            return Evaluate2(
                text,
                _codeBlockFirst, CodeBlocksWithLangEvaluator,
                _listLevel > 0 ? _listNested : _listTopLevel, ListEvaluator,
                s1 => DoBlockquotes(s1,
                s2 => DoHeaders(s2,
                s3 => DoHorizontalRules(s3,
                s4 => DoTable(s4,
                s5 => DoNote(s5, supportTextAlignment,
                s6 => DoIndentCodeBlock(s6,
                sn => FormParagraphs(sn, supportTextAlignment)))))))
            );
        }

        /// <summary>
        /// Perform transformations that occur *within* block-level tags like paragraphs, headers, and list items.
        /// </summary>
        private IEnumerable<CInline> RunSpanGamut(string text)
        {
            if (text is null)
            {
                Helper.ThrowArgNull(nameof(text));
            }

            return DoCodeSpans(text,
                s0 => DoImagesOrHrefs(s0,
                s1 => DoTextDecorations(s1,
                s2 => DoText(s2))));
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
                Helper.ThrowArgNull(nameof(text));
            }

            var trimemdText = _newlinesLeadingTrailing.Replace(text, "");

            string[] grafs = trimemdText == "" ?
                new string[0] :
                _newlinesMultiple.Split(trimemdText);

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

                var ctbox = new CTextBlock(RunSpanGamut(chip));

                if (indiAlignment.HasValue)
                    ctbox.TextAlignment = indiAlignment.Value;

                ctbox.Classes.Add(ParagraphClass);

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
                Helper.ThrowArgNull(nameof(text));
            }

            return Evaluate(text, _imageOrHrefInline, ImageOrHrefInlineEvaluator, defaultHandler);
        }

        private CInline ImageOrHrefInlineEvaluator(Match match)
        {
            if (match is null)
            {
                Helper.ThrowArgNull(nameof(match));
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
                Helper.ThrowArgNull(nameof(match));
            }

            string linkText = match.Groups[3].Value;
            string url = match.Groups[4].Value;
            string title = match.Groups[7].Value;

            var link = new CHyperlink(RunSpanGamut(linkText));
            link.Command = (urlTxt) =>
            {
                if (HyperlinkCommand != null && HyperlinkCommand.CanExecute(urlTxt))
                {
                    HyperlinkCommand.Execute(urlTxt);
                }
            };

            link.CommandParameter = url;

            if (!String.IsNullOrEmpty(title)
                && !title.Any(ch => !Char.IsLetterOrDigit(ch)))
            {
                link.Classes.Add(title);
            }

            return link;
        }

        private CInline TreatsAsImage(Match match)
        {
            string altText = match.Groups[3].Value;
            string urlTxt = match.Groups[4].Value;
            string title = match.Groups[7].Value;

            var image = new CImage(
                Task.Run(() => BitmapLoader?.Get(urlTxt)),
                ImageNotFound);

            if (!String.IsNullOrEmpty(title)
                && !title.Any(ch => !Char.IsLetterOrDigit(ch)))
            {
                image.Classes.Add(title);
            }

            return image;
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
                Helper.ThrowArgNull(nameof(text));
            }

            return Evaluate<Control>(text, _headerSetext, m => SetextHeaderEvaluator(m),
                s => Evaluate<Control>(s, _headerAtx, m => AtxHeaderEvaluator(m), defaultHandler));
        }

        private CTextBlock SetextHeaderEvaluator(Match match)
        {
            if (match is null)
            {
                Helper.ThrowArgNull(nameof(match));
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
                Helper.ThrowArgNull(nameof(match));
            }

            string header = match.Groups[2].Value;
            int level = match.Groups[1].Value.Length;
            return CreateHeader(level, RunSpanGamut(header));
        }

        public CTextBlock CreateHeader(int level, IEnumerable<CInline> content)
        {
            if (content is null)
            {
                Helper.ThrowArgNull(nameof(content));
            }

            var heading = new CTextBlock(content);

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

                case 5:
                    heading.Classes.Add(Heading5Class);
                    break;

                case 6:
                    heading.Classes.Add(Heading6Class);
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
                Helper.ThrowArgNull(nameof(text));
            }

            return Evaluate<Control>(text, _note,
                m => NoteEvaluator(m, supportTextAlignment),
                defaultHandler);
        }

        private Border NoteEvaluator(Match match, bool supportTextAlignment)
        {
            if (match is null)
            {
                Helper.ThrowArgNull(nameof(match));
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
                Helper.ThrowArgNull(nameof(content));
            }

            var note = new CTextBlock(content);
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

        private static readonly Regex _horizontalRules = new Regex(@"
                ^[ ]{0,3}                   # Leading space
                    ([-=*_])                # $1: First marker ([markers])
                    (?>                     # Repeated marker group
                        [ ]{0,2}            # Zero, one, or two spaces.
                        \1                  # Marker character
                    ){2,}                   # Group repeated at least twice
                    [ ]*                    # Trailing spaces
                    \n                      # End of line.
                ", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

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
                Helper.ThrowArgNull(nameof(text));
            }

            return Evaluate(text, _horizontalRules, RuleEvaluator, defaultHandler);
        }

        /// <summary>
        /// Single line separator.
        /// </summary>
        private Rule RuleEvaluator(Match match)
        {
            if (match is null)
            {
                Helper.ThrowArgNull(nameof(match));
            }

            switch (match.Groups[1].Value)
            {
                default:
                case "-":
                    return new Rule(RuleType.Single);

                case "=":
                    return new Rule(RuleType.TwoLines);

                case "*":
                    return new Rule(RuleType.Bold);

                case "_":
                    return new Rule(RuleType.BoldWithSingle);
            }
        }

        #endregion


        #region grammer - list

        // `alphabet order` and `roman number` must start 'a.'～'c.' and 'i,'～'iii,'.
        // This restrict is avoid to treat "Yes," as list marker.
        private const string _firstListMaker = @"(?:[*+=-]|\d+[.]|[a-c][.]|[i]{1,3}[,]|[A-C][.]|[I]{1,3}[,])";
        private const string _subseqListMaker = @"(?:[*+=-]|\d+[.]|[a-c][.]|[cdilmvx]+[,]|[A-C][.]|[CDILMVX]+[,])";

        //private const string _markerUL = @"[*+=-]";
        //private const string _markerOL = @"\d+[.]|\p{L}+[.,]";

        // Unordered List
        private const string _markerUL_Disc = @"[*]";
        private const string _markerUL_Box = @"[+]";
        private const string _markerUL_Circle = @"[-]";
        private const string _markerUL_Square = @"[=]";

        // Ordered List
        private const string _markerOL_Number = @"\d+[.]";
        private const string _markerOL_LetterLower = @"[a-c][.]";
        private const string _markerOL_LetterUpper = @"[A-C][.]";
        private const string _markerOL_RomanLower = @"[cdilmvx]+[,]";
        private const string _markerOL_RomanUpper = @"[CDILMVX]+[,]";

        private int _listLevel;

        /// <summary>
        /// Maximum number of levels a single list can have.
        /// In other words, _listDepth - 1 is the maximum number of nested lists.
        /// </summary>
        private const int _listDepth = 4;

        private static readonly string _wholeList = string.Format(@"
            (?<whltxt>                      # whole list
              (?<mkr_i>                     # list marker with indent
                (?![ ]{{0,3}}(?<hrm>[-=*_])([ ]{{0,2}}\k<hrm>){{2,}})
                (?<idt>[ ]{{0,{2}}})
                (?<mkr>{0})                 # first list item marker
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
                    {1}[ ]+
                  )
              )
            )", _firstListMaker, _subseqListMaker, _listDepth - 1);

        private static readonly Regex _startNoIndentRule = new Regex(@"\A[ ]{0,2}(?<hrm>[-=*_])([ ]{0,2}\k<hrm>){2,}",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private static readonly Regex _startNoIndentSublistMarker = new Regex(@"\A" + _subseqListMaker, RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private static readonly Regex _startQuoteOrHeader = new Regex(@"\A(\#{1,6}[ ]|>|```)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private static readonly Regex _listNested = new Regex(@"^" + _wholeList,
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private static readonly Regex _listTopLevel = new Regex(@"(?:(?<=\n)|\A\n?)" + _wholeList,
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private IEnumerable<Control> ListEvaluator(Match match)
        {
            if (match is null)
            {
                Helper.ThrowArgNull(nameof(match));
            }

            // Check text marker style.
            (TextMarkerStyle textMarker, string markerPattern, int indentAppending)
                = GetTextMarkerStyle(match.Groups["mkr"].Value);

            Regex markerRegex = new Regex(@"\A" + markerPattern, RegexOptions.Compiled);

            // count indent from first marker with indent
            int countIndent = TextUtil.CountIndent(match.Groups["mkr_i"].Value);

            // whole list
            string[] whileListLins = match.Groups["whltxt"].Value.Split('\n');

            // collect detendentable line
            var listBulder = new StringBuilder();
            var outerListBuildre = new StringBuilder();
            var isInOuterList = false;
            foreach (var line in whileListLins)
            {
                if (!isInOuterList)
                {
                    if (String.IsNullOrEmpty(line))
                    {
                        listBulder.Append("").Append("\n");
                    }
                    else if (TextUtil.TryDetendLine(line, countIndent, out var stripedLine))
                    {
                        // is it horizontal line?
                        if (_startNoIndentRule.IsMatch(stripedLine))
                        {
                            isInOuterList = true;
                        }
                        // is it header or blockquote?
                        else if (_startQuoteOrHeader.IsMatch(stripedLine))
                        {
                            isInOuterList = true;
                        }
                        // is it had list marker?
                        else if (_startNoIndentSublistMarker.IsMatch(stripedLine))
                        {
                            // is it same marker as now processed?
                            var targetMarkerMch = markerRegex.Match(stripedLine);
                            if (targetMarkerMch.Success)
                            {
                                listBulder.Append(stripedLine).Append("\n");
                            }
                            else isInOuterList = true;
                        }
                        else
                        {
                            var detentedline = TextUtil.DetentLineBestEffort(stripedLine, indentAppending);
                            listBulder.Append(detentedline).Append("\n");
                        }
                    }
                    else isInOuterList = true;
                }

                if (isInOuterList)
                {
                    outerListBuildre.Append(line).Append("\n");
                }
            }

            string list = listBulder.ToString();

            IEnumerable<Control> listItems = ProcessListItems(list, markerPattern);

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            CTextBlock FindFirstFrom(IControl ctrl)
            {
                if (ctrl is IPanel pnl)
                {
                    foreach (var chld in pnl.Children)
                    {
                        var res = FindFirstFrom(chld);
                        if (res != null) return res;
                    }
                }
                if (ctrl is CTextBlock ctxt)
                {
                    return ctxt;
                }
                return null;
            }

            foreach (Tuple<Control, int> listItemTpl in listItems.Select((elm, idx) => Tuple.Create(elm, idx)))
            {
                var index = listItemTpl.Item2;
                CTextBlock markerTxt = new CTextBlock(textMarker.CreateMakerText(index));

                var control = listItemTpl.Item1;
                CTextBlock controlTxt = FindFirstFrom(control);

                markerTxt.ObserveBaseHeightOf(controlTxt);

                grid.RowDefinitions.Add(new RowDefinition());
                grid.Children.Add(markerTxt);
                grid.Children.Add(control);

                markerTxt.TextAlignment = TextAlignment.Right;
                markerTxt.TextWrapping = TextWrapping.NoWrap;
                markerTxt.Classes.Add(ListMarkerClass);
                Grid.SetRow(markerTxt, index);
                Grid.SetColumn(markerTxt, 0);

                Grid.SetRow(control, index);
                Grid.SetColumn(control, 1);
            }

            grid.Classes.Add(ListClass);

            yield return grid;


            if (outerListBuildre.Length != 0)
            {
                foreach (var ctrl in RunBlockGamut(outerListBuildre.ToString(), true))
                    yield return ctrl;
            }
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
                Helper.ThrowArgNull(nameof(match));
            }

            string item = match.Groups[4].Value;
            string leadingLine = match.Groups[1].Value;

            if (!String.IsNullOrEmpty(leadingLine) || Regex.IsMatch(item, @"\n{2,}"))
                // we could correct any bad indentation here..

                return Create<StackPanel, Control>(RunBlockGamut(item, false));
            else
            {
                // recursion for sub-lists
                return Create<StackPanel, Control>(RunBlockGamut(item, false));
            }
        }

        /// <summary>
        /// Get the text marker style based on a specific regex.
        /// </summary>
        /// <param name="markerText">list maker (eg. * + 1. a. </param>
        /// <returns>
        ///     1; return Type. 
        ///     2: match regex pattern
        ///     3: char length of listmaker
        /// </returns>
        private static (TextMarkerStyle, string, int) GetTextMarkerStyle(string markerText)
        {
            if (Regex.IsMatch(markerText, _markerUL_Disc))
            {
                return (TextMarkerStyle.Disc, _markerUL_Disc, 2);
            }
            else if (Regex.IsMatch(markerText, _markerUL_Box))
            {
                return (TextMarkerStyle.Box, _markerUL_Box, 2);
            }
            else if (Regex.IsMatch(markerText, _markerUL_Circle))
            {
                return (TextMarkerStyle.Circle, _markerUL_Circle, 2);
            }
            else if (Regex.IsMatch(markerText, _markerUL_Square))
            {
                return (TextMarkerStyle.Square, _markerUL_Square, 2);
            }
            else if (Regex.IsMatch(markerText, _markerOL_Number))
            {
                return (TextMarkerStyle.Decimal, _markerOL_Number, 3);
            }
            else if (Regex.IsMatch(markerText, _markerOL_LetterLower))
            {
                return (TextMarkerStyle.LowerLatin, _markerOL_LetterLower, 3);
            }
            else if (Regex.IsMatch(markerText, _markerOL_LetterUpper))
            {
                return (TextMarkerStyle.UpperLatin, _markerOL_LetterUpper, 3);
            }
            else if (Regex.IsMatch(markerText, _markerOL_RomanLower))
            {
                return (TextMarkerStyle.LowerRoman, _markerOL_RomanLower, 3);
            }
            else if (Regex.IsMatch(markerText, _markerOL_RomanUpper))
            {
                return (TextMarkerStyle.UpperRoman, _markerOL_RomanUpper, 3);
            }

            Helper.ThrowInvalidOperation("sorry library manager forget to modify about listmerker.");
            // dummy
            return (TextMarkerStyle.Disc, _markerUL_Disc, 2);
        }

        #endregion


        #region grammer - table

        private static readonly Regex _table = new Regex(@"
            (                               # whole table
                [ \n]*
                (?<hdr>                     # table header
                    ([^\n\|]*\|[^\n]+)
                )
                [ ]*\n[ ]*
                (?<col>                     # column style
                    \|?([ ]*:?-+:?[ ]*(\||$))+
                )
                (?<row>                     # table row
                    (
                        [ ]*\n[ ]*
                        ([^\n\|]*\|[^\n]+)
                    )+
                )
            )",
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        public IEnumerable<Control> DoTable(string text, Func<string, IEnumerable<Control>> defaultHandler)
        {
            if (text is null)
            {
                Helper.ThrowArgNull(nameof(text));
            }

            return Evaluate(text, _table, TableEvalutor, defaultHandler);
        }

        private Border TableEvalutor(Match match)
        {
            if (match is null)
            {
                Helper.ThrowArgNull(nameof(match));
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

            var mdtable = new TextileTable(
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

        private IEnumerable<Border> CreateTableRow(IList<ITableCell> mdcells, int rowIdx)
        {
            foreach (var mdcell in mdcells)
            {
                var cell = new Border();

                if (!(mdcell.Text is null))
                {
                    var txtbx = new CTextBlock(RunSpanGamut(mdcell.Text));
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

        private static Regex _codeBlockFirst = new Regex(@"
                    ^          # Character before opening
                    [ ]{0,3}
                    (`{3,})          # $1 = Opening run of `
                    ([^\n`]*)        # $2 = The code lang
                    \n
                    ((.|\n)+?)       # $3 = The code block
                    \n[ ]*
                    \1
                    (?!`)[\n]+", RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.Compiled);

        private static Regex _indentCodeBlock = new Regex(@"
                    (?:\A|^[ ]*\n)
                    (
                    [ ]{4}.+
                    (\n([ ]{4}.+|[ ]*))*
                    \n?
                    )
                    ", RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.Compiled);

        private IEnumerable<Control> DoIndentCodeBlock(string text, Func<string, IEnumerable<Control>> defaultHandler)
        {
            if (text is null)
            {
                Helper.ThrowArgNull(nameof(text));
            }

            return Evaluate(text, _indentCodeBlock, CodeBlocksWithoutLangEvaluator, defaultHandler);
        }

        private Border CodeBlocksWithLangEvaluator(Match match)
            => CodeBlocksEvaluator(match.Groups[2].Value, match.Groups[3].Value);

        private Border CodeBlocksWithoutLangEvaluator(Match match)
        {
            var detentTxt = String.Join("\n", match.Groups[1].Value.Split('\n').Select(line => TextUtil.DetentLineBestEffort(line, 4)));
            return CodeBlocksEvaluator(null, _newlinesLeadingTrailing.Replace(detentTxt, ""));
        }



        private Border CodeBlocksEvaluator(string lang, string code)
        {
            if (String.IsNullOrEmpty(lang))
            {
                var ctxt = new TextBlock()
                {
                    Text = code,
                    TextWrapping = TextWrapping.NoWrap
                };
                ctxt.Classes.Add(CodeBlockClass);

                var scrl = new ScrollViewer();
                scrl.Classes.Add(CodeBlockClass);
                scrl.Content = ctxt;
                scrl.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

                var result = new Border();
                result.Classes.Add(CodeBlockClass);
                result.Child = scrl;

                return result;
            }
            else
            {
                // check wheither style is set
                if (!ThemeDetector.IsAvalonEditSetup)
                {
                    var aeStyle = new StyleInclude(new Uri("avares://Markdown.Avalonia/"))
                    {
                        Source = new Uri("avares://AvaloniaEdit/AvaloniaEdit.xaml")
                    };
                    Application.Current.Styles.Add(aeStyle);
                }

                var txtEdit = new TextEditor();
                var highlight = HighlightingManager.Instance.GetDefinitionByExtension("." + lang);
                txtEdit.Tag = lang;
                //txtEdit.SetValue(TextEditor.SyntaxHighlightingProperty, highlight);

                txtEdit.Text = code;
                txtEdit.HorizontalAlignment = HorizontalAlignment.Stretch;
                txtEdit.IsReadOnly = true;

                var result = new Border();
                result.Classes.Add(CodeBlockClass);
                result.Child = txtEdit;

                return result;
            }
        }

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
                Helper.ThrowArgNull(nameof(text));
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
                Helper.ThrowArgNull(nameof(match));
            }

            string span = match.Groups[2].Value;
            span = Regex.Replace(span, @"^[ ]*", ""); // leading whitespace
            span = Regex.Replace(span, @"[ ]*$", ""); // trailing whitespace

            var result = new CCode(new[] { new CRun() { Text = span } });

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
                Helper.ThrowArgNull(nameof(text));
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

                void HandleBefore()
                {
                    if (buff.Length > 0)
                    {
                        rtn.AddRange(defaultHandler(buff.ToString()));
                        buff.Clear();
                    }
                }

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
                                    HandleBefore();
                                    rtn.Add(inline);
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
                                    HandleBefore();
                                    rtn.Add(inline);
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
                                    HandleBefore();
                                    rtn.Add(inline);
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
                                    HandleBefore();
                                    rtn.Add(inline);
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
                Helper.ThrowArgNull(nameof(match));
            }

            var content = match.Groups[contentGroup].Value;

            return new CItalic(RunSpanGamut(content));
        }

        private CBold BoldEvaluator(Match match, int contentGroup)
        {
            if (match is null)
            {
                Helper.ThrowArgNull(nameof(match));
            }

            var content = match.Groups[contentGroup].Value;

            return new CBold(RunSpanGamut(content));
        }

        private CStrikethrough StrikethroughEvaluator(Match match, int contentGroup)
        {
            if (match is null)
            {
                Helper.ThrowArgNull(nameof(match));
            }

            var content = match.Groups[contentGroup].Value;

            return new CStrikethrough(RunSpanGamut(content));
        }

        private CUnderline UnderlineEvaluator(Match match, int contentGroup)
        {
            if (match is null)
            {
                Helper.ThrowArgNull(nameof(match));
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
                Helper.ThrowArgNull(nameof(text));
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
            [\n]*
            ([>].*)
            (\n[>].*)*
            [\n]*
            ", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private static Regex _blockquoteFirst = new Regex(@"
            ^
            ([>].*)
            (\n[>].*)*
            [\n]*
            ", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private IEnumerable<Control> DoBlockquotes(string text, Func<string, IEnumerable<Control>> defaultHandler)
        {
            if (text is null)
            {
                Helper.ThrowArgNull(nameof(text));
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
                Helper.ThrowArgNull(nameof(match));
            }

            // trim '>'
            var trimmedTxt = string.Join(
                    "\n",
                    match.Value.Trim().Split('\n')
                        .Select(txt =>
                        {
                            if (txt.Length <= 1) return string.Empty;
                            var trimmed = txt.Substring(1);
                            if (trimmed.FirstOrDefault() == ' ') trimmed = trimmed.Substring(1);
                            return trimmed;
                        })
                        .ToArray()
            );

            var blocks = RunBlockGamut(trimmedTxt + "\n", true);

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
                       [^()\n\t]+? # Anything other than parens or whitespace
                     |
                       \(
                           ", _nestDepth) + RepeatString(
                    @" \)
                    )*?"
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
                Helper.ThrowArgNull(nameof(text));
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
        private IEnumerable<T> Evaluate2<T>(
                string text,
                Regex expression1, Func<Match, T> build1,
                Regex expression2, Func<Match, IEnumerable<T>> build2,
                Func<string, IEnumerable<T>> rest)
        {
            if (text is null)
            {
                Helper.ThrowArgNull(nameof(text));
            }

            var index = 0;

            var rtn = new List<T>();

            var match1 = expression1.Match(text, index);
            var match2 = expression2.Match(text, index);

            IEnumerable<T> ProcPre(Match m)
            {
                var prefix = text.Substring(index, m.Index - index);
                return rest(prefix);
            }

            void ProcessMatch1()
            {
                if (match1.Index > index)
                {
                    rtn.AddRange(ProcPre(match1));
                }
                rtn.Add(build1(match1));
                index = match1.Index + match1.Length;
            }

            void ProcessMatch2()
            {
                if (match2.Index > index)
                {
                    rtn.AddRange(ProcPre(match2));
                }
                rtn.AddRange(build2(match2));
                index = match2.Index + match2.Length;
            }

            // match1 vs match2
            while (match1.Success && match2.Success)
            {
                if (match1.Index < match2.Index)
                {
                    ProcessMatch1();
                }
                else
                {
                    ProcessMatch2();
                }
                match1 = expression1.Match(text, index);
                match2 = expression2.Match(text, index);
            }

            while (match1.Success)
            {
                ProcessMatch1();
                match1 = expression1.Match(text, index);
            }

            while (match2.Success)
            {
                ProcessMatch2();
                match2 = expression2.Match(text, index);
            }

            if (index < text.Length)
            {
                var suffix = text.Substring(index, text.Length - index);
                rtn.AddRange(rest(suffix));
            }

            return rtn;
        }

        private IEnumerable<T> Evaluate<T>(string text, Regex expression, Func<Match, T> build, Func<string, IEnumerable<T>> rest)
        {
            if (text is null)
            {
                Helper.ThrowArgNull(nameof(text));
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