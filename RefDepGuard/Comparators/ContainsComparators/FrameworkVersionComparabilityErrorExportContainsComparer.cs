using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RefDepGuard.Data.FrameworkVersion;

namespace RefDepGuard.Comparators
{
    public class FrameworkVersionComparabilityErrorExportContainsComparer : IEqualityComparer<FrameworkVersionComparabilityError>
    {
        public bool Equals(FrameworkVersionComparabilityError x, FrameworkVersionComparabilityError y)
        {
            return x.ErrorRelevantProjectName == y.ErrorRelevantProjectName;
        }

        public int GetHashCode(FrameworkVersionComparabilityError obj)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + obj.ErrorRelevantProjectName.GetHashCode();
                return hash;
            }
        }
    }
}
