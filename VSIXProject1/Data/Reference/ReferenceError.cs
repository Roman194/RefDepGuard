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
        public ReferenceLevel CurrentReferenceLevel;

        public ReferenceError(string referenceName, string errorRelevantProjectName, bool isReferenceRequired = true, ReferenceLevel referenceLevel = ReferenceLevel.Global)
        {
            ReferenceName = referenceName;
            ErrorRelevantProjectName = errorRelevantProjectName;
            IsReferenceRequired = isReferenceRequired;
            CurrentReferenceLevel = referenceLevel;
        }
    }
}
