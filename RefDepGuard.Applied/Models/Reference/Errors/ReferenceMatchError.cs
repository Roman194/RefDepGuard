using System;
using System.Collections.Generic;
using System.Text;
using RefDepGuard.Applied.Models.Problem;

namespace RefDepGuard.Applied.Models.Reference.Errors
{
    /// <summary>
    /// Shows a ref match error between config file one level required and unacceptable rules
    /// </summary>
    public class ReferenceMatchError
    {
        public ProblemLevel RuleLevel;
        public string ReferenceName;
        public string ProjectName;
        public bool IsProjNameMatchError;

        /// <param name="ruleLevel">relevant rule level</param>
        /// <param name="referenceName">reference name string</param>
        /// <param name="projectName">project name string</param>
        /// <param name="isProjNameMatchError">shows if its a project name match error ("self-locking")</param>
        public ReferenceMatchError(ProblemLevel ruleLevel, string referenceName, string projectName, bool isProjNameMatchError)
        {
            RuleLevel = ruleLevel;
            ReferenceName = referenceName;
            ProjectName = projectName;
            IsProjNameMatchError = isProjNameMatchError;
        }
    }
}