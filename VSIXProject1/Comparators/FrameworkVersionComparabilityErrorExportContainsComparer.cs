using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject1.Data.FrameworkVersion;

namespace VSIXProject1.Comparators
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
