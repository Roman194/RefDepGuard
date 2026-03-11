
namespace RefDepGuard.Data.FrameworkVersion
{
    /// <summary>
    /// Shows an error of the comparatibilty between TargetFramework(-s) of the project and its max_framework_version config file value
    /// </summary>
    public class FrameworkVersionComparatibilityError
    {
        public ProblemLevel ErrorLevel;
        public string TargetFrameworkVersion;
        public string MaxFrameworkVersion;
        public string ErrorRelevantProjectName;
        
        /// <param name="errorLevel">relevant error level</param>
        /// <param name="targetFrameworkVersion">target framework version string</param>
        /// <param name="maxFrameworkVersion">max frameowkr version string</param>
        /// <param name="errorRelevantProjectName">error relevant proj name string (if its a project error level))</param>
        public FrameworkVersionComparatibilityError(ProblemLevel errorLevel,  string targetFrameworkVersion, string maxFrameworkVersion, string errorRelevantProjectName)
        {
            ErrorLevel = errorLevel;
            TargetFrameworkVersion = targetFrameworkVersion;
            MaxFrameworkVersion = maxFrameworkVersion;
            ErrorRelevantProjectName = errorRelevantProjectName;
        }
    }
}