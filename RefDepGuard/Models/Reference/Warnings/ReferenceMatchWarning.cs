
namespace RefDepGuard.Data.Reference
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
