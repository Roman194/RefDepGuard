using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1
{
    public class ReferenceAffiliation
    {
        public ProblemLevel ReferenceTypeValue;
        public List<string> RequiredReferences;
        public List<string> UnacceptableReferences;

        public ReferenceAffiliation(ProblemLevel referenceType, List<string> requiredReferences, List<string> unacceptableReferences)
        {
            ReferenceTypeValue = referenceType;
            RequiredReferences = requiredReferences;
            UnacceptableReferences = unacceptableReferences;
            
        }

    }
}
