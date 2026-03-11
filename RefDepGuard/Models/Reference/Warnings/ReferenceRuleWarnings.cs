using System.Collections.Generic;
using RefDepGuard.Data.Reference;

namespace RefDepGuard.Models.Reference
{
    /// <summary>
    /// It's an abstraction on the all extention reference rule warnings
    /// </summary>
    public class ReferenceRuleWarnings
    {
        public List<ReferenceMatchWarning> ReferenceMatchWarningsList;
        public List<ProjectNotFoundWarning> ProjectNotFoundWarningsList;

        /// <param name="referenceMatchWarningsList">list of ReferenceMatchWarning values</param>
        /// <param name="projectNotFoundWarningsList">list of ProjectNotFoundWarning varnings</param>
        public ReferenceRuleWarnings(List<ReferenceMatchWarning> referenceMatchWarningsList, List<ProjectNotFoundWarning> projectNotFoundWarningsList)
        { 
            ReferenceMatchWarningsList = referenceMatchWarningsList;
            ProjectNotFoundWarningsList = projectNotFoundWarningsList;
        }
    }
}