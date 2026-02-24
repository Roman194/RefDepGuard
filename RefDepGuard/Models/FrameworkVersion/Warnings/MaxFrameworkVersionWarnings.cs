using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RefDepGuard.Data.FrameworkVersion;

namespace RefDepGuard.Models.FrameworkVersion
{
    public class MaxFrameworkVersionWarnings
    {
        public List<MaxFrameworkVersionConflictWarning> MaxFrameworkVersionConflictWarningsList;
        public List<MaxFrameworkVersionReferenceConflictWarning> MaxFrameworkVersionReferenceConflictWarningsList;

        public MaxFrameworkVersionWarnings(List<MaxFrameworkVersionConflictWarning> maxFrameworkVersionConflictWarningsList, List<MaxFrameworkVersionReferenceConflictWarning> maxFrameworkVersionReferenceConflictWarningsList)
        {
            MaxFrameworkVersionConflictWarningsList = maxFrameworkVersionConflictWarningsList;
            MaxFrameworkVersionReferenceConflictWarningsList = maxFrameworkVersionReferenceConflictWarningsList;
        }
    }
}
