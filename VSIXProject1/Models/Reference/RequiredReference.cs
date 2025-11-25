using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1.Data.Reference
{
    public class RequiredReference
    {
        public string ReferenceName;
        public string RelevantProject;

        public RequiredReference(string referenceName, string relevantProject)
        {
            ReferenceName = referenceName;
            RelevantProject = relevantProject;
        }
    }
}
