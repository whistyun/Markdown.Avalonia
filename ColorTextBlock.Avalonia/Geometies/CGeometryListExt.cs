using System.Collections.Generic;

namespace ColorTextBlock.Avalonia.Geometries
{
    static class CGeometryListExt
    {
        public static int IndexOf(this List<CGeometry> list, ITextPointerHandleable handleable)
        {
            for (var i = 0; i < list.Count; ++i)
            {
                if (ReferenceEquals(list[i], handleable))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
