using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1.Data.Reference
{
    public class ReferenceMatchWarning
    {
        public ProblemLevel HighReferenceLevel;
        public ProblemLevel LowReferenceLevel;
        public string ReferenceName;
        public string ProjectName;
        public bool IsReferenceStraight;
        public bool IsHighLevelReq;

        public ReferenceMatchWarning(ProblemLevel highReferenceLevel, ProblemLevel lowReferenceLevel, string referenceName, string projectName, bool isReferenceStaright, bool isHighLevelReq)
        {
            HighReferenceLevel = highReferenceLevel;
            LowReferenceLevel = lowReferenceLevel;
            ReferenceName = referenceName;
            ProjectName = projectName;
            IsReferenceStraight = isReferenceStaright;
            IsHighLevelReq = isHighLevelReq;
        }
    }
}
