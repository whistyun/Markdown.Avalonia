using ColorDocument.Avalonia;
using ColorDocument.Avalonia.DocumentElements;
using Markdown.Avalonia.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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


        private static readonly string _firstListLine = @"
              ^
              (?![ ]{{0,3}}(?<hrm>[-=*_])([ ]{{0,2}}\k<hrm>){{2,}}[ ]*\n) # ignore horizontal syntax
              (?<indent>[ ]{{0,{2}}})                                     # indent
              (?<marker>{0})                                              # first list item marker
              [ ]+
              (?:[^ \n]|\n)
            ";

        private static readonly string _ruleLine = @"
            \G
            [ ]{{0,{0}}}
            (?<hrm>[-=*_])([ ]{{0,2}}\k<hrm>){{2,}}[ ]*\n";

        private static readonly string _listLine = @"
            \G
            (?<indent>[ ]{{0,{0}}})
            {1}                                                            # list marker
            [ ]+
            (?<content>[^\n]*)                                         # content
            ";

        private static readonly Regex _startQuoteOrHeader = new(@"
            \G(\#{1,6}[ ]|>|```)",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        protected AbstractListParser(Regex pattern) : base(pattern, "ListEvaluator")
        {
        }

        protected static Regex CreateWholeListPattern(
                string firstListMarkerPattern,
                string subseqListMarkerPattern)
        {
            var format = string.Format(_firstListLine, firstListMarkerPattern, subseqListMarkerPattern, _listDepth - 1);
            return new Regex(format, RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
        }

        protected ListBlockElement ListEvalutor(
            string text,
            Match match,
            string alllistMarkersPattern,
            IMarkdownEngine2 engine,
            out int parseTextBegin, out int parseTextEnd
            )
        {
            parseTextBegin = match.Index;

            // Check text marker style.
            (TextMarkerStyle textMarker, string markerPattern, int indentAppending)
                = GetTextMarkerStyle(match.Groups["marker"].Value);

            // count indent from first marker with indent
            int countIndent = match.Groups["indent"].Value.Length;

            var ruleLinePtn = new Regex(
                String.Format(_ruleLine, countIndent + indentAppending - 1),
                RegexOptions.IgnorePatternWhitespace);

            var listLinePtn = new Regex(
                String.Format(_listLine, countIndent + indentAppending - 1, markerPattern),
                RegexOptions.IgnorePatternWhitespace);

            var allListLinePtn = new Regex(
                String.Format(_listLine, countIndent + indentAppending - 1, alllistMarkersPattern),
                RegexOptions.IgnorePatternWhitespace);

            int caret = parseTextBegin;

            var listItems = new List<ListItemElement>();

            var hasEmptyLine = false;
            var listBuilder = new StringBuilder();
            while (caret < text.Length)
            {
                // is it blank line?
                if (text[caret] == '\n')
                {
                    listBuilder.Append('\n');
                    hasEmptyLine = true;
                    ++caret;
                }
                // is it horizontal line?
                else if (ruleLinePtn.IsMatch(text, caret))
                {
                    break;
                }
                // is it header or blockquote?
                else if (_startQuoteOrHeader.IsMatch(text, caret))
                {
                    break;
                }
                else
                {
                    // Dose it have list marker?
                    var listLineMch = listLinePtn.Match(text, caret);
                    if (listLineMch.Success)
                    {
                        // next list item?
                        if (listBuilder.Length > 0)
                        {
                            TrimEnd(listBuilder);
                            var elements = engine.ParseGamutElement(listBuilder.ToString(), new ParseStatus(false));
                            listItems.Add(new ListItemElement(elements));
                            listBuilder.Length = 0;
                        }

                        listBuilder.Append(listLineMch.Groups["content"].Value);
                        caret += listLineMch.Length;

                        if (caret < text.Length && text[caret] == '\n')
                        {
                            ++caret;
                            listBuilder.Append('\n');
                        }

                        hasEmptyLine = false;
                    }
                    // Dose it have other list marker?
                    else if (allListLinePtn.IsMatch(text, caret))
                    {
                        break;
                    }
                    else
                    {
                        var st = caret;
                        if (!MoveIndent(text, countIndent + indentAppending, ref st))
                        {
                            if (hasEmptyLine) break;
                        }

                        caret = st;
                        if (caret < text.Length)
                        {
                            MoveLineEnd(text, ref caret);
                            listBuilder.Append(text, st, caret - st);

                            if (caret < text.Length && text[caret] == '\n')
                            {
                                ++caret;
                                listBuilder.Append('\n');
                            }
                        }
                    }
                }
            }

            if (listBuilder.Length > 0)
            {
                TrimEnd(listBuilder);
                var elements = engine.ParseGamutElement(listBuilder.ToString(), new ParseStatus(false));
                listItems.Add(new ListItemElement(elements)); ;
            }

            parseTextEnd = caret;
            return new ListBlockElement(textMarker.Change(), listItems);
        }

        private static void TrimEnd(StringBuilder text)
        {
            if (text.Length < 2)
                return;

            if (text[text.Length - 1] != '\n')
                return;

            int len = text.Length;
            while (len >= 2)
            {
                if (text[len - 2] == '\n')
                    len--;
                else
                    break;
            }

            text.Length = len;
        }

        private static bool MoveIndent(string text, int count, ref int caret)
        {
            for (var i = 0; i < count; ++i)
            {
                if (caret == text.Length)
                    return false;

                if (text[caret] != ' ')
                    return false;

                ++caret;
            }

            return true;
        }

        private static bool MoveLineEnd(string text, ref int caret)
        {
            for (; caret < text.Length; ++caret)
            {
                if (text[caret] == '\n')
                {
                    return true;
                }
            }

            return false;
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
