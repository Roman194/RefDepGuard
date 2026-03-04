
using System.Collections.Generic;

namespace RefDepGuard.Data.FrameworkVersion
{
    public class RequiredMaxFrVersion
    {
        public string VersionText;
        public List<int> VersionNums;
        public ProblemLevel ReqLevel;
        public string ProjectTypeRule;
        public bool IsConflictWarningRelevantForThisProject;

        public RequiredMaxFrVersion(string versionText, List<int> versionNums, ProblemLevel reqLevel, string projectTypeRule, bool isConflictWarningRelevantForThisProject)
        {
            VersionText = versionText;
            VersionNums = versionNums;
            ReqLevel = reqLevel;
            ProjectTypeRule = projectTypeRule;
            IsConflictWarningRelevantForThisProject = isConflictWarningRelevantForThisProject;
        }
    }
}
