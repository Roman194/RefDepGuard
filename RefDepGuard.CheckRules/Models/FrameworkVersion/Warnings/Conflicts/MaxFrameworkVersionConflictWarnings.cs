using System;
using System.Collections.Generic;
using System.Text;

namespace RefDepGuard.CheckRules.Models.FrameworkVersion.Warnings.Conflicts
{
    /// <summary>
    /// Shows a union conflict max_fr_ver warnings
    /// </summary>
    public class MaxFrameworkVersionConflictWarnings
    {
        public List<MaxFrameworkVersionConflictWarning> MaxFrameworkVersionConflictWarningsList;
        public List<MaxFrameworkVersionReferenceConflictWarning> MaxFrameworkVersionReferenceConflictWarningsList;

        /// <param name="maxFrameworkVersionConflictWarningsList">list of MaxFrameworkVersionConflictWarning values</param>
        /// <param name="maxFrameworkVersionReferenceConflictWarningsList">list of MaxFrameworkVersionReferenceConflictWarning values</param>
        public MaxFrameworkVersionConflictWarnings(List<MaxFrameworkVersionConflictWarning> maxFrameworkVersionConflictWarningsList, List<MaxFrameworkVersionReferenceConflictWarning> maxFrameworkVersionReferenceConflictWarningsList)
        {
            MaxFrameworkVersionConflictWarningsList = maxFrameworkVersionConflictWarningsList;
            MaxFrameworkVersionReferenceConflictWarningsList = maxFrameworkVersionReferenceConflictWarningsList;
        }
    }
}