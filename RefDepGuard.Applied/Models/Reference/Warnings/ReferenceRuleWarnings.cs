using System;
using System.Collections.Generic;
using System.Text;

namespace RefDepGuard.Applied.Models.Reference.Warnings
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