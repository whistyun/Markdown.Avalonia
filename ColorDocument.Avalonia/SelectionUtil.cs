using Avalonia;
using ColorDocument.Avalonia.DocumentElements;
using Markdown.Avalonia.Utils;
using System;
using System.Linq;

namespace ColorDocument.Avalonia
{
    internal static class SelectionUtil
    {
        public static SelectDirection SelectVertical<T>(EnumerableEx<T> elements, Point from, Point to)
            where T : DocumentElement
        {
            var c = elements.GetRectInDoc();

            int fp = ComputeIdxVertical(c, from);
            int tp = ComputeIdxVertical(c, to);

            return Select(elements, from, to, fp, tp);
        }

        public static SelectDirection SelectGrid<T>(EnumerableEx<T> elements, Point from, Point to)
            where T : DocumentElement
        {
            var c = elements.GetRectInDoc();

            int fp = ComputeIdxGrid(c, from);
            int tp = ComputeIdxGrid(c, to);

            return Select(elements, from, to, fp, tp);
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

        private static SelectDirection Select<T>(EnumerableEx<T> elements, Point from, Point to, int fp, int tp)
            where T : DocumentElement
        {
            if (fp < tp)
            {
                var workF = from;
                var workT = new Point(Double.PositiveInfinity, Double.PositiveInfinity);

                for (var i = fp; i < tp - 1; ++i)
                {
                    elements[i].Select(workF, workT);
                    workF = new Point(0, 0);
                }

                elements[tp - 1].Select(workF, to);

                return SelectDirection.Backward;
            }
            else if (tp < fp)
            {
                var workF = new Point(0, 0);
                var workT = to;

                for (var i = tp - 1; i >= fp - 1; --i)
                {
                    elements[i].Select(workF, workT);
                    workT = new Point(Double.PositiveInfinity, Double.PositiveInfinity);
                }

                elements[fp].Select(from, workT);

                return SelectDirection.Backward;
            }
            else
            {
                return elements[tp].Select(from, to);
            }
        }
    }
}
