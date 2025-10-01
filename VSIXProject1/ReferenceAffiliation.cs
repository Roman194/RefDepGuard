using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1
{
    public class ReferenceAffiliation
    {
        public string Reference;
        public bool IsReferenceGlobal;

        public ReferenceAffiliation(string reference, bool isReferenceGlobal)
        {
            Reference = reference;
            IsReferenceGlobal = isReferenceGlobal;
            
        }

    }
}
