using RefDepGuard.CheckRules.Models.FrameworkVersion.Errors;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefDepGuard.CheckRules.Models.FrameworkVersion
{
    /// <summary>
    /// Shows a max_fr_ver/TargetFramework(-s) rule problems: comparatibility errors and untyped warnings
    /// </summary>
    public class MaxFrameworkRuleProblems
    {
        public List<FrameworkVersionComparabilityError> FrameworkVersionComparabilityErrorList;
        public List<string> UntypedWarningsList;

        /// <param name="frameworkVersionComparabilityErrorList">list of FrameworkVersionComparatibilityError values</param>
        /// <param name="untypedWarningsList">list of string values</param>
        public MaxFrameworkRuleProblems(List<FrameworkVersionComparabilityError> frameworkVersionComparabilityErrorList, List<string> untypedWarningsList)
        {
            FrameworkVersionComparabilityErrorList = frameworkVersionComparabilityErrorList;
            UntypedWarningsList = untypedWarningsList;
        }
    }
}