using System.Collections.Generic;

namespace RefDepGuard
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
