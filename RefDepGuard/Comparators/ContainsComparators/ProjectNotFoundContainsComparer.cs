using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RefDepGuard.Models.Reference;

namespace RefDepGuard.Comparators.ContainsComparators
{
    public class ProjectNotFoundContainsComparer : IEqualityComparer<ProjectNotFoundWarning>
    {
        bool IEqualityComparer<ProjectNotFoundWarning>.Equals(ProjectNotFoundWarning x, ProjectNotFoundWarning y)
        {
            return x.ReferenceName == y.ReferenceName && x.WarningLevel == y.WarningLevel && x.ProjName == y.ProjName;
        }

        int IEqualityComparer<ProjectNotFoundWarning>.GetHashCode(ProjectNotFoundWarning obj)
        {
            int hash = 17;
            hash = hash * 23 + obj.ReferenceName.GetHashCode();
            hash = hash * 23 + obj.WarningLevel.GetHashCode();
            hash = hash * 23 + obj.ProjName.GetHashCode();

            return hash;
        }
    }
}
