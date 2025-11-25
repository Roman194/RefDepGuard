using System.Collections.Generic;
using VSIXProject1.Data.FrameworkVersion;

namespace VSIXProject1
{
    internal class MaxFrameworkVersionDeviantValueSortComparer : IComparer<MaxFrameworkVersionDeviantValueError>
    {
        public int Compare(MaxFrameworkVersionDeviantValueError x, MaxFrameworkVersionDeviantValueError y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            // Enum values order defined as Global = 0, Solution = 1, Project = 2 by default
            return x.ErrorLevel.CompareTo(y.ErrorLevel);
        }
    }
}