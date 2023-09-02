using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace Markdown.Avalonia.Html.Core.Utils
{
    internal class Length
    {
        public double Value { get; }
        public Unit Unit { get; }

        public Length(double value, Unit unit)
        {
            Value = value;
            Unit = unit;
        }

        public double ToPoint()
        {
            return Unit switch
            {
                Unit.Percentage => throw new InvalidOperationException("Percentage canot convert point"),
                Unit.em => Value * 11,
                Unit.ex => Value * 11 / 2,
                Unit.QuarterMillimeters => Value * 3.77952755905512 / 4,
                Unit.Millimeters => Value * 3.77952755905512,
                Unit.Centimeters => Value * 37.7952755905512,
                Unit.Inches => Value * 96.0,
                Unit.Points => Value * 1.33333333333333,
                Unit.Picas => Value * 16,
                Unit.Pixels => Value,

                _ => throw new NotSupportedException("")
            };
        }

        public static bool TryParse(string? text,
#if NET6_0_OR_GREATER
            [MaybeNullWhen(false)]
            out Length rslt)
#else
            out Length rslt)
#endif
        {
            if (String.IsNullOrEmpty(text))
                goto failParse;

            var mch = Regex.Match(text, @"^([0-9\.\+\-eE]+)(%|em|ex|mm|Q|cm|in|pt|pc|px|)$");

            if (!mch.Success)
                goto failParse;

            var numTxt = mch.Groups[1].Value.Trim();
            var unitTxt = mch.Groups[2].Value;

            if (!double.TryParse(numTxt, out var numVal))
                goto failParse;

            var unitEnm = unitTxt switch
            {
                "%" => Unit.Percentage,
                "em" => Unit.em,
                "ex" => Unit.ex,
                "mm" => Unit.Millimeters,
                "Q" => Unit.QuarterMillimeters,
                "cm" => Unit.Centimeters,
                "in" => Unit.Inches,
                "pt" => Unit.Points,
                "pc" => Unit.Picas,
                "px" => Unit.Pixels,
                "" => Unit.Pixels,
                _ => Unit.Pixels,
            };

            rslt = new Length(numVal, unitEnm);
            return true;

        failParse:
            rslt = null;
            return false;
        }
    }

    internal enum Unit
    {
        Percentage,
        em,
        ex,
        QuarterMillimeters,
        Millimeters,
        Centimeters,
        Inches,
        // pt: 1/72 in
        Points,
        // pc: 1/6 in
        Picas,
        // px; 1/96 in
        Pixels
    }
}
