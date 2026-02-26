
namespace RefDepGuard
{
    public class ReferenceMatchError
    {
        public ProblemLevel ReferenceLevelValue;
        public string ReferenceName;
        public string ProjectName;
        public bool IsProjNameMatchError;

        public ReferenceMatchError(ProblemLevel referenceLevel, string referenceName, string projectName, bool isProjNameMatchError) 
        {
            ReferenceLevelValue = referenceLevel;
            ReferenceName = referenceName;
            ProjectName = projectName;
            IsProjNameMatchError = isProjNameMatchError;
        }
    }
}
