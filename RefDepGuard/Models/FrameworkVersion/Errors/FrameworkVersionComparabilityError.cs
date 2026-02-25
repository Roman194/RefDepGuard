
namespace RefDepGuard.Data.FrameworkVersion
{
    public class FrameworkVersionComparabilityError
    {
        public ProblemLevel ErrorLevel;
        public string TargetFrameworkVersion;
        public string MaxFrameworkVersion;
        public string ErrorRelevantProjectName;
        
        public FrameworkVersionComparabilityError(ProblemLevel errorLevel,  string targetFrameworkVersion, string maxFrameworkVersion, string errorRelevantProjectName)
        {
            ErrorLevel = errorLevel;
            TargetFrameworkVersion = targetFrameworkVersion;
            MaxFrameworkVersion = maxFrameworkVersion;
            ErrorRelevantProjectName = errorRelevantProjectName;
        }
    }
}
