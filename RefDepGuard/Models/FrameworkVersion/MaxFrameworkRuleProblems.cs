using System.Collections.Generic;
using RefDepGuard.Data.FrameworkVersion;

namespace RefDepGuard.Models.FrameworkVersion
{
    /// <summary>
    /// Shows a max_fr_ver/TargetFramework(-s) rule problems: comparatibility errors and untyped warnings
    /// </summary>
    public class MaxFrameworkRuleProblems
    {
        public List<FrameworkVersionComparatibilityError> FrameworkVersionComparabilityErrorList;
        public List<string> UntypedWarningsList;

        /// <param name="frameworkVersionComparabilityErrorList">list of FrameworkVersionComparatibilityError values</param>
        /// <param name="untypedWarningsList">list of string values</param>
        public MaxFrameworkRuleProblems(List<FrameworkVersionComparatibilityError> frameworkVersionComparabilityErrorList, List<string> untypedWarningsList)
        {
            FrameworkVersionComparabilityErrorList = frameworkVersionComparabilityErrorList;
            UntypedWarningsList = untypedWarningsList;
        }
    }
}