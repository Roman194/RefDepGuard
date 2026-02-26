
namespace RefDepGuard.Data.FrameworkVersion
{
    public class RequiredMaxFrVersion
    {
        public string VersionText;
        public ProblemLevel ReqLevel;
        public string ProjectTypeRule;
        public bool IsConflictWarningRelevantForThisProject;

        public RequiredMaxFrVersion(string versionText, ProblemLevel reqLevel, string projectTypeRule, bool isConflictWarningRelevantForThisProject)
        {
            VersionText = versionText;
            ReqLevel = reqLevel;
            ProjectTypeRule = projectTypeRule;
            IsConflictWarningRelevantForThisProject = isConflictWarningRelevantForThisProject;
        }
    }
}
