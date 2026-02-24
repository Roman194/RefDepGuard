using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RefDepGuard.Data.FrameworkVersion;

namespace RefDepGuard.Comparators
{
    public class FrameworkVersionComparabilityErrorContainsComparer : IEqualityComparer<FrameworkVersionComparabilityError>
    {
        public bool Equals(FrameworkVersionComparabilityError x, FrameworkVersionComparabilityError y)
        {
            return x.ErrorLevel == y.ErrorLevel && 
                x.TargetFrameworkVersion == y.TargetFrameworkVersion && 
                x.MaxFrameworkVersion == y.MaxFrameworkVersion && 
                x.ErrorRelevantProjectName == y.ErrorRelevantProjectName;
        }

        public int GetHashCode(FrameworkVersionComparabilityError obj)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + obj.ErrorLevel.GetHashCode();
                hash = hash * 23 + obj.TargetFrameworkVersion.GetHashCode();
                hash = hash * 23 + obj.MaxFrameworkVersion.GetHashCode();
                hash = hash * 23 + obj.ErrorRelevantProjectName.GetHashCode();
                return hash;
            }
        }
    }
}
