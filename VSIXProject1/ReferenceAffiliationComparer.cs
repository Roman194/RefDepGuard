using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1
{
    public class ReferenceAffiliationComparer: IEqualityComparer<ReferenceAffiliation>
    {
        public bool Equals(ReferenceAffiliation x, ReferenceAffiliation y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x == null || y == null) return false;

            return x.Reference == y.Reference;
        }

        public int GetHashCode(ReferenceAffiliation obj)
        {
            if (obj == null || obj.Reference == null) return 0;
            return obj.Reference.GetHashCode();
        }
    }
}
