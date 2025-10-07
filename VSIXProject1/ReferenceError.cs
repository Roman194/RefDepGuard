using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1
{
    public class ReferenceError
    {
        public string ReferenceName;
        public string ErrorRelevantProjectName;
        public bool IsReferenceRequired;
        public ReferenceType CurrentReferenceType;

        public ReferenceError(string referenceName, string errorRelevantProjectName, bool isReferenceRequired, ReferenceType referenceType)
        {
            ReferenceName = referenceName;
            ErrorRelevantProjectName = errorRelevantProjectName;
            IsReferenceRequired = isReferenceRequired;
            CurrentReferenceType = referenceType;
        }
    }
}
