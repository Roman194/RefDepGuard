using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1
{
    public class ReferenceErrorContainsComparer : IEqualityComparer<ReferenceError>
    {
        public bool Equals(ReferenceError x, ReferenceError y)
        {
            return x.ReferenceName == y.ReferenceName && x.ErrorRelevantProjectName == y.ErrorRelevantProjectName;
        }

        public int GetHashCode(ReferenceError obj)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (obj.ReferenceName?.GetHashCode() ?? 0);
                hash = hash * 23 + (obj.ErrorRelevantProjectName?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}
