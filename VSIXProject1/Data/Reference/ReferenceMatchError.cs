using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1
{
    public class ReferenceMatchError
    {
        public ReferenceType ReferenceTypeValue;
        public string ReferenceName;
        public string ProjectName;
        public bool IsProjNameMatchError;

        public ReferenceMatchError(ReferenceType referenceType, string referenceName, string projectName, bool isProjNameMatchError)
        {
            ReferenceTypeValue = referenceType;
            ReferenceName = referenceName;
            ProjectName = projectName;
            IsProjNameMatchError = isProjNameMatchError;
        }
    }
}
