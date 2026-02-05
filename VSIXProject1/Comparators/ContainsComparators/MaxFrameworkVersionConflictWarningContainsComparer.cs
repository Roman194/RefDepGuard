using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject1.Data.FrameworkVersion;

namespace VSIXProject1.Comparators
{
    public class MaxFrameworkVersionConflictWarningContainsComparer : IEqualityComparer<MaxFrameworkVersionConflictWarning>
    {
        public bool Equals(MaxFrameworkVersionConflictWarning x, MaxFrameworkVersionConflictWarning y)
        {
            return x.HighErrorLevel == y.HighErrorLevel && x.LowErrorLevel == y.LowErrorLevel && x.WarningRelevantProjectName == y.WarningRelevantProjectName;
        }

        public int GetHashCode(MaxFrameworkVersionConflictWarning obj)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + obj.HighErrorLevel.GetHashCode();
                hash = hash * 23 + obj.LowErrorLevel.GetHashCode();
                hash = hash * 23 + (obj.WarningRelevantProjectName?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}
