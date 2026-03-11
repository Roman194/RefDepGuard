using System.Collections.Generic;

namespace RefDepGuard
{
    /// <summary>
    /// This class implements the IComparer interface to provide a custom sorting logic for ReferenceError objects.
    /// <see cref="ReferenceError"/> for details on the properties of the objects being compared.
    /// </summary>
    public class ReferenceErrorSortComparer : IComparer<ReferenceError>
    {
        public int Compare(ReferenceError x, ReferenceError y)
        {

            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            // Enum values order defined as Global = 0, Solution = 1, Project = 2 by default
            return x.CurrentRuleLevel.CompareTo(y.CurrentRuleLevel);
        }
    }
}
