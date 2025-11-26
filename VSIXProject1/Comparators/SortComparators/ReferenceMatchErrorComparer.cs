using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1
{
    public class ReferenceMatchErrorComparer : IEqualityComparer<ReferenceMatchError>
    {
        public bool Equals(ReferenceMatchError x, ReferenceMatchError y)
        {
            return x.ReferenceName == y.ReferenceName;
        }

        public int GetHashCode(ReferenceMatchError obj)
        {
            return obj.ReferenceName.GetHashCode();
        }
    }
}
