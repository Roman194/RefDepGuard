using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject1.Data.FrameworkVersion;

namespace VSIXProject1.Comparators
{
    public class FrameworkVersionComparabilityErrorContainsComparer : IEqualityComparer<FrameworkVersionComparabilityError>
    {
        public bool Equals(FrameworkVersionComparabilityError x, FrameworkVersionComparabilityError y)
        {
            return x.ErrorLevel == y.ErrorLevel && x.TargetFrameworkVersion == y.TargetFrameworkVersion && x.MaxFrameworkVersion == y.MaxFrameworkVersion;
        }

        public int GetHashCode(FrameworkVersionComparabilityError obj)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + obj.ErrorLevel.GetHashCode();
                hash = hash * 23 + obj.TargetFrameworkVersion.GetHashCode();
                hash = hash * 23 + obj.MaxFrameworkVersion.GetHashCode();
                return hash;
            }
        }
    }
}
