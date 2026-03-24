using System;
using System.Collections.Generic;
using System.Text;

namespace RefDepGuard.CheckRules.Models.Reference.Errors
{
    /// <summary>
    /// Shows an error when a real solution references is not match to required and unacceptable reference rules inside config files
    /// </summary>
    public class ReferenceError
    {
        public string ReferenceName;
        public string ErrorRelevantProjectName;
        public bool IsReferenceRequired;
        public ProblemLevel CurrentRuleLevel;

        /// <param name="referenceName">reference name string</param>
        /// <param name="errorRelevantProjectName">error relevant proj name string</param>
        /// <param name="isReferenceRequired">shows if the ref is required or unacceptable</param>
        /// <param name="ruleLevel">shows an error relevant rule level</param>
        public ReferenceError(string referenceName, string errorRelevantProjectName, bool isReferenceRequired = true, ProblemLevel ruleLevel = ProblemLevel.Global)
        {
            ReferenceName = referenceName;
            ErrorRelevantProjectName = errorRelevantProjectName;
            IsReferenceRequired = isReferenceRequired;
            CurrentRuleLevel = ruleLevel;
        }
    }
}