using System.Collections.Generic;
using RefDepGuard.Data.FrameworkVersion;

namespace RefDepGuard.Models.FrameworkVersion
{
    /// <summary>
    /// Shows a union conflict max_fr_ver warnings
    /// </summary>
    public class MaxFrameworkVersionWarnings
    {
        public List<MaxFrameworkVersionConflictWarning> MaxFrameworkVersionConflictWarningsList;
        public List<MaxFrameworkVersionReferenceConflictWarning> MaxFrameworkVersionReferenceConflictWarningsList;

        /// <param name="maxFrameworkVersionConflictWarningsList">list of MaxFrameworkVersionConflictWarning values</param>
        /// <param name="maxFrameworkVersionReferenceConflictWarningsList">list of MaxFrameworkVersionReferenceConflictWarning values</param>
        public MaxFrameworkVersionWarnings(List<MaxFrameworkVersionConflictWarning> maxFrameworkVersionConflictWarningsList, List<MaxFrameworkVersionReferenceConflictWarning> maxFrameworkVersionReferenceConflictWarningsList)
        {
            MaxFrameworkVersionConflictWarningsList = maxFrameworkVersionConflictWarningsList;
            MaxFrameworkVersionReferenceConflictWarningsList = maxFrameworkVersionReferenceConflictWarningsList;
        }
    }
}