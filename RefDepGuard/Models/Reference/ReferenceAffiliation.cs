using System.Collections.Generic;

namespace RefDepGuard
{
    /// <summary>
    /// Shows a refernce rules affiliation by rules level
    /// </summary>
    public class ReferenceAffiliation
    {
        public ProblemLevel RulesLevel;
        public List<string> RequiredReferences;
        public List<string> UnacceptableReferences;

        /// <param name="rulesLevel">relevant rules level</param>
        /// <param name="requiredReferences">list of string values of the required refs</param>
        /// <param name="unacceptableReferences">list of string values of the unacceptable refs</param>
        public ReferenceAffiliation(ProblemLevel rulesLevel, List<string> requiredReferences, List<string> unacceptableReferences)
        {
            RulesLevel = rulesLevel;
            RequiredReferences = requiredReferences;
            UnacceptableReferences = unacceptableReferences;
        }
    }
}