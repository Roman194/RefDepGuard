using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1.Models.Reference
{
    public class ProjectNotFoundWarning
    {
        public string ReferenceName;
        public ErrorLevel WarningLevel;
        public string ProjName;

        public ProjectNotFoundWarning(string referenceName, ErrorLevel warningLevel, string projName)
        {
            ReferenceName = referenceName;
            WarningLevel = warningLevel;
            ProjName = projName;
        }
    }
}
