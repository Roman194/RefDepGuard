using System.Collections.Generic;

namespace RefDepGuard.Comparators
{
    /// <summary>
    /// This class implements the IComparer interface to provide a custom sorting logic for ReferenceMatchError objects.
    /// <see cref="ReferenceMatchError"/> for details on the properties of the objects being compared.
    /// </summary>
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
