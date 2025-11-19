using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject1.Data.FrameworkVersion;

namespace VSIXProject1.Comparators
{
    public class MaxFrameworkVersionDeviantValueExportContainsComparer : IEqualityComparer<MaxFrameworkVersionDeviantValue>
    {
        public bool Equals(MaxFrameworkVersionDeviantValue x, MaxFrameworkVersionDeviantValue y)
        {
            return x.ErrorRelevantProjectName == y.ErrorRelevantProjectName || x.ErrorRelevantProjectName == "";
        }

        public int GetHashCode(MaxFrameworkVersionDeviantValue obj)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (obj.ErrorRelevantProjectName?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}
