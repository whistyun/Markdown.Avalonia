using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using ColorTextBlock.Avalonia;
using Markdown.Avalonia.Controls;
using Markdown.Avalonia.Parsers;
using Markdown.Avalonia.Plugins;
using Markdown.Avalonia.Tables;
using Markdown.Avalonia.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
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
        public const string ContainerBlockClass = "ContainerBlock";
        public const string NoContainerClass = "NoContainer";
        public const string BlockquoteClass = "Blockquote";
        public const string NoteClass = "Note";

        public const string ParagraphClass = "Paragraph";

        public const string TableClass = "Table";
        public const string TableHeaderClass = "TableHeader";
        public const string TableFirstRowClass = "FirstTableRow";
        public const string TableRowOddClass = "OddTableRow";
        public const string TableRowEvenClass = "EvenTableRow";
        public const string TableLastRowClass = "LastTableRow";

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
            set
            {
                _assetPathRoot = value;
#pragma warning disable CS0618
                if (BitmapLoader is not null)
                    BitmapLoader.AssetPathRoot = value;
#pragma warning restore CS0618
                if (_setupInfo is not null)
                    _setupInfo.PathResolver.AssetPathRoot = value;
            }
        }

        private string[] _assetAssemblyNames;
        public IEnumerable<string> AssetAssemblyNames => _assetAssemblyNames;

        private ICommand? _hyperlinkCommand;
        /// <inheritdoc/>
        public ICommand? HyperlinkCommand
        {
            get => _hyperlinkCommand ?? _setupInfo?.HyperlinkCommand;
            set
            {
                _hyperlinkCommand = value;
            }
        }

        public MdAvPlugins Plugins { get; set; }

        [Obsolete]
        private IBitmapLoader? _loader;
        /// <inheritdoc/>
        [Obsolete("Please use Plugins propety. see https://github.com/whistyun/Markdown.Avalonia/wiki/How-to-migrages-to-ver11")]
        public IBitmapLoader? BitmapLoader
        {
            get => _loader;
            set
            {
                _loader = value;
                if (_loader is not null)
                {
                    _loader.AssetPathRoot = _assetPathRoot;
                }
            }
        }

        private IContainerBlockHandler? _containerBlockHandler;
        public IContainerBlockHandler? ContainerBlockHandler
        {
            get => _containerBlockHandler ?? _setupInfo?.ContainerBlock;
            set
            {
                _containerBlockHandler = value;
            }
        }

        public CascadeDictionary CascadeResources { get; } = new CascadeDictionary();

        public IResourceDictionary Resources
        {
            get => CascadeResources.Owner;
            set => CascadeResources.Owner = value;
        }

        public bool UseResource { get; set; }

        #region dependencyobject property

        public static readonly DirectProperty<Markdown, ICommand?> HyperlinkCommandProperty =
            AvaloniaProperty.RegisterDirect<Markdown, ICommand?>(nameof(HyperlinkCommand),
                mdEng => mdEng.HyperlinkCommand,
                (mdEng, command) => mdEng.HyperlinkCommand = command);

        [Obsolete("Please use Plugins propety. see https://github.com/whistyun/Markdown.Avalonia/wiki/How-to-migrages-to-ver11")]
        public static readonly DirectProperty<Markdown, IBitmapLoader?> BitmapLoaderProperty =
            AvaloniaProperty.RegisterDirect<Markdown, IBitmapLoader?>(nameof(BitmapLoader),
                mdEng => mdEng.BitmapLoader,
                (mdEng, loader) => mdEng.BitmapLoader = loader);

        #endregion

        #region ParseInfo

        private SetupInfo _setupInfo;
        private BlockParser[] _topBlockParsers;
        private BlockParser[] _blockParsers;
        private InlineParser[] _inlines;
        private bool _supportTextAlignment;
        private bool _supportStrikethrough;
        private bool _supportTextileInline;

        #endregion

        public Markdown()
        {
            _assetPathRoot = Environment.CurrentDirectory;

            var stack = new StackTrace();
            _assetAssemblyNames = stack.GetFrames()
                            .Select(frm => frm?.GetMethod()?.DeclaringType?.Assembly?.GetName()?.Name)
                            .OfType<string>()
                            .Where(name => !name.Equals("Markdown.Avalonia"))
                            .Distinct()
                            .ToArray();

            Plugins = new MdAvPlugins();

            _setupInfo = null!;
            _topBlockParsers = null!;
            _blockParsers = null!;
            _inlines = null!;
            SetupParser();
        }

        private void SetupParser()
        {
            var info = Plugins.Info;
            if (ReferenceEquals(info, _setupInfo))
                return;

            var topBlocks = new List<BlockParser>();
            var subBlocks = new List<BlockParser>();
            var inlines = new List<InlineParser>();


            // top-level block parser

            if (info.EnableListMarkerExt)
            {
                topBlocks.Add(BlockParser.New(_extListNested, nameof(ListEvaluator), ExtListEvaluator));
            }
            else
            {
                topBlocks.Add(BlockParser.New(_commonListNested, nameof(ListEvaluator), CommonListEvaluator));
            }

            topBlocks.Add(BlockParser.New(_codeBlockBegin, nameof(CodeBlocksWithLangEvaluator), CodeBlocksWithLangEvaluator));

            if (info.EnableContainerBlockExt)
            {
                topBlocks.Add(BlockParser.New(_containerBlockFirst, nameof(ContainerBlockEvaluator), ContainerBlockEvaluator));
            }


            // sub-level block parser

            subBlocks.Add(BlockParser.New(_blockquoteFirst, nameof(BlockquotesEvaluator), BlockquotesEvaluator));
            subBlocks.Add(BlockParser.New(_headerSetext, nameof(SetextHeaderEvaluator), SetextHeaderEvaluator));
            subBlocks.Add(BlockParser.New(_headerAtx, nameof(AtxHeaderEvaluator), AtxHeaderEvaluator));

            if (info.EnableRuleExt)
            {
                subBlocks.Add(BlockParser.New(_horizontalRules, nameof(RuleEvaluator), RuleEvaluator));
            }
            else
            {
                subBlocks.Add(BlockParser.New(_horizontalCommonRules, nameof(RuleEvaluator), RuleCommonEvaluator));
            }

            if (info.EnableTableBlock)
            {
                subBlocks.Add(BlockParser.New(_table, nameof(TableEvalutor), TableEvalutor));
            }

            if (info.EnableNoteBlock)
            {
                subBlocks.Add(BlockParser.New(_note, nameof(NoteEvaluator), NoteEvaluator));
            }

            subBlocks.Add(BlockParser.New(_indentCodeBlock, nameof(CodeBlocksWithoutLangEvaluator), CodeBlocksWithoutLangEvaluator));


            // inline parser

            inlines.Add(InlineParser.New(_codeSpan, nameof(CodeSpanEvaluator), CodeSpanEvaluator));
            inlines.Add(InlineParser.New(_imageOrHrefInline, nameof(ImageOrHrefInlineEvaluator), ImageOrHrefInlineEvaluator));

            if (StrictBoldItalic)
            {
                inlines.Add(InlineParser.New(_strictBold, nameof(BoldEvaluator), BoldEvaluator));
                inlines.Add(InlineParser.New(_strictItalic, nameof(ItalicEvaluator), ItalicEvaluator));

                if (info.EnableStrikethrough)
                    inlines.Add(InlineParser.New(_strikethrough, nameof(StrikethroughEvaluator), StrikethroughEvaluator));
            }


            // parser registered by plugin

            topBlocks.AddRange(info.TopBlock);
            subBlocks.AddRange(info.Block);
            inlines.AddRange(info.Inline);


            // inform path info to resolver
            info.PathResolver.AssetPathRoot = AssetPathRoot;
            info.PathResolver.CallerAssemblyNames = AssetAssemblyNames;

            info.Overwrite(_hyperlinkCommand);
            info.Overwrite(_containerBlockHandler);
            info.Overwrite(_loader);


            _topBlockParsers = topBlocks.Select(p => info.Override(p)).ToArray();
            _blockParsers = subBlocks.Select(p => info.Override(p)).ToArray();
            _inlines = inlines.ToArray();
            _supportTextAlignment = info.EnableTextAlignment;
            _supportStrikethrough = info.EnableStrikethrough;
            _supportTextileInline = info.EnableTextileInline;
            _setupInfo = info;
        }


        /// <inheritdoc/>
        public Control Transform(string? text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            SetupParser();

            text = TextUtil.Normalize(text, _tabWidth);

            var status = new ParseStatus(true & _supportTextAlignment);
            var document = Create<StackPanel, Control>(PrivateRunBlockGamut(text, status));
            document.Orientation = Orientation.Vertical;

            return document;
        }

        /// <summary>
        /// Perform transformations that form block-level tags like paragraphs, headers, and list items.
        /// </summary>
        public IEnumerable<Control> RunBlockGamut(string? text, ParseStatus status)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            SetupParser();

            text = TextUtil.Normalize(text, _tabWidth);

            return PrivateRunBlockGamut(text, status);
        }

        /// <summary>
        /// Perform transformations that occur *within* block-level tags like paragraphs, headers, and list items.
        /// </summary>
        public IEnumerable<CInline> RunSpanGamut(string? text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            SetupParser();

            text = TextUtil.Normalize(text, _tabWidth);

            return PrivateRunSpanGamut(text);
        }

        private IEnumerable<Control> PrivateRunBlockGamut(string text, ParseStatus status)
        {
            var index = 0;
            var length = text.Length;
            var rtn = new List<Control>();

            var candidates = new List<Candidate<BlockParser>>();

            for (; ; )
            {
                candidates.Clear();

                foreach (var parser in _topBlockParsers)
                {
                    var match = parser.Pattern.Match(text, index, length);
                    if (match.Success) candidates.Add(new Candidate<BlockParser>(match, parser));
                }

                if (candidates.Count == 0) break;

                candidates.Sort();

                int bestBegin = 0;
                int bestEnd = 0;
                IEnumerable<Control>? result = null;

                foreach (var c in candidates)
                {
                    result = c.Parser.Convert(text, c.Match, status, this, out bestBegin, out bestEnd);
                    if (result is not null) break;
                }

                if (result is null) break;

                if (bestBegin > index)
                {
                    RunBlockRest(text, index, bestBegin - index, status, 0, rtn);
                }

                rtn.AddRange(result);

                length -= bestEnd - index;
                index = bestEnd;
            }

            if (index < text.Length)
            {
                RunBlockRest(text, index, text.Length - index, status, 0, rtn);
            }

            return rtn;


            void RunBlockRest(
               string text, int index, int length,
               ParseStatus status,
                int parserStart,
               List<Control> outto)
            {
                for (; parserStart < _blockParsers.Length; ++parserStart)
                {
                    var parser = _blockParsers[parserStart];

                    for (; ; )
                    {
                        var match = parser.Pattern.Match(text, index, length);
                        if (!match.Success) break;

                        var rslt = parser.Convert(text, match, status, this, out int parseBegin, out int parserEnd);
                        if (rslt is null) break;

                        if (parseBegin > index)
                        {
                            RunBlockRest(text, index, parseBegin - index, status, parserStart + 1, outto);
                        }
                        outto.AddRange(rslt);

                        length -= parserEnd - index;
                        index = parserEnd;
                    }

                    if (length == 0) break;
                }

                if (length != 0)
                {
                    outto.AddRange(FormParagraphs(text.Substring(index, length), status));
                }
            }
        }

        private IEnumerable<CInline> PrivateRunSpanGamut(string text)
        {
            var rtn = new List<CInline>();
            RunSpanRest(text, 0, text.Length, 0, rtn);
            return rtn;

            void RunSpanRest(
                string text, int index, int length,
                int parserStart,
                List<CInline> outto)
            {
                for (; parserStart < _inlines.Length; ++parserStart)
                {
                    var parser = _inlines[parserStart];

                    for (; ; )
                    {
                        var match = parser.Pattern.Match(text, index, length);
                        if (!match.Success) break;

                        var rslt = parser.Convert(text, match, this, out int parseBegin, out int parserEnd);
                        if (rslt is null) break;

                        if (parseBegin > index)
                        {
                            RunSpanRest(text, index, parseBegin - index, parserStart + 1, outto);
                        }
                        outto.AddRange(rslt);

                        length -= parserEnd - index;
                        index = parserEnd;
                    }

                    if (length == 0) break;
                }

                if (length != 0)
                {
                    var subtext = text.Substring(index, length);

                    outto.AddRange(
                        StrictBoldItalic ?
                            DoText(subtext) :
                            DoTextDecorations(subtext, s => DoText(s)));
                }
            }
        }


        #region grammer - paragraph

        private static readonly Regex _align = new(@"^p([<=>])\.", RegexOptions.Compiled);
        private static readonly Regex _newlinesLeadingTrailing = new(@"^\n+|\n+\z", RegexOptions.Compiled);
        private static readonly Regex _newlinesMultiple = new(@"\n{2,}", RegexOptions.Compiled);

        /// <summary>
        /// splits on two or more newlines, to form "paragraphs";    
        /// </summary>
        private IEnumerable<Control> FormParagraphs(string text, ParseStatus status)
        {
            var trimemdText = _newlinesLeadingTrailing.Replace(text, "");

            string[] grafs = trimemdText == "" ?
                new string[0] :
                _newlinesMultiple.Split(trimemdText);

            foreach (var g in grafs)
            {
                var chip = g;

                TextAlignment? indiAlignment = null;

                if (status.SupportTextAlignment)
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

                var ctbox = new CTextBlock(PrivateRunSpanGamut(chip));

                if (indiAlignment.HasValue)
                    ctbox.TextAlignment = indiAlignment.Value;

                ctbox.Classes.Add(ParagraphClass);

                yield return ctbox;
            }
        }

        #endregion

        #region grammer - image or href

        private static readonly Regex _imageOrHrefInline = new(string.Format(@"
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


        private CInline ImageOrHrefInlineEvaluator(Match match)
        {
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
            string linkText = match.Groups[3].Value;
            string url = match.Groups[4].Value;
            string title = match.Groups[7].Value;

            var link = new CHyperlink(PrivateRunSpanGamut(linkText))
            {
                Command = (urlTxt) =>
                {
                    if (HyperlinkCommand != null && HyperlinkCommand.CanExecute(urlTxt))
                    {
                        HyperlinkCommand.Execute(urlTxt);
                    }
                },

                CommandParameter = url
            };

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

            return LoadImage(urlTxt, title);
        }

        private CInline LoadImage(string urlTxt, string title)
        {
            if (UseResource && CascadeResources.TryGet(urlTxt, out var resourceVal))
            {
                if (resourceVal is Control control)
                {
                    return new CInlineUIContainer(control);
                }

                CImage? cimg = null;
                if (resourceVal is Bitmap renderedImage)
                {
                    cimg = new CImage(renderedImage);
                }
                if (resourceVal is IEnumerable<Byte> byteEnum)
                {
                    try
                    {
                        using (var memstream = new MemoryStream(byteEnum.ToArray()))
                        {
                            var bitmap = new Bitmap(memstream);
                            cimg = new CImage(bitmap);
                        }
                    }
                    catch { }
                }

                if (cimg is not null)
                {
                    if (!String.IsNullOrEmpty(title)
                        && !title.Any(ch => !Char.IsLetterOrDigit(ch)))
                    {
                        cimg.Classes.Add(title);
                    }
                    return cimg;
                }
            }

            CImage image = _setupInfo.LoadImage(urlTxt);

            if (!String.IsNullOrEmpty(title)
                && !title.Any(ch => !Char.IsLetterOrDigit(ch)))
            {
                image.Classes.Add(title);
            }

            return image;
        }


        #endregion

        #region grammer - header

        /// <summary>
        /// Turn Markdown headers into HTML header tags
        /// </summary>
        /// <remarks>
        /// Header 1  
        /// ========  
        /// 
        /// Header 2  
        /// --------  
        /// </remarks>
        private static readonly Regex _headerSetext = new(@"
                ^(.+?)
                [ ]*
                \n
                (=+|-+)     # $1 = string of ='s or -'s
                [ ]*
                \n+",
                RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        /// <summary>
        /// # Header 1  
        /// ## Header 2  
        /// ## Header 2 with closing hashes ##  
        /// ...  
        /// ###### Header 6  
        /// </remarks>
        private static readonly Regex _headerAtx = new(@"
                ^(\#{1,6})  # $1 = string of #'s
                [ ]*
                (.+?)       # $2 = Header text
                [ ]*
                \#*         # optional closing #'s (not counted)
                \n+",
                RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private CTextBlock SetextHeaderEvaluator(Match match)
        {
            string header = match.Groups[1].Value;
            int level = match.Groups[2].Value.StartsWith("=") ? 1 : 2;

            //TODO: Style the paragraph based on the header level
            return CreateHeader(level, PrivateRunSpanGamut(header.Trim()));
        }

        private CTextBlock AtxHeaderEvaluator(Match match)
        {
            string header = match.Groups[2].Value;
            int level = match.Groups[1].Value.Length;
            return CreateHeader(level, PrivateRunSpanGamut(header));
        }

        private CTextBlock CreateHeader(int level, IEnumerable<CInline> content)
        {
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

        /// <summary>
        /// Turn Markdown into HTML paragraphs.
        /// </summary>
        /// <remarks>
        /// < Note
        /// </remarks>
        private static readonly Regex _note = new(@"
                ^(\<)       # $1 = starting marker <
                [ ]*
                (.+?)       # $2 = Header text
                [ ]*
                \>*         # optional closing >'s (not counted)
                \n+
            ", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private Border NoteEvaluator(Match match, ParseStatus status)
        {
            string text = match.Groups[2].Value;

            TextAlignment? indiAlignment = null;

            if (status.SupportTextAlignment)
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

            return NoteComment(PrivateRunSpanGamut(text), indiAlignment);
        }

        private Border NoteComment(IEnumerable<CInline> content, TextAlignment? indiAlignment)
        {
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

        /// <summary>
        /// Turn Markdown horizontal rules into HTML hr tags
        /// </summary>
        /// <remarks>
        /// ***  
        /// * * *  
        /// ---
        /// - - -
        /// </remarks>
        private static readonly Regex _horizontalRules = new(@"
                ^[ ]{0,3}                   # Leading space
                    ([-=*_])                # $1: First marker ([markers])
                    (?>                     # Repeated marker group
                        [ ]{0,2}            # Zero, one, or two spaces.
                        \1                  # Marker character
                    ){2,}                   # Group repeated at least twice
                    [ ]*                    # Trailing spaces
                    \n                      # End of line.
                ", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private static readonly Regex _horizontalCommonRules = new(@"
                ^[ ]{0,3}                   # Leading space
                    ([-*_])                 # $1: First marker ([markers])
                    (?>                     # Repeated marker group
                        [ ]{0,2}            # Zero, one, or two spaces.
                        \1                  # Marker character
                    ){2,}                   # Group repeated at least twice
                    [ ]*                    # Trailing spaces
                    \n                      # End of line.
                ", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        /// <summary>
        /// Single line separator.
        /// </summary>
        private Rule RuleEvaluator(Match match)
        {
            return match.Groups[1].Value switch
            {
                "=" => new Rule(RuleType.TwoLines),
                "*" => new Rule(RuleType.Bold),
                "_" => new Rule(RuleType.BoldWithSingle),
                "-" => new Rule(RuleType.Single),
                _ => new Rule(RuleType.Single),
            };
        }

        private Rule RuleCommonEvaluator(Match match)
        {
            return new Rule(RuleType.Single);
        }
        #endregion


        #region grammer - list

        // `alphabet order` and `roman number` must start 'a.'～'c.' and 'i,'～'iii,'.
        // This restrict is avoid to treat "Yes," as list marker.
        private const string _extFirstListMaker = @"(?:[*+=-]|\d+[.]|[a-c][.]|[i]{1,3}[,]|[A-C][.]|[I]{1,3}[,])";
        private const string _extSubseqListMaker = @"(?:[*+=-]|\d+[.]|[a-c][.]|[cdilmvx]+[,]|[A-C][.]|[CDILMVX]+[,])";

        private const string _commonListMaker = @"(?:[*+-]|\d+[.])";

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

        /// <summary>
        /// Maximum number of levels a single list can have.
        /// In other words, _listDepth - 1 is the maximum number of nested lists.
        /// </summary>
        private const int _listDepth = 4;

        private static readonly string _wholeListFormat = @"
            ^
            (?<whltxt>                      # whole list
              (?<mkr_i>                     # list marker with indent
                (?![ ]{{0,3}}(?<hrm>[-=*_])([ ]{{0,2}}\k<hrm>){{2,}}[ ]*\n)
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
            )";

        private static readonly Regex _startNoIndentRule = new(@"\A[ ]{0,2}(?<hrm>[-=*_])([ ]{0,2}\k<hrm>){2,}[ ]*$",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private static readonly Regex _startNoIndentSublistMarker = new(@"\A" + _extSubseqListMaker, RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private static readonly Regex _startQuoteOrHeader = new(@"\A(\#{1,6}[ ]|>|```)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private static readonly Regex _startNoIndentCommonSublistMarker = new(@"\A" + _commonListMaker, RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private static readonly Regex _commonListNested = new(
            String.Format(_wholeListFormat, _commonListMaker, _commonListMaker, _listDepth - 1),
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private static readonly Regex _startNoIndentExtSublistMarker = new(@"\A" + _extSubseqListMaker, RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private static readonly Regex _extListNested = new(
            String.Format(_wholeListFormat, _extFirstListMaker, _extSubseqListMaker, _listDepth - 1),
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);


        private IEnumerable<Control> ExtListEvaluator(Match match)
            => ListEvaluator(match, _startNoIndentExtSublistMarker);

        private IEnumerable<Control> CommonListEvaluator(Match match)
            => ListEvaluator(match, _startNoIndentCommonSublistMarker);

        private IEnumerable<Control> ListEvaluator(Match match, Regex sublistMarker)
        {
            // Check text marker style.
            (TextMarkerStyle textMarker, string markerPattern, int indentAppending)
                = GetTextMarkerStyle(match.Groups["mkr"].Value);

            Regex markerRegex = new(@"\A" + markerPattern, RegexOptions.Compiled);

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
                        else if (sublistMarker.IsMatch(stripedLine))
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

            static CTextBlock? FindFirstFrom(Control ctrl)
            {
                if (ctrl is Panel pnl)
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
                var markerTxt = new CTextBlock(textMarker.CreateMakerText(index));

                var control = listItemTpl.Item1;
                CTextBlock? controlTxt = FindFirstFrom(control);

                // adjust baseline
                if (controlTxt is not null)
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
                foreach (var ctrl in PrivateRunBlockGamut(outerListBuildre.ToString(), ParseStatus.Init))
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

        private Control ListItemEvaluator(Match match)
        {
            string item = match.Groups[4].Value;

            var status = new ParseStatus(false);

            // we could correct any bad indentation here..
            // recursion for sub-lists
            return Create<StackPanel, Control>(PrivateRunBlockGamut(item, status));
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

        private static readonly Regex _table = new(@"
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

        private Border TableEvalutor(Match match)
        {
            var headerTxt = match.Groups["hdr"].Value.Trim();
            var styleTxt = match.Groups["col"].Value.Trim();
            var rowTxt = match.Groups["row"].Value.Trim();

            static string ExtractCoverBar(string txt)
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
            table.ColumnDefinitions = new AutoScaleColumnDefinitions(mdtable.ColCount, table);

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

                    if (rowIdx == 0)
                        cell.Classes.Add(TableFirstRowClass);
                    if (rowIdx == mdtable.Details.Count - 1)
                        cell.Classes.Add(TableLastRowClass);

                    table.Children.Add(cell);
                }
            }

            table.Classes.Add(TableClass);

            var result = new Border { Child = table };
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
                    var txtbx = new CTextBlock(PrivateRunSpanGamut(mdcell.Text));
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

        #region grammer - container block

        private static readonly Regex _containerBlockFirst = new(@"
                    ^          # Character before opening
                    [ ]{0,3}
                    (:{3,})          # $1 = Opening run of `
                    ([^\n`]*)        # $2 = The container type
                    \n
                    ((.|\n)+?)       # $3 = The code block
                    \n[ ]*
                    \1
                    (?!:)[\n]+", RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.Compiled);

        private Border ContainerBlockEvaluator(Match match)
        {
            var result = ContainerBlockHandler?.ProvideControl(AssetPathRoot, match.Groups[2].Value, match.Groups[3].Value);

            if (result is null)
            {
                Border _retVal = CodeBlocksEvaluator(match.Value);
                _retVal.Classes.Add(NoContainerClass);
                return _retVal;
            }

            result.Classes.Add(ContainerBlockClass);
            return result;
        }

        #endregion

        #region grammer - code block

        private static readonly Regex _codeBlockBegin = new(@"
                    ^          # Character before opening
                    [ ]{0,3}
                    (`{3,})          # $1 = Opening run of `
                    ([^\n`]*)        # $2 = The code lang
                    \n", RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.Compiled);


        private static readonly Regex _indentCodeBlock = new(@"
                    (?:\A|^[ ]*\n)
                    (
                    [ ]{4}.+
                    (\n([ ]{4}.+|[ ]*))*
                    \n?
                    )
                    ", RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.Compiled);

        private Border? CodeBlocksWithLangEvaluator(string text, Match match, out int parseTextBegin, out int parseTextEnd)
        {
            var closeTagPattern = new Regex($"\n[ ]*{match.Groups[1].Value}[ ]*\n");
            var closeTagMatch = closeTagPattern.Match(text, match.Index + match.Length);

            int codeEndIndex;
            if (closeTagMatch.Success)
            {
                codeEndIndex = closeTagMatch.Index;
                parseTextEnd = closeTagMatch.Index + closeTagMatch.Length;
            }
            else if (_setupInfo.EnablePreRenderingCodeBlock)
            {
                codeEndIndex = text.Length;
                parseTextEnd = text.Length;
            }
            else
            {
                parseTextBegin = parseTextEnd = -1;
                return null;
            }

            parseTextBegin = match.Index;

            string code = text.Substring(match.Index + match.Length, codeEndIndex - (match.Index + match.Length));
            return CodeBlocksEvaluator(code);
        }

        private Border CodeBlocksWithoutLangEvaluator(Match match)
        {
            var detentTxt = String.Join("\n", match.Groups[1].Value.Split('\n').Select(line => TextUtil.DetentLineBestEffort(line, 4)));
            return CodeBlocksEvaluator(_newlinesLeadingTrailing.Replace(detentTxt, ""));
        }

        private Border CodeBlocksEvaluator(string code)
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

        #endregion

        #region grammer - code

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
        private static readonly Regex _codeSpan = new(@"
                    (?<!\\)   # Character before opening ` can't be a backslash
                    (`+)      # $1 = Opening run of `
                    (.+?)     # $2 = The code block
                    (?<!`)
                    \1
                    (?!`)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

        private CCode CodeSpanEvaluator(Match match)
        {
            string span = match.Groups[2].Value;
            span = Regex.Replace(span, @"^[ ]*", ""); // leading whitespace
            span = Regex.Replace(span, @"[ ]*$", ""); // trailing whitespace

            var result = new CCode(new[] { new CRun() { Text = span } });

            return result;
        }

        #endregion

        #region grammer - textdecorations

        private static readonly Regex _strictBold = new(@"([\W_]|^) (\*\*|__) (?=\S) ([^\r]*?\S[\*_]*) \2 ([\W_]|$)",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex _strictItalic = new(@"([\W_]|^) (\*|_) (?=\S) ([^\r\*_]*?\S) \2 ([\W_]|$)",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex _strikethrough = new(@"(~~) (?=\S) (.+?) (?<=\S) \1",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex _underline = new(@"(__) (?=\S) (.+?) (?<=\S) \1",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// Turn Markdown *italics* and **bold** into HTML strong and em tags
        /// </summary>
        private IEnumerable<CInline> DoTextDecorations(string text, Func<string, IEnumerable<CInline>> defaultHandler)
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

        private CUnderline? ParseAsUnderline(string text, ref int start)
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
                var span = new CUnderline(PrivateRunSpanGamut(text.Substring(bgn, end - bgn)));
                return span;
            }
            else
            {
                start += bgnCnt - 1;
                return null;
            }
        }

        private CStrikethrough? ParseAsStrikethrough(string text, ref int start)
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
                var span = new CStrikethrough(PrivateRunSpanGamut(text.Substring(bgn, end - bgn)));
                return span;
            }
            else
            {
                start += bgnCnt - 1;
                return null;
            }
        }

        private CInline? ParseAsBoldOrItalic(string text, ref int start)
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

                            var span = new CItalic(PrivateRunSpanGamut(text.Substring(bgn, end - bgn)));
                            return span;
                        }
                    case 2: // bold
                        {
                            start = end + cnt - 1;
                            var span = new CBold(PrivateRunSpanGamut(text.Substring(bgn, end - bgn)));
                            return span;
                        }

                    default: // >3; bold-italic
                        {
                            bgn = start + 3;
                            start = end + 3 - 1;

                            var inline = new CItalic(PrivateRunSpanGamut(text.Substring(bgn, end - bgn)));
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

        private CInline? ParseAsColor(string text, ref int start)
        {
            if (start + 1 >= text.Length)
                return null;

            if (text[start + 1] != '{')
                return null;

            int end = text.IndexOf('}', start + 1);

            if (end == -1)
                return null;

            var styleTxts = text.Substring(start + 2, end - (start + 2));

            int bgnIdx = end + 1;
            int endIdx = EscapedIndexOf(text, bgnIdx, '%');

            CSpan span;
            if (endIdx == -1)
            {
                endIdx = text.Length - 1;
                span = new CSpan(PrivateRunSpanGamut(text.Substring(bgnIdx)));
            }
            else
            {
                span = new CSpan(PrivateRunSpanGamut(text.Substring(bgnIdx, endIdx - bgnIdx)));
            }

            foreach (var styleTxt in styleTxts.Split(';'))
            {
                var nameAndVal = styleTxt.Split(':');

                if (nameAndVal.Length != 2)
                    return null;

                var name = nameAndVal[0].Trim();
                var colorLbl = nameAndVal[1].Trim();

                switch (name)
                {
                    case "color":
                        try
                        {
                            var color = colorLbl.StartsWith("#") ?
                                (IBrush?)new BrushConverter().ConvertFrom(colorLbl) :
                                (IBrush?)new BrushConverter().ConvertFromString(colorLbl);

                            span.Foreground = color;
                        }
                        catch { }
                        break;

                    case "background":
                        try
                        {
                            var color = colorLbl.StartsWith("#") ?
                                (IBrush?)new BrushConverter().ConvertFrom(colorLbl) :
                                (IBrush?)new BrushConverter().ConvertFromString(colorLbl);

                            span.Background = color;
                        }
                        catch { }
                        break;

                    default:
                        return null;
                }
            }

            start = endIdx;
            return span;
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

        private CItalic ItalicEvaluator(Match match)
        {
            var content = match.Groups[3].Value;

            return new CItalic(PrivateRunSpanGamut(content));
        }

        private CBold BoldEvaluator(Match match)
        {
            var content = match.Groups[3].Value;

            return new CBold(PrivateRunSpanGamut(content));
        }

        private CStrikethrough StrikethroughEvaluator(Match match)
        {
            var content = match.Groups[2].Value;

            return new CStrikethrough(PrivateRunSpanGamut(content));
        }

        private CUnderline UnderlineEvaluator(Match match)
        {
            var content = match.Groups[2].Value;

            return new CUnderline(PrivateRunSpanGamut(content));
        }

        #endregion

        #region grammer - text

        private static readonly Regex _eoln = new("\\s+");
        private static readonly Regex _lbrk = new(@"\ {2,}\n");

        private IEnumerable<CRun> DoText(string text)
        {
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

        private static readonly Regex _blockquoteFirst = new(@"
            ^
            ([>].*)
            (\n[>].*)*
            [\n]*
            ", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private Border BlockquotesEvaluator(Match match)
        {
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

            var status = new ParseStatus(true & _supportTextAlignment);
            var blocks = PrivateRunBlockGamut(trimmedTxt + "\n", status);

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

        /// <summary>
        /// Reusable pattern to match balanced [brackets]. See Friedl's 
        /// "Mastering Regular Expressions", 2nd Ed., pp. 328-331.
        /// </summary>
        private static string GetNestedBracketsPattern()
        {
            // in other words [this] and [this[also]] and [this[also[too]]]
            // up to _nestDepth
            return RepeatString(@"
                   (?>              # Atomic matching
                      [^\[\]]+      # Anything other than brackets
                    |
                      \[
                          ", _nestDepth) + RepeatString(
                   @" \]
                   )*"
                   , _nestDepth);
        }

        /// <summary>
        /// Reusable pattern to match balanced (parens). See Friedl's 
        /// "Mastering Regular Expressions", 2nd Ed., pp. 328-331.
        /// </summary>
        private static string GetNestedParensPattern()
        {
            // in other words (this) and (this(also)) and (this(also(too)))
            // up to _nestDepth
            return RepeatString(@"
                   (?>              # Atomic matching
                      [^()\n\t]+? # Anything other than parens or whitespace
                    |
                      \(
                          ", _nestDepth) + RepeatString(
                   @" \)
                   )*?"
                   , _nestDepth);
        }

        /// <summary>
        /// this is to emulate what's evailable in PHP
        /// </summary>
        private static string RepeatString(string text, int count)
        {
            var sb = new StringBuilder(text.Length * count);
            for (int i = 0; i < count; i++)
                sb.Append(text);
            return sb.ToString();
        }

        #endregion


        #region helper - parse

        private TResult Create<TResult, TContent>(IEnumerable<TContent> content)
            where TResult : Panel, new()
            where TContent : Control
        {
            var result = new TResult();
            foreach (var c in content)
            {
                result.Children.Add(c);
            }

            return result;
        }


        //private IEnumerable<T> Evaluates<T>(
        //        string text, ParseStatus status,
        //        BlockParser<T>[] primary,
        //        BlockParser<T>[] secondly,
        //        Func<string, ParseStatus, IEnumerable<T>> rest
        //    )
        //{
        //    var index = 0;
        //    var length = text.Length;
        //    var rtn = new List<T>();
        //
        //    while (true)
        //    {
        //        int bestIndex = Int32.MaxValue;
        //        Match? bestMatch = null;
        //        BlockParser<T>? bestParser = null;
        //
        //        foreach (var parser in primary)
        //        {
        //            var match = parser.Pattern.Match(text, index, length);
        //            if (match.Success && match.Index < bestIndex)
        //            {
        //                bestIndex = match.Index;
        //                bestMatch = match;
        //                bestParser = parser;
        //            }
        //        }
        //
        //        if (bestParser is null || bestMatch is null) break;
        //
        //        var result = bestParser.Convert(text, bestMatch, status, this, out bestIndex, out int newIndex);
        //
        //        if (bestIndex > index)
        //        {
        //            EvaluateRest(rtn, text, index, bestIndex - index, status, secondly, 0, rest);
        //        }
        //
        //        rtn.AddRange(result);
        //
        //        length -= newIndex - index;
        //        index = newIndex;
        //    }
        //
        //    if (index < text.Length)
        //    {
        //        EvaluateRest(rtn, text, index, text.Length - index, status, secondly, 0, rest);
        //    }
        //
        //    return rtn;
        //
        //}
        //
        //private void EvaluateRest<T>(
        //    List<T> resultIn,
        //    string text, int index, int length,
        //    ParseStatus status,
        //    BlockParser<T>[] parsers, int parserStart,
        //    Func<string, ParseStatus, IEnumerable<T>> rest)
        //{
        //    for (; parserStart < parsers.Length; ++parserStart)
        //    {
        //        var parser = parsers[parserStart];
        //
        //        for (; ; )
        //        {
        //            var match = parser.Pattern.Match(text, index, length);
        //            if (!match.Success) break;
        //
        //            var result = parser.Convert(text, match, status, this, out var matchStartIndex, out int newIndex);
        //
        //            if (matchStartIndex > index)
        //            {
        //                EvaluateRest(resultIn, text, index, match.Index - index, status, parsers, parserStart + 1, rest);
        //            }
        //
        //            resultIn.AddRange(result);
        //
        //            length -= newIndex - index;
        //            index = newIndex;
        //        }
        //
        //        if (length == 0) break;
        //    }
        //
        //    if (length != 0)
        //    {
        //        var suffix = text.Substring(index, length);
        //        resultIn.AddRange(rest(suffix, status));
        //    }
        //}

        #endregion
    }

    internal struct Candidate<T> : IComparable<Candidate<T>>
    {
        public Match Match { get; }
        public T Parser { get; }

        public Candidate(Match result, T parser)
        {
            Match = result;
            Parser = parser;
        }

        public int CompareTo(Candidate<T> other)
            => Match.Index.CompareTo(other.Match.Index);
    }

    internal class UnclosableStream : Stream
    {
        private Stream _stream;

        public UnclosableStream(Stream stream)
        {
            _stream = stream;
        }

        public override bool CanRead => _stream.CanRead;
        public override bool CanSeek => _stream.CanSeek;
        public override bool CanWrite => _stream.CanWrite;
        public override long Length => _stream.Length;

        public override long Position
        {
            get => _stream.Position;
            set => _stream.Position = value;
        }

        public override void Flush() { }
        public override void Close() { }

        public override int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);

        public override void SetLength(long value) => _stream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}