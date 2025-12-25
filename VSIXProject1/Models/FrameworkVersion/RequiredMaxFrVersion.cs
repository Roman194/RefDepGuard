using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1.Data.FrameworkVersion
{
    public class RequiredMaxFrVersion
    {
        public string VersionText;
        public ErrorLevel ErrorLevel;
        public string ProjectTypeRule;
        public bool IsConflictWarningRelevantForThisProject;

        public RequiredMaxFrVersion(string versionText, ErrorLevel errorLevel, string projectTypeRule, bool isConflictWarningRelevantForThisProject)
        {
            VersionText = versionText;
            ErrorLevel = errorLevel;
            ProjectTypeRule = projectTypeRule;
            IsConflictWarningRelevantForThisProject = isConflictWarningRelevantForThisProject;
        }

    }
}
