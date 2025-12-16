using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject1.Models;

namespace VSIXProject1.Comparators.ContainsComparators
{
    public class ProjectMatchWarningContainsComparer : IEqualityComparer<ProjectMatchWarning>
    {
        public bool Equals(ProjectMatchWarning x, ProjectMatchWarning y)
        {
            return x.ProjName == y.ProjName; // && x.IsNoProjectInConfigFile == y.IsNoProjectInConfigFile; По хорошему не должно быть случаев, когда один проект отсутствует и там и там
        }

        public int GetHashCode(ProjectMatchWarning obj)
        {
            int hash = 17;
            hash = hash * 23 + obj.ProjName.GetHashCode();
            return hash;
        }
    }
}
