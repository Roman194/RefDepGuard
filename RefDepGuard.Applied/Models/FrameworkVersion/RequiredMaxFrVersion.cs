using RefDepGuard.Applied.Models.Problem;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefDepGuard.Applied.Models.FrameworkVersion
{
    /// <summary>
    /// Shows an required max_fr_version parameters
    /// </summary>
    public class RequiredMaxFrVersion
    {
        public string VersionText;
        public List<int> VersionNums;
        public ProblemLevel ReqLevel;
        public string ProjectTypeRule;
        public bool IsConflictWarningRelevantForThisProject;

        /// <param name="versionText">max_fr_version string</param>
        /// <param name="versionNums">max_fr_version in list of ints format</param>
        /// <param name="reqLevel">relevant parameter level</param>
        /// <param name="projectTypeRule">project type rule string</param>
        /// <param name="isConflictWarningRelevantForThisProject">shows if conflict warn is relevant for this project</param>
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