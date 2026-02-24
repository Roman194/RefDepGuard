using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RefDepGuard.Data.Reference;

namespace RefDepGuard.Models.Reference
{
    public class ReferenceRuleWarnings
    {
        public List<ReferenceMatchWarning> ReferenceMatchWarningsList;
        public List<ProjectNotFoundWarning> ProjectNotFoundWarningsList;

        public ReferenceRuleWarnings(List<ReferenceMatchWarning> referenceMatchWarningsList, List<ProjectNotFoundWarning> projectNotFoundWarningsList)
        { 
            ReferenceMatchWarningsList = referenceMatchWarningsList;
            ProjectNotFoundWarningsList = projectNotFoundWarningsList;
        }
    }
}
