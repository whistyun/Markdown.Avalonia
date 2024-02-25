using Avalonia;
using Avalonia.Layout;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ColorDocument.Avalonia
{
    internal static class SelectionUtil
    {
        public static List<DocumentElement> SelectVertical<T>(Layoutable anchor, EnumerableEx<T> elements, Point from, Point to)
            where T : DocumentElement
        {
            var c = elements.GetRectInDoc(anchor);

            int fp = ComputeIdxVertical(c, from);
            int tp = ComputeIdxVertical(c, to);

            return Select(c, from, to, fp, tp);
        }

        public static List<DocumentElement> SelectGrid<T>(Layoutable anchor, EnumerableEx<T> elements, Point from, Point to)
            where T : DocumentElement
        {
            var c = elements.GetRectInDoc(anchor);

            int fp = ComputeIdxGrid(c, from);
            int tp = ComputeIdxGrid(c, to);

            return Select(c, from, to, fp, tp);
        }

        static int ComputeIdxVertical(EnumerableEx<DocumentElementWithBound> elements, Point pnt)
        {
            if (pnt.X <= 0 && pnt.Y <= 0)
                return 0;

            if (pnt.X == Double.PositiveInfinity && pnt.Y == Double.PositiveInfinity)
                return elements.Count - 1;

            foreach ((var c, int i) in elements.Select((value, index) => (value, index)))
            {
                var bounds = c.Rect;

                if (pnt.Y < bounds.Bottom)
                    return i;
            }

            return elements.Count - 1;
        }

        static int ComputeIdxGrid(EnumerableEx<DocumentElementWithBound> elements, Point pnt)
        {
            if (pnt.X <= 0 && pnt.Y <= 0)
                return 0;

            if (pnt.X == Double.PositiveInfinity && pnt.Y == Double.PositiveInfinity)
                return elements.Count - 1;


            double prevLeft = double.NegativeInfinity;
            int verticalLastHit = -1;

            foreach ((var c, int i) in elements.Select((value, index) => (value, index)))
            {
                var bounds = c.Rect;

                if (pnt.Y < bounds.Bottom)
                {
                    if (pnt.X < bounds.Right)
                        return i;

                    if (bounds.Left < prevLeft)
                        return verticalLastHit;

                    prevLeft = bounds.Left;
                    verticalLastHit = i;
                }
            }

            return elements.Count - 1;
        }

        private static List<DocumentElement> Select(EnumerableEx<DocumentElementWithBound> elements, Point from, Point to, int fp, int tp)
        {
            var list = new List<DocumentElement>();

            if (fp < tp)
            {
                var workF = from;
                var workT = new Point(Double.PositiveInfinity, Double.PositiveInfinity);

                for (var i = fp; i <= tp; ++i)
                {
                    if (i == tp)
                    {
                        workT = to;
                    }

                    var element = elements[i].Element;
                    var rect = elements[i].Rect;

                    element.Select(
                        new Point(workF.X - rect.X, workF.Y - rect.Y),
                        new Point(workT.X - rect.X, workT.Y - rect.Y));

                    list.Add(element);
                    workF = new Point(0, 0);
                }
            }
            else if (tp < fp)
            {
                var workF = from;
                var workT = new Point(0, 0);

                for (var i = fp; i >= tp; --i)
                {
                    if (i == tp)
                    {
                        workT = to;
                    }

                    var element = elements[i].Element;
                    var rect = elements[i].Rect;

                    element.Select(
                        new Point(workF.X - rect.X, workF.Y - rect.Y),
                        new Point(workT.X - rect.X, workT.Y - rect.Y));

                    list.Add(element);
                    workF = new Point(Double.PositiveInfinity, Double.PositiveInfinity);
                }
            }
            else
            {
                var element = elements[tp].Element;
                var rect = elements[tp].Rect;

                element.Select(
                       new Point(from.X - rect.X, from.Y - rect.Y),
                       new Point(to.X - rect.X, to.Y - rect.Y));

                list.Add(element);
            }

            return list;
        }
    }
}
