using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1
{
    public class ReferenceAffiliation
    {
        public ReferenceType ReferenceTypeValue;
        public List<ConfigFileReference> RequiredReferences;
        public List<ConfigFileReference> UnacceptableReferences;

        public ReferenceAffiliation(ReferenceType referenceType, List<ConfigFileReference> requiredReferences, List<ConfigFileReference> unacceptableReferences)
        {
            ReferenceTypeValue = referenceType;
            RequiredReferences = requiredReferences;
            UnacceptableReferences = unacceptableReferences;
            
        }

    }
}
