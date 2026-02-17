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
        public ProblemLevel WarningLevel;
        public string ProjName;

        public ProjectNotFoundWarning(string referenceName, ProblemLevel warningLevel, string projName)
        {
            ReferenceName = referenceName;
            WarningLevel = warningLevel;
            ProjName = projName;
        }
    }
}
