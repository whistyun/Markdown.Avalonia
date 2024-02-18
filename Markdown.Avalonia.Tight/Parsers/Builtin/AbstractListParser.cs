using ColorDocument.Avalonia.DocumentElements;
using Markdown.Avalonia.Utils;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Markdown.Avalonia.Parsers.Builtin
{
    internal abstract class AbstractListParser : BlockParser2
    {
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

        private static readonly Regex _startQuoteOrHeader = new(@"\A(\#{1,6}[ ]|>|```)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        protected AbstractListParser(Regex pattern) : base(pattern, "ListEvaluator")
        {
        }

        protected static Regex CreateWholeListPattern(
                string firstListMarkerPattern,
                string subseqListMarkerPattern)
        {
            var format = string.Format(_wholeListFormat, firstListMarkerPattern, subseqListMarkerPattern, _listDepth - 1);
            return new Regex(format, RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
        }


        protected ListBlockElement ListEvalutor(
            Match match,
            Regex sublistMarker,
            IMarkdownEngine2 engine,
            out int parseTextBegin, out int parseTextEnd)
        {
            parseTextBegin = match.Index;

            // Check text marker style.
            (TextMarkerStyle textMarker, string markerPattern, int indentAppending)
                = GetTextMarkerStyle(match.Groups["mkr"].Value);

            Regex markerRegex = new(@"\A" + markerPattern, RegexOptions.Compiled);

            // count indent from first marker with indent
            int countIndent = TextUtil.CountIndent(match.Groups["mkr_i"].Value);

            // whole list
            string[] listLines = match.Groups["whltxt"].Value.Split('\n');
            parseTextEnd = match.Groups["whltxt"].Index;

            // collect detendentable line
            var listBulder = new StringBuilder();
            var outerListBuildre = new StringBuilder();
            for (var i = 0; i < listLines.Length; ++i)
            {
                var line = listLines[i];

                if (string.IsNullOrEmpty(line))
                {
                    listBulder.Append("").Append("\n");
                }
                else if (TextUtil.TryDetendLine(line, countIndent, out var stripedLine))
                {
                    // is it horizontal line?
                    if (_startNoIndentRule.IsMatch(stripedLine))
                    {
                        break;
                    }
                    // is it header or blockquote?
                    else if (_startQuoteOrHeader.IsMatch(stripedLine))
                    {
                        break;
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
                        else break;
                    }
                    else
                    {
                        var detentedline = TextUtil.DetentLineBestEffort(stripedLine, indentAppending);
                        listBulder.Append(detentedline).Append("\n");
                    }
                }
                else break;

                parseTextEnd += i == listLines.Length - 1 ? line.Length : line.Length + 1;
            }

            string list = listBulder.ToString();

            IEnumerable<ListItemElement> listItems = ProcessListItems(list, markerPattern, engine);

            return new ListBlockElement(textMarker.Change(), listItems);

        }
        /// <summary>
        /// Process the contents of a single ordered or unordered list, splitting it
        /// into individual list items.
        /// </summary>
        private IEnumerable<ListItemElement> ProcessListItems(string list, string marker, IMarkdownEngine2 engine)
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
                string item = m.Groups[4].Value;

                var status = new ParseStatus(false);

                // we could correct any bad indentation here..
                // recursion for sub-lists

                yield return new ListItemElement(engine.ParseGamutElement(item, status));
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
    }
}
