using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1
{
    public class ReferenceMatchError
    {
        public ProblemLevel ReferenceLevelValue;
        public string ReferenceName;
        public string ProjectName;
        public bool IsProjNameMatchError;

        public ReferenceMatchError(ProblemLevel referenceLevel, string referenceName, string projectName, bool isProjNameMatchError) 
        {
            ReferenceLevelValue = referenceLevel;
            ReferenceName = referenceName;
            ProjectName = projectName;
            IsProjNameMatchError = isProjNameMatchError;
        }
    }
}
