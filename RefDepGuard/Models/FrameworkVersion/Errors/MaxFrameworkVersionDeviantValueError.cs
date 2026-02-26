
namespace RefDepGuard.Data.FrameworkVersion
{
    public class MaxFrameworkVersionDeviantValueError
    {
        public ProblemLevel ErrorLevel;
        public string ErrorRelevantProjectName;
        public bool IsProjectTypeCopyError;
        
        public MaxFrameworkVersionDeviantValueError(ProblemLevel errorLevel, string errorRelevantProjectName, bool isProjectTypeCopyError)
        {
            ErrorLevel = errorLevel;
            ErrorRelevantProjectName = errorRelevantProjectName;
            IsProjectTypeCopyError = isProjectTypeCopyError;
        }
    }
}
