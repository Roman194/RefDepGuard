using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1.Data.Reference
{
    public class ReferenceMatchWarning
    {
        public ReferenceLevel HighReferenceLevel;
        public ReferenceLevel LowReferenceLevel;
        public string ReferenceName;
        public string ProjectName;
        public bool IsHighLevelReq;

        public ReferenceMatchWarning(ReferenceLevel highReferenceLevel, ReferenceLevel lowReferenceLevel, string referenceName, string projectName, bool isHighLevelReq)
        {
            HighReferenceLevel = highReferenceLevel;
            LowReferenceLevel = lowReferenceLevel;
            ReferenceName = referenceName;
            ProjectName = projectName;
            IsHighLevelReq = isHighLevelReq;
        }
    }
}
