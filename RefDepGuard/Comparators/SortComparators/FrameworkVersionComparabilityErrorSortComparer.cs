using System.Collections.Generic;
using RefDepGuard.Data.FrameworkVersion;

namespace RefDepGuard
{
    internal class FrameworkVersionComparabilityErrorSortComparer : IComparer<FrameworkVersionComparabilityError>
    {
        public int Compare(FrameworkVersionComparabilityError x, FrameworkVersionComparabilityError y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            // Enum values order defined as Global = 0, Solution = 1, Project = 2 by default
            return x.ErrorLevel.CompareTo(y.ErrorLevel);
        }
    }
}