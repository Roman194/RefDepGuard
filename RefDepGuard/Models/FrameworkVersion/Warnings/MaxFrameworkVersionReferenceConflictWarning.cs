using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefDepGuard.Data.FrameworkVersion
{
    public class MaxFrameworkVersionReferenceConflictWarning
    {
        public string ProjName;
        public string ProjFrameworkVersion;
        public string RefName;
        public string RefFrameworkVersion;
        public bool IsOneProjectsTypeConflict;

        public MaxFrameworkVersionReferenceConflictWarning(string projName, string projFrameworkVersion, string refName, string refFrameworkVersion, bool isOneProjectsTypeConflict)
        {
            ProjName = projName;
            ProjFrameworkVersion = projFrameworkVersion;
            RefName = refName;
            RefFrameworkVersion = refFrameworkVersion;
            IsOneProjectsTypeConflict = isOneProjectsTypeConflict;
        }
    }
}
