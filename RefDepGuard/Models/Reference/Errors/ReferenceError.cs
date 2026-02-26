
namespace RefDepGuard
{
    public class ReferenceError
    {
        public string ReferenceName;
        public string ErrorRelevantProjectName;
        public bool IsReferenceRequired;
        public ProblemLevel CurrentReferenceLevel;

        public ReferenceError(string referenceName, string errorRelevantProjectName, bool isReferenceRequired = true, ProblemLevel referenceLevel = ProblemLevel.Global)
        {
            ReferenceName = referenceName;
            ErrorRelevantProjectName = errorRelevantProjectName;
            IsReferenceRequired = isReferenceRequired;
            CurrentReferenceLevel = referenceLevel;
        }
    }
}
