using RefDepGuard.Applied.Models.FrameworkVersion.Errors;
using RefDepGuard.Applied.Models.FrameworkVersion.Warnings;
using System.Collections.Generic;

namespace RefDepGuard.CheckRules.Models
{
    /// <summary>
    /// Shows a max_fr_ver/TargetFramework(-s) rule problems: comparatibility errors and untyped warnings
    /// </summary>
    public class MaxFrameworkRuleProblems
    {
        public List<FrameworkVersionComparabilityError> FrameworkVersionComparabilityErrorList;
        public List<MaxFrameworkIllegalTemplateUsageWarning> MaxFrameworkVersionIllegalTemplateUsageWarningList;
        public List<string> UntypedWarningsList;

        /// <param name="frameworkVersionComparabilityErrorList">list of FrameworkVersionComparatibilityError values</param>
        /// <param name="untypedWarningsList">list of string values</param>
        public MaxFrameworkRuleProblems(
            List<FrameworkVersionComparabilityError> frameworkVersionComparabilityErrorList, 
            List<MaxFrameworkIllegalTemplateUsageWarning> maxFrameworkVersionIllegalTemplateUsageWarningList, 
            List<string> untypedWarningsList
            )
        {
            FrameworkVersionComparabilityErrorList = frameworkVersionComparabilityErrorList;
            MaxFrameworkVersionIllegalTemplateUsageWarningList = maxFrameworkVersionIllegalTemplateUsageWarningList;
            UntypedWarningsList = untypedWarningsList;
        }
    }
}