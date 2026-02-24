using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefDepGuard
{
    public class ReferenceErrorSortComparer : IComparer<ReferenceError>
    {
        public int Compare(ReferenceError x, ReferenceError y)
        {

            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            // Enum values order defined as Global = 0, Solution = 1, Project = 2 by default
            return x.CurrentReferenceLevel.CompareTo(y.CurrentReferenceLevel);

        }
    }

}
