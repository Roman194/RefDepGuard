using System.Collections.Generic;

namespace RefDepGuard.Comparators
{
    public class ReferenceMatchErrorSortComparer : IComparer<ReferenceMatchError>
    {
        public int Compare(ReferenceMatchError x, ReferenceMatchError y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            // Enum values order defined as Global = 0, Solution = 1, Project = 2 by default
            return x.ReferenceLevelValue.CompareTo(y.ReferenceLevelValue);
        }
    }
}
